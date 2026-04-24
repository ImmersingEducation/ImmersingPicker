using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ImmersingPicker.Core.Models;
using Serilog;

namespace ImmersingPicker.Services.Services;

/// <summary>
/// 下载管理器 - 负责文件下载和进度报告
/// </summary>
public class DownloadManager
{
    private static readonly DownloadManager _instance = new();
    public static DownloadManager Instance => _instance;

    private static readonly ILogger _logger = Log.ForContext<DownloadManager>();
    private readonly HttpClient _httpClient;
    private CancellationTokenSource? _currentDownloadCts;

    private DownloadManager()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(30); // 大文件下载需要更长时间
    }

    /// <summary>
    /// 下载文件并报告进度
    /// </summary>
    /// <param name="url">下载链接</param>
    /// <param name="destinationFolder">目标文件夹</param>
    /// <param name="fileName">文件名</param>
    /// <param name="progress">进度报告</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>下载完成的文件路径</returns>
    public async Task<string> DownloadFileAsync(
        string url,
        string destinationFolder,
        string fileName,
        IProgress<DownloadProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _currentDownloadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var cts = _currentDownloadCts;

        try
        {
            _logger.Information("开始下载文件: {Url}", url);

            // 确保目标文件夹存在
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            var filePath = Path.Combine(destinationFolder, fileName);
            var progressInfo = new DownloadProgressInfo
            {
                Status = DownloadStatus.Pending,
                StatusMessage = "准备下载..."
            };
            progress?.Report(progressInfo);

            // 获取文件大小
            using var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
            var headResponse = await _httpClient.SendAsync(headRequest, cts.Token);
            headResponse.EnsureSuccessStatusCode();

            var totalBytes = headResponse.Content.Headers.ContentLength ?? -1;
            progressInfo.TotalBytesToReceive = totalBytes;

            // 开始下载
            using var response = await _httpClient.GetAsync(
                url, 
                HttpCompletionOption.ResponseHeadersRead, 
                cts.Token);

            response.EnsureSuccessStatusCode();

            progressInfo.Status = DownloadStatus.Downloading;
            progressInfo.StatusMessage = "下载中...";
            progress?.Report(progressInfo);

            // 下载文件
            await using var contentStream = await response.Content.ReadAsStreamAsync(cts.Token);
            await using var fileStream = new FileStream(
                filePath, 
                FileMode.Create, 
                FileAccess.Write, 
                FileShare.None, 
                8192, 
                useAsync: true);

            var buffer = new byte[8192];
            long totalBytesRead = 0;
            var lastReportTime = DateTime.UtcNow;
            var lastBytesRead = 0L;

            while (true)
            {
                cts.Token.ThrowIfCancellationRequested();

                var bytesRead = await contentStream.ReadAsync(buffer, cts.Token);
                if (bytesRead == 0)
                    break;

                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cts.Token);
                totalBytesRead += bytesRead;

                // 每秒报告一次进度
                var now = DateTime.UtcNow;
                if ((now - lastReportTime).TotalSeconds >= 1.0)
                {
                    var bytesPerSecond = (totalBytesRead - lastBytesRead) / (now - lastReportTime).TotalSeconds;
                    
                    progressInfo.BytesReceived = totalBytesRead;
                    progressInfo.ProgressPercentage = totalBytes > 0 
                        ? (double)totalBytesRead / totalBytes * 100 
                        : 0;
                    progressInfo.BytesPerSecond = bytesPerSecond;
                    progressInfo.StatusMessage = totalBytes > 0
                        ? $"下载中... {progressInfo.ProgressPercentage:F1}% ({FormatBytes(bytesPerSecond)}/s)"
                        : $"下载中... ({FormatBytes(totalBytesRead)})";

                    progress?.Report(progressInfo);

                    lastReportTime = now;
                    lastBytesRead = totalBytesRead;
                }
            }

            await fileStream.FlushAsync(cts.Token);

            // 下载完成
            progressInfo.Status = DownloadStatus.Completed;
            progressInfo.ProgressPercentage = 100;
            progressInfo.BytesReceived = totalBytesRead;
            progressInfo.StatusMessage = "下载完成!";
            progressInfo.DownloadedFilePath = filePath;
            progress?.Report(progressInfo);

            _logger.Information("文件下载完成: {FilePath}, 大小: {Size}", filePath, FormatBytes(totalBytesRead));
            return filePath;
        }
        catch (OperationCanceledException)
        {
            _logger.Warning("下载已取消");
            progress?.Report(new DownloadProgressInfo
            {
                Status = DownloadStatus.Cancelled,
                StatusMessage = "下载已取消"
            });
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "下载文件时发生错误");
            progress?.Report(new DownloadProgressInfo
            {
                Status = DownloadStatus.Failed,
                StatusMessage = $"下载失败: {ex.Message}"
            });
            throw;
        }
    }

    /// <summary>
    /// 取消当前下载
    /// </summary>
    public void CancelDownload()
    {
        _currentDownloadCts?.Cancel();
        _logger.Information("已取消下载");
    }

    /// <summary>
    /// 获取临时下载目录
    /// </summary>
    public static string GetTempDownloadPath()
    {
        var tempFolder = Path.Combine(Path.GetTempPath(), "ImmersingPicker_Updates");
        return tempFolder;
    }

    /// <summary>
    /// 格式化字节数为可读字符串
    /// </summary>
    private static string FormatBytes(double bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
