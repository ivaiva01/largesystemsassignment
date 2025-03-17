using System.Diagnostics;
using Application;

namespace Infrastructure;

public class TracingService : ITracingService
{
    private readonly ActivitySource _activitySource;

    public TracingService()
    {
        _activitySource = new ActivitySource("IndexingService");
    }

    public Activity? StartActivity(string name)
    {
        var activity = _activitySource.StartActivity(name);
        return activity;
    }

    public void StopActivity(Activity? activity, bool hasError = false)
    {
        if (activity == null) return;

        if (hasError)
        {
            activity.SetTag("error", true);
        }
        
        activity.Stop();
    }
}