using ServiceLocator.Discovery.Option;
using ServiceLocator.Discovery.Service;
using UptimeKuma.Pushr.Services.HostedServices.StatusClient;
using UptimeKuma.Pushr.Services.HostedServices.TaskRunner;
using UptimeKuma.Pushr.Services.MonitorStore;
using UptimeKuma.Pushr.Services.TaskRunnerNotify;
using UptimeKuma.Pushr.Services.TaskStore;
using UptimeKuma.Pushr.TaskRunner;
using UptimeKuma.Pushr.TaskRunner.UiOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddCommandLine(args)
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", true);

builder.Services
    .UseServiceDiscovery()
    .FromAssembly(typeof(Program).Assembly)
    .DiscoverOptions(builder.Configuration)
    .FromAssembly(typeof(Program).Assembly)
    .LocateServices()
    .AddHostedService<TaskRunnerService>()
    .AddHostedService<KumaPushClientService>();

var app = builder.Build();
app.MapStaticAssets();

if (builder.Configuration.GetValue<bool>("PUSHR_ENABLE_UI") == true)
{
    // GET /api/monitor-types  — list available monitor types
    app.MapGet("/api/monitor-types", (IMonitorStoreService ms) =>
        ms.GetTasks().Select(m => new
        {
            id = m.Id,
            name = m.Name,
            description = m.Description,
            isSupported = m.IsSupported,
            notSupportedReason = m.NotSupportedReason,
            options = m.GetOptionsTemplate().Select(o => new
            {
                key = o.Key,
                name = o.Name,
                description = o.Description,
                type = OptionType(o),
                defaultValue = o.Default,
                required = o.Required,
                suggestedValues = o.SuggestedValues ?? new Dictionary<string, string>()
            })
        })
    );

    // GET /api/tasks  — list tasks with current runtime state
    app.MapGet("/api/tasks", (ITaskStoreService ts, IMonitorStoreService ms, ITaskRunnerNotifyService ns) =>
        ts.Tasks.Select(t =>
        {
            var monitor = ms.FindTask(t.ReportableMonitorId);
            var stateEntry = ns.States?.FirstOrDefault(s => s.Data.Id == t.Id);
            var state = stateEntry?.LastState?.State ?? MonitorState.Unknown;
            var brokenInfo = stateEntry?.LastState?.BrokenInfoText;
            var opts = monitor?.GetOptionsTemplate().ToList() ?? new();
            if (t.Data != null) t.PopulateUiOptions(opts);
            var fields = opts.ToDictionary(o => o.Key, o => o.Value ?? "");
            return new
            {
                id = t.Id,
                title = t.Title,
                monitorTypeId = t.ReportableMonitorId,
                monitorTypeName = monitor?.Name ?? "Unknown",
                intervalSeconds = (int)t.Interval.TotalSeconds,
                pushUrl = t.PushUrl,
                disabled = t.Disabled,
                state = state.ToString(),
                brokenInfoText = brokenInfo,
                fields,
                due = stateEntry.Value.LastExecutionDate
            };
        })
    );

    // POST /api/tasks  — create a new task
    app.MapPost("/api/tasks", async (CreateUpdateTaskRequest req, ITaskStoreService ts, IMonitorStoreService ms) =>
    {
        var monitor = ms.FindTask(req.MonitorTypeId);
        if (monitor is null) return Results.NotFound();

        var opts = monitor.GetOptionsTemplate().ToList();
        foreach (var opt in opts)
            if (req.Fields.TryGetValue(opt.Key, out var val)) opt.Value = val;

        var task = new MonitorData { Id = Guid.NewGuid().ToString("D"), ReportableMonitorId = req.MonitorTypeId };
        task.InfuseUiOptions(opts);
        await ts.AddTask(task);
        return Results.Ok();
    });

    // PUT /api/tasks/{id}  — update an existing task
    app.MapPut("/api/tasks/{id}", async (string id, CreateUpdateTaskRequest req, ITaskStoreService ts, IMonitorStoreService ms) =>
    {
        var existing = ts.Tasks.FirstOrDefault(t => t.Id == id);
        if (existing is null) return Results.NotFound();
        var monitor = ms.FindTask(existing.ReportableMonitorId);
        if (monitor is null) return Results.NotFound();

        var opts = monitor.GetOptionsTemplate().ToList();
        foreach (var opt in opts)
            if (req.Fields.TryGetValue(opt.Key, out var val)) opt.Value = val;

        existing.InfuseUiOptions(opts);
        await ts.UpdateTask(existing);
        return Results.Ok();
    });

    // DELETE /api/tasks/{id}
    app.MapDelete("/api/tasks/{id}", async (string id, ITaskStoreService ts) =>
    {
        if (!ts.Tasks.Any(t => t.Id == id)) return Results.NotFound();
        await ts.RemoveTask(id);
        return Results.NoContent();
    });

    // POST /api/tasks/{id}/toggle  — flip disabled flag
    app.MapPost("/api/tasks/{id}/toggle", async (string id, ITaskStoreService ts) =>
    {
        var existing = ts.Tasks.FirstOrDefault(t => t.Id == id);
        if (existing is null) return Results.NotFound();
        existing.Disabled = !existing.Disabled;
        await ts.UpdateTask(existing);
        return Results.Ok(new { disabled = existing.Disabled });
    });

    app.MapFallbackToFile("index.html");
}

await app.RunAsync();

// ---- helpers ----
static string OptionType(IUiOption o) => o.GetType().Name switch
{
    nameof(IntUiOption) => "number",
    nameof(UrlUiOption) => "url",
    nameof(IpUiOption) => "ip",
    nameof(YesNoUiOption) => "bool",
    nameof(OfListUiOption) => "list",
    _ => "text"
};

record CreateUpdateTaskRequest(string MonitorTypeId, IDictionary<string, string> Fields);
