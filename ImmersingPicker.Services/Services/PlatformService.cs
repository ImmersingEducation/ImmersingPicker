using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Services.Services;

public class PlatformServices
{
    public static PlatformServices Instance { get; set; } = new PlatformServices();

    public PlatformServices()
    {
        AppSettings.Instance.LaunchOnSystemStartChanged += OnLaunchOnSystemStartChanged;
    }

    private void OnLaunchOnSystemStartChanged(bool state)
    {
        AutoStart(state);
    }

    public bool AutoStart(bool state)
    {
        if (state)
        { 
            return EnableAutoStart();
        }
        else
        {
            DisableAutoStart();
            return true;
        }
    }

    public bool EnableAutoStart()
    {
        if (OperatingSystem.IsWindows())
        {
            return EnableAutoStartWindows();
        }
        return false;
    }

    public void DisableAutoStart()
    {
        if (OperatingSystem.IsWindows())
        {
            DisableAutoStartWindows();
        }
    }

    private bool EnableAutoStartWindows()
    {
        string GetStartupFolderPath() => Environment.GetFolderPath(Environment.SpecialFolder.Startup);

        void ExecutePowerShellScript(string script)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas"
            };

            using (var process = Process.Start(processStartInfo))
            {
                process?.WaitForExit();
                if (process?.ExitCode != 0)
                {
                    throw new Exception($"PowerShell script execution failed. Exit code: {process?.ExitCode}");
                }
            }
        }

        try
        {
            string startupFolderPath = GetStartupFolderPath();
            string shortcutPath = Path.Combine(startupFolderPath, "ImmersingPicker.lnk");

            // 获取当前可执行文件的完整路径
            string exePath = Assembly.GetEntryAssembly()?.Location ?? Path.GetFullPath("./ImmersingPicker.exe");

            // 检查是否是dll文件，如果是，尝试找到对应的exe文件
            if (exePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                string potentialExePath = Path.ChangeExtension(exePath, ".exe");
                if (File.Exists(potentialExePath))
                {
                    exePath = potentialExePath;
                }
                else
                {
                    // 如果没有找到对应的exe文件，尝试在当前目录查找
                    potentialExePath = Path.GetFullPath("./ImmersingPicker.exe");
                    if (File.Exists(potentialExePath))
                    {
                        exePath = potentialExePath;
                    }
                }
            }

            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException($"Executable file not found at: {exePath}");
            }

            // 构建PowerShell脚本，使用-Command参数而不是-File
            string script = $@"
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut('{shortcutPath.Replace("'", "''")}')
$Shortcut.TargetPath = '{exePath.Replace("'", "''")}'
$Shortcut.WorkingDirectory = '{Path.GetDirectoryName(exePath)?.Replace("'", "''")}'
$Shortcut.Description = 'ImmersingPicker'
$Shortcut.Save()
";
            ExecutePowerShellScript(script);
            return true;
        }
        catch (Exception e)
        {
            // 记录异常但不抛出，返回false
            Debug.WriteLine($"Failed to create shortcut: {e.Message}");
            return false;
        }
    }

    public void DisableAutoStartWindows()
    {
        string GetStartupFolderPath() => Environment.GetFolderPath(Environment.SpecialFolder.Startup);

        string shortcutPath = Path.Combine(GetStartupFolderPath(), "ImmersingPicker.lnk");
        if (File.Exists(shortcutPath))
        {
            File.Delete(shortcutPath);
        }
    }

    public bool CreateDesktopShortcut()
    {
        if (OperatingSystem.IsWindows())
        {
            return CreateDesktopShortcutWindows();
        }
        return false;
    }

    public bool CreateStartMenuShortcut()
    {
        if (OperatingSystem.IsWindows())
        {
            return CreateStartMenuShortcutWindows();
        }
        return false;
    }

    private bool CreateDesktopShortcutWindows()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string shortcutPath = Path.Combine(desktopPath, "ImmersingPicker.lnk");
        return CreateShortcut(shortcutPath);
    }

    private bool CreateStartMenuShortcutWindows()
    {
        string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        string programMenuPath = Path.Combine(startMenuPath, "Programs");
        string shortcutPath = Path.Combine(programMenuPath, "ImmersingPicker.lnk");
        return CreateShortcut(shortcutPath);
    }

    private bool CreateShortcut(string shortcutPath)
    {
        void ExecutePowerShellScript(string script)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas"
            };

            using (var process = Process.Start(processStartInfo))
            {
                process?.WaitForExit();
                if (process?.ExitCode != 0)
                {
                    throw new Exception($"PowerShell script execution failed. Exit code: {process?.ExitCode}");
                }
            }
        }

        try
        {
            string exePath = Assembly.GetEntryAssembly()?.Location ?? Path.GetFullPath("./ImmersingPicker.exe");

            if (exePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                string potentialExePath = Path.ChangeExtension(exePath, ".exe");
                if (File.Exists(potentialExePath))
                {
                    exePath = potentialExePath;
                }
                else
                {
                    potentialExePath = Path.GetFullPath("./ImmersingPicker.exe");
                    if (File.Exists(potentialExePath))
                    {
                        exePath = potentialExePath;
                    }
                }
            }

            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException($"Executable file not found at: {exePath}");
            }

            string script = $@"
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut('{shortcutPath.Replace("'", "''")}')
$Shortcut.TargetPath = '{exePath.Replace("'", "''")}'
$Shortcut.WorkingDirectory = '{Path.GetDirectoryName(exePath)?.Replace("'", "''")}'
$Shortcut.Description = 'ImmersingPicker'
$Shortcut.Save()
";
            ExecutePowerShellScript(script);
            return true;
        }
        catch (Exception e)
        {
            // 记录异常但不抛出，返回false
            Debug.WriteLine($"Failed to create shortcut: {e.Message}");
            return false;
        }
    }
}