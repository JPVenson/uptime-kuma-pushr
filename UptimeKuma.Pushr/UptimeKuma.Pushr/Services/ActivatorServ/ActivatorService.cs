using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ServiceLocator.Attributes;

namespace UptimeKuma.Pushr.Services.ActivatorServ
{
	[SingletonService()]
	public class ActivatorService
	{
		private readonly IServiceProvider _serviceProvider;

		public ActivatorService(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		///		Injects services from the <see cref="IServiceProvider"/> into the method.
		///		if fixedValues contain any number of values, those values will be injected first in order of presence
		/// </summary>
		/// <param name="method">The delegate to inject</param>
		/// <param name="target">The target of the delegate</param>
		/// <param name="fixedValues"></param>
		/// <returns>if the delegate returns any value it is returned</returns>
		public object ActivateMethod(Delegate method, object target, params object[] fixedValues)
		{
			return ActivateMethod(method.Method, target, fixedValues);
		}


		/// <summary>
		///		Injects services from the <see cref="IServiceProvider"/> into the method.
		///		if fixedValues contain any number of values, those values will be injected first in order of presence
		/// </summary>
		/// <param name="method">The delegate to inject</param>
		/// <param name="target">The target of the delegate</param>
		/// <param name="fixedValues"></param>
		/// <returns>if the delegate returns any value it is returned</returns>
		public object ActivateMethod(MethodInfo method, object target, params object[] fixedValues)
		{
			var scope = _serviceProvider.CreateScope();
			var constructorArgs = method.GetParameters();
			var services = new object[constructorArgs.Length];
			Array.Copy(fixedValues, services, fixedValues.Length);

			for (var index = fixedValues.Length; index < constructorArgs.Length; index++)
			{
				var constructorArg = constructorArgs[index];
				services[index] = scope.ServiceProvider.GetService(constructorArg.ParameterType);
			}

			return method.Invoke(target, services);
		}


		/// <summary>
		///		Injects services from the <see cref="IServiceProvider"/> into the method.
		///		if fixedValues contain any number of values, those values will be injected first in order of presence
		/// </summary>
		/// <param name="method">The delegate to inject</param>
		/// <param name="target">The target of the delegate</param>
		/// <param name="fixedValues"></param>
		/// <returns>if the delegate returns any value it is returned</returns>
		public TResult ActivateMethod<TResult>(MethodInfo method, object target, params object[] fixedValues)
		{
			return (TResult)ActivateMethod(method, target, fixedValues);
		}


		/// <summary>
		///		Injects services from the <see cref="IServiceProvider"/> into the method.
		///		if fixedValues contain any number of values, those values will be injected first in order of presence
		/// </summary>
		/// <param name="method">The delegate to inject</param>
		/// <param name="target">The target of the delegate</param>
		/// <param name="fixedValues"></param>
		public void ActivateVoidMethod(MethodBase method, object target, params object[] fixedValues)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var constructorArgs = method.GetParameters();
				var services = new object[constructorArgs.Length];
				Array.Copy(fixedValues, services, fixedValues.Length);

				for (var index = fixedValues.Length; index < constructorArgs.Length; index++)
				{
					var constructorArg = constructorArgs[index];
					services[index] = scope.ServiceProvider.GetService(constructorArg.ParameterType);
				}
				method.Invoke(target, services);
			}

		}

		public T ActivateType<T>(params object[] fixedValues) where T : class
		{
			return (T)ActivateType(typeof(T), fixedValues);
		}

		public T ActivateType<T>(Type type, params object[] fixedValues) where T : class
		{
			if (!typeof(T).IsAssignableFrom(type))
			{
				throw new NotSupportedException($"Cannot cast type '{type}' into '{typeof(T)}' for activation");
			}

			return ActivateType(type, fixedValues) as T;
		}

		/// <summary>
		///		Tries to create a new instance of <see cref="type"/> by injecting its parameter first from <see cref="fixedValues"/> and then lookup from <see cref="IServiceProvider"/>
		/// </summary>
		/// <param name="type"></param>
		/// <param name="fixedValues"></param>
		/// <returns></returns>
		public object ActivateType(Type type, params object[] fixedValues)
		{

			using (var scope = _serviceProvider.CreateScope())
			{
				if (type.IsInterface)
				{
					return scope.ServiceProvider.GetService(type);
				}

				var firstOrDefault = type.GetConstructors().FirstOrDefault();
				if (firstOrDefault == null)
				{
					return Activator.CreateInstance(type);
				}

				var constructorArgs = firstOrDefault.GetParameters();
				var services = new object[constructorArgs.Length];
				Array.Copy(fixedValues, services, fixedValues.Length);

				for (var index = fixedValues.Length; index < constructorArgs.Length; index++)
				{
					var constructorArg = constructorArgs[index];
					services[index] = scope.ServiceProvider.GetService(constructorArg.ParameterType);
				}
				return Activator.CreateInstance(type, services);
			}

		}
	}
}
