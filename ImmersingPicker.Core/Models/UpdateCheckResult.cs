namespace ImmersingPicker.Core.Models;

/// <summary>
/// 更新检查结果枚举
/// </summary>
public enum UpdateCheckResult
{
    /// <summary>
    /// 有可用更新
    /// </summary>
    UpdateAvailable,

    /// <summary>
    /// 已是最新版本
    /// </summary>
    NoUpdate,

    /// <summary>
    /// 检查失败
    /// </summary>
    CheckFailed,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled
}
