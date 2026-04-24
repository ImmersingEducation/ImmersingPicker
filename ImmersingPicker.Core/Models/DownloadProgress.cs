namespace ImmersingPicker.Core.Models;

/// <summary>
/// 下载进度信息
/// </summary>
public class DownloadProgressInfo
{
    /// <summary>
    /// 进度百分比 (0-100)
    /// </summary>
    public double ProgressPercentage { get; set; }

    /// <summary>
    /// 已下载字节
    /// </summary>
    public long BytesReceived { get; set; }

    /// <summary>
    /// 总字节数
    /// </summary>
    public long TotalBytesToReceive { get; set; }

    /// <summary>
    /// 下载速度 (字节/秒)
    /// </summary>
    public double BytesPerSecond { get; set; }

    /// <summary>
    /// 下载状态
    /// </summary>
    public DownloadStatus Status { get; set; }

    /// <summary>
    /// 状态消息
    /// </summary>
    public string? StatusMessage { get; set; }

    /// <summary>
    /// 下载的文件路径 (完成后)
    /// </summary>
    public string? DownloadedFilePath { get; set; }
}

/// <summary>
/// 下载状态枚举
/// </summary>
public enum DownloadStatus
{
    /// <summary>
    /// 等待中
    /// </summary>
    Pending,

    /// <summary>
    /// 下载中
    /// </summary>
    Downloading,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed,

    /// <summary>
    /// 已失败
    /// </summary>
    Failed,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled
}
