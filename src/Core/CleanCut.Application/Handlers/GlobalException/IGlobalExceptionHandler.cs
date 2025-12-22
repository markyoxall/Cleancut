using System;
using System.Threading.Tasks;

namespace CleanCut.Application.Handlers.GlobalException;

public enum ExceptionSource
{
    UIThread,
    AppDomain,
    TaskScheduler,
    Other
}

public interface IGlobalExceptionHandler
{
    Task HandleAsync(Exception ex, ExceptionSource source);
}
