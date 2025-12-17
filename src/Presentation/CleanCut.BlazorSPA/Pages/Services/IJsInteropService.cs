using System.Threading.Tasks;
using System.Threading;

namespace CleanCut.BlazorSPA.Pages.Services;

public interface IJsInteropService
{
    ValueTask<bool> ConfirmAsync(string message);
    ValueTask AlertAsync(string message);
}
