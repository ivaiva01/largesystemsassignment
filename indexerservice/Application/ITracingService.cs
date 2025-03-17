using System.Diagnostics;

namespace Application;

public interface ITracingService
{
    Activity? StartActivity(string name);
    void StopActivity(Activity? activity, bool hasError = false);
}