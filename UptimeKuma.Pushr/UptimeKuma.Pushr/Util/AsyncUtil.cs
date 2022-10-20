namespace UptimeKuma.Pushr.Util
{
	internal static class AsyncUtil
	{
		/// <summary>
		/// Creates a TPL Task that is marked as completed when a <see cref="WaitHandle"/> is signaled.
		/// </summary>
		/// <param name="handle">The handle whose signal triggers the task to be completed.</param>
		/// <returns>A Task that is completed after the handle is signaled.</returns>
		/// <remarks>
		/// There is a (brief) time delay between when the handle is signaled and when the task is marked as completed.
		/// </remarks>
		public static Task Await(this WaitHandle handle)
		{
			var tcs = new TaskCompletionSource<object>();
			var localVariableInitLock = new object();
			lock (localVariableInitLock)
			{
				RegisteredWaitHandle callbackHandle = null;
				callbackHandle = ThreadPool.RegisterWaitForSingleObject(
					handle,
					(state, timedOut) =>
					{
						tcs.SetResult(null);

						// We take a lock here to make sure the outer method has completed setting the local variable callbackHandle.
						lock (localVariableInitLock)
						{
							callbackHandle.Unregister(null);
						}
					},
					state: null,
					millisecondsTimeOutInterval: Timeout.Infinite,
					executeOnlyOnce: true);
			}

			return tcs.Task;
		}
	}
}
