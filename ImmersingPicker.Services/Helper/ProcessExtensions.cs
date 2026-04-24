using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ImmersingPicker.Services.Helper;

/// <summary>
/// Process 扩展方法
/// </summary>
public static class ProcessExtensions
{
    /// <summary>
    /// 异步等待进程退出
    /// </summary>
    public static async Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        
        // 注册进程退出事件
        process.EnableRaisingEvents = true;
        process.Exited += (sender, args) => tcs.TrySetResult(true);
        
        // 检查进程是否已经退出
        if (process.HasExited)
        {
            return;
        }

        // 等待退出或取消
        using var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
        await tcs.Task;
    }
}
