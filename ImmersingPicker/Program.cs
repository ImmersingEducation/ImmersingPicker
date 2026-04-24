using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ImmersingPicker.Services.Helper;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace ImmersingPicker;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                if (ex is TaskCanceledException or OperationCanceledException or ObjectDisposedException)
                {
                    return;
                }
                Log.Fatal(ex, "未处理的异常");
            }
        };

        var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        Directory.CreateDirectory(logDirectory);
        var logFileName = $"log-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt";
        var logFilePath = Path.Combine(logDirectory, logFileName);

        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Verbose()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Code
            )
#elif RELEASE
            .MinimumLevel.Information()
#else
#endif
            .WriteTo.File(
                path: logFilePath,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 5 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                shared: true,
                encoding: Encoding.UTF8
            )
            .CreateLogger();

        var appBuilder = BuildAvaloniaApp();

        try
        {
            appBuilder.StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex) when (ex is not TaskCanceledException and not OperationCanceledException)
        {
            Log.Fatal(ex, "遇到了无法回退的错误。");
        }
        finally
        {
            if (appBuilder.Instance is App app)
            {
                try
                {
                    app.Shutdown();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "应用程序关闭清理时发生错误");
                }
            }

            try
            {
                Log.CloseAndFlush();
            }
            catch
            {
            }
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

}