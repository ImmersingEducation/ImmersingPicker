using System;
using System.Text.Json.Serialization;

namespace ImmersingPicker.Core.Models;

/// <summary>
/// GitHub Release 更新信息
/// </summary>
public class UpdateInfo
{
    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 发布日期
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// 下载链接
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小 (字节)
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 更新日志 (Release Notes)
    /// </summary>
    public string ReleaseNotes { get; set; } = string.Empty;

    /// <summary>
    /// 是否强制更新
    /// </summary>
    public bool IsMandatory { get; set; }

    /// <summary>
    /// Release 名称
    /// </summary>
    public string ReleaseName { get; set; } = string.Empty;

    /// <summary>
    /// 是否为预发布版本
    /// </summary>
    public bool IsPrerelease { get; set; }

    /// <summary>
    /// 资产文件名
    /// </summary>
    public string AssetName { get; set; } = string.Empty;
}
