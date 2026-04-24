using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace ImmersingPicker.Services.Services;

/// <summary>
/// 更新安装器 - 负责安装下载更新
/// </summary>
public class UpdateInstaller
{
    private static readonly UpdateInstaller _instance = new();
    public static UpdateInstaller Instance => _instance;

    private static readonly ILogger _logger = Log.ForContext<UpdateInstaller>();

    private UpdateInstaller()
    {
    }

    /// <summary>
    /// 安装更新
    /// </summary>
    /// <param name="installerPath">安装文件路径</param>
    /// <param name="applicationFolder">应用程序文件夹</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功启动安装程序</returns>
    public async Task<bool> InstallUpdateAsync(
        string installerPath,
        string? applicationFolder = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(installerPath))
            {
                _logger.Error("安装文件不存在: {Path}", installerPath);
                return false;
            }

            var extension = Path.GetExtension(installerPath).ToLowerInvariant();

            _logger.Information("开始安装更新, 文件类型: {Extension}", extension);

            // 根据文件类型选择安装方式
            if (extension == ".exe" || extension == ".msi")
            {
                // 可执行安装文件
                return await RunInstallerAsync(installerPath, cancellationToken);
            }
            else if (extension == ".zip")
            {
                // 便携版 ZIP 文件
                return await InstallPortableVersionAsync(installerPath, applicationFolder, cancellationToken);
            }
            else
            {
                _logger.Error("不支持的文件类型: {Extension}", extension);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "安装更新时发生错误");
            return false;
        }
    }

    /// <summary>
    /// 运行安装程序
    /// </summary>
    private async Task<bool> RunInstallerAsync(string installerPath, CancellationToken cancellationToken)
    {
        try
        {
            _logger.Information("启动安装程序: {Path}", installerPath);

            // 备份当前版本
            var backupPath = CreateBackup();

            var processStartInfo = new ProcessStartInfo
            {
                FileName = installerPath,
                UseShellExecute = true,
                Verb = "runas" // 请求管理员权限
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                _logger.Error("无法启动安装程序");
                return false;
            }

            // 等待安装完成
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode == 0)
            {
                _logger.Information("安装程序完成运行, 退出码: {ExitCode}", process.ExitCode);
                
                // 清理临时文件
                CleanupTempFiles(installerPath);
                
                return true;
            }
            else
            {
                _logger.Warning("安装程序退出码: {ExitCode}, 尝试恢复备份", process.ExitCode);
                
                // 安装失败,尝试恢复备份
                if (!string.IsNullOrEmpty(backupPath))
                {
                    await RestoreBackupAsync(backupPath);
                }
                
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "运行安装程序时发生错误");
            return false;
        }
    }

    /// <summary>
    /// 安装便携版本
    /// </summary>
    private async Task<bool> InstallPortableVersionAsync(
        string zipPath,
        string? applicationFolder,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.Information("开始安装便携版本: {Path}", zipPath);

            // 如果没有指定应用文件夹,使用当前目录
            if (string.IsNullOrEmpty(applicationFolder))
            {
                applicationFolder = AppDomain.CurrentDomain.BaseDirectory;
            }

            // 创建备份
            var backupPath = CreateBackup();

            // 解压 ZIP 文件
            var tempExtractPath = Path.Combine(Path.GetTempPath(), "ImmersingPicker_Update_" + Guid.NewGuid().ToString("N"));
            
            await Task.Run(() =>
            {
                ZipFile.ExtractToDirectory(zipPath, tempExtractPath, true);
            }, cancellationToken);

            _logger.Information("ZIP 文件解压到: {Path}", tempExtractPath);

            // 复制文件到应用文件夹 (排除某些文件)
            await CopyFilesAsync(tempExtractPath, applicationFolder, cancellationToken);

            // 清理临时文件
            CleanupTempFiles(zipPath, tempExtractPath);

            _logger.Information("便携版本安装完成");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "安装便携版本时发生错误");
            return false;
        }
    }

    /// <summary>
    /// 创建当前版本备份
    /// </summary>
    private string? CreateBackup()
    {
        try
        {
            var appFolder = AppDomain.CurrentDomain.BaseDirectory;
            var backupFolder = Path.Combine(Path.GetTempPath(), "ImmersingPicker_Backup_" + Guid.NewGuid().ToString("N"));
            
            Directory.CreateDirectory(backupFolder);

            // 只备份关键文件
            var filesToBackup = new[]
            {
                "*.dll",
                "*.exe",
                "*.json",
                "*.pdb"
            };

            foreach (var pattern in filesToBackup)
            {
                var files = Directory.GetFiles(appFolder, pattern, SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    var destFile = Path.Combine(backupFolder, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }
            }

            _logger.Information("备份创建完成: {Path}", backupFolder);
            return backupFolder;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "创建备份时发生错误");
            return null;
        }
    }

    /// <summary>
    /// 恢复备份
    /// </summary>
    private async Task<bool> RestoreBackupAsync(string backupPath)
    {
        try
        {
            if (!Directory.Exists(backupPath))
            {
                _logger.Warning("备份文件夹不存在");
                return false;
            }

            var appFolder = AppDomain.CurrentDomain.BaseDirectory;
            
            _logger.Information("开始恢复备份: {Path}", backupPath);

            var files = Directory.GetFiles(backupPath, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var destFile = Path.Combine(appFolder, Path.GetFileName(file));
                
                // 等待文件可用
                await WaitForFileAsync(destFile, TimeSpan.FromSeconds(5));
                
                File.Copy(file, destFile, true);
            }

            _logger.Information("备份恢复完成");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "恢复备份时发生错误");
            return false;
        }
    }

    /// <summary>
    /// 复制文件 (异步)
    /// </summary>
    private async Task CopyFilesAsync(string sourceFolder, string destinationFolder, CancellationToken cancellationToken)
    {
        var files = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
        
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(sourceFolder, file);
            var destFile = Path.Combine(destinationFolder, relativePath);

            var destDirectory = Path.GetDirectoryName(destFile);
            if (!string.IsNullOrEmpty(destDirectory) && !Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
            }

            // 跳过正在使用的文件
            if (IsFileInUse(destFile))
            {
                _logger.Warning("文件正在使用中,跳过: {File}", destFile);
                continue;
            }

            try
            {
                await WaitForFileAsync(file, TimeSpan.FromSeconds(2));
                File.Copy(file, destFile, true);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "复制文件失败: {File}", destFile);
            }
        }
    }

    /// <summary>
    /// 检查文件是否被占用
    /// </summary>
    private bool IsFileInUse(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
    }

    /// <summary>
    /// 等待文件可用
    /// </summary>
    private async Task WaitForFileAsync(string filePath, TimeSpan timeout)
    {
        var startTime = DateTime.UtcNow;
        
        while (DateTime.UtcNow - startTime < timeout)
        {
            if (!File.Exists(filePath))
            {
                await Task.Delay(100);
                continue;
            }

            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return; // 文件可用
            }
            catch (IOException)
            {
                await Task.Delay(100);
            }
        }
    }

    /// <summary>
    /// 清理临时文件
    /// </summary>
    private void CleanupTempFiles(params string[] paths)
    {
        foreach (var path in paths)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    _logger.Debug("已删除临时文件: {Path}", path);
                }
                else if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                    _logger.Debug("已删除临时文件夹: {Path}", path);
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "清理临时文件失败: {Path}", path);
            }
        }
    }

    /// <summary>
    /// 重启应用程序
    /// </summary>
    public void RestartApplication()
    {
        try
        {
            _logger.Information("正在重启应用程序...");
            
            var processStartInfo = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath ?? Environment.CommandLine,
                UseShellExecute = true
            };

            Process.Start(processStartInfo);
            Process.GetCurrentProcess().CloseMainWindow();
            Process.GetCurrentProcess().WaitForExit(3000);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "重启应用程序时发生错误");
        }
    }
}
