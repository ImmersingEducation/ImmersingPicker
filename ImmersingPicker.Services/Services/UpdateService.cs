using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services.Helper;
using Serilog;

namespace ImmersingPicker.Services.Services;

/// <summary>
/// GitHub 更新服务 - 负责从 GitHub Releases 获取版本信息
/// </summary>
public class UpdateService
{
    private static readonly UpdateService _instance = new();
    public static UpdateService Instance => _instance;

    private static readonly ILogger _logger = Log.ForContext<UpdateService>();
    private readonly HttpClient _httpClient;

    // GitHub API 端点
    private const string GitHubOwner = "ImmersingEducation";
    private const string GitHubRepo = "ImmersingPicker";
    private const string GitHubApiBaseUrl = "https://api.github.com";

    private UpdateService()
    {
        var handler = new HttpClientHandler
        {
            SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            ServerCertificateCustomValidationCallback = CertificateValidationCallback
        };
        
        _httpClient = new HttpClient(handler);
        
        _httpClient.DefaultRequestHeaders.UserAgent.Clear();
        _httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("ImmersingPicker", VersionHelper.GetCurrentVersion()?.ToString() ?? "0.0.0.0"));
        
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    private bool CertificateValidationCallback(
        HttpRequestMessage request,
        X509Certificate2? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        if (certificate == null)
        {
            _logger.Warning("SSL证书验证失败: 证书为空");
            return false;
        }

        var targetHost = request.RequestUri?.Host ?? "unknown";
        
        if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors))
        {
            _logger.Warning("SSL证书链验证失败，尝试手动验证证书链");
            
            var chainPolicy = new X509ChainPolicy
            {
                VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority,
                RevocationMode = X509RevocationMode.NoCheck
            };
            
            var newChain = new X509Chain();
            newChain.ChainPolicy = chainPolicy;
            
            if (newChain.Build(certificate))
            {
                _logger.Information("证书链手动验证成功: {Subject}", certificate.Subject);
                return true;
            }
            
            _logger.Warning("证书链手动验证失败");
        }

        if (targetHost.EndsWith("github.com", StringComparison.OrdinalIgnoreCase) ||
            targetHost.EndsWith("githubusercontent.com", StringComparison.OrdinalIgnoreCase))
        {
            _logger.Information("允许GitHub域名的证书: {Host}", targetHost);
            return true;
        }

        _logger.Warning("SSL证书验证失败: {Errors}, Host: {Host}", sslPolicyErrors, targetHost);
        return false;
    }

    /// <summary>
    /// 检查是否有可用更新
    /// </summary>
    /// <param name="allowPrerelease">是否允许预发布版本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>检查结果和更新信息</returns>
    public async Task<(UpdateCheckResult Result, UpdateInfo? UpdateInfo)> CheckForUpdatesAsync(
        bool allowPrerelease = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("开始检查更新...");

            UpdateInfo? updateInfo;

            if (allowPrerelease)
            {
                // 获取所有发布版本,找到最新的
                updateInfo = await GetLatestReleaseIncludingPrereleasesAsync(cancellationToken);
            }
            else
            {
                // 仅获取最新稳定版本
                updateInfo = await GetLatestStableReleaseAsync(cancellationToken);
            }

            if (updateInfo == null)
            {
                _logger.Warning("无法获取发布信息");
                return (UpdateCheckResult.CheckFailed, null);
            }

            // 检查用户是否跳过了此版本
            if (updateInfo.Version == AppSettings.Instance.SkippedUpdateVersion)
            {
                _logger.Information("用户已跳过此版本: {Version}", updateInfo.Version);
                return (UpdateCheckResult.Cancelled, null);
            }

            // 比较版本
            var currentVersion = VersionHelper.GetCurrentVersion();
            if (currentVersion == null)
            {
                _logger.Warning("无法获取当前版本");
                return (UpdateCheckResult.CheckFailed, null);
            }

            var comparison = VersionHelper.CompareVersions(currentVersion, updateInfo.Version);
            if (!comparison.HasValue)
            {
                _logger.Warning("版本比较失败");
                return (UpdateCheckResult.CheckFailed, null);
            }

            if (comparison.Value < 0)
            {
                _logger.Information("发现新版本: {Version}", updateInfo.Version);
                return (UpdateCheckResult.UpdateAvailable, updateInfo);
            }

            _logger.Information("当前已是最新版本");
            return (UpdateCheckResult.NoUpdate, null);
        }
        catch (TaskCanceledException ex)
        {
            _logger.Warning(ex, "检查更新超时或已取消");
            return (UpdateCheckResult.Cancelled, null);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "检查更新时发生错误");
            return (UpdateCheckResult.CheckFailed, null);
        }
    }

    /// <summary>
    /// 获取最新稳定版本
    /// </summary>
    private async Task<UpdateInfo?> GetLatestStableReleaseAsync(CancellationToken cancellationToken)
    {
        try
        {
            var url = $"{GitHubApiBaseUrl}/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";
            _logger.Debug("请求 GitHub API: {Url}", url);
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.Warning("GitHub API 请求失败: {StatusCode} - {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                
                // 如果是 404，可能是仓库不存在或没有 release
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.Warning("GitHub 仓库或 Release 不存在");
                    return null;
                }
                
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.Debug("GitHub API 响应: {Content}", content);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var release = JsonSerializer.Deserialize<GitHubReleaseResponse>(content, options);
            
            if (release == null)
            {
                _logger.Warning("GitHub API 返回空响应");
                return null;
            }

            return ParseGitHubRelease(release);
        }
        catch (HttpRequestException ex)
        {
            _logger.Error(ex, "网络请求失败");
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "获取最新稳定版本失败");
            return null;
        }
    }

    /// <summary>
    /// 获取最新版本 (包括预发布版本)
    /// </summary>
    private async Task<UpdateInfo?> GetLatestReleaseIncludingPrereleasesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var url = $"{GitHubApiBaseUrl}/repos/{GitHubOwner}/{GitHubRepo}/releases?per_page=1";
            _logger.Debug("请求 GitHub API: {Url}", url);
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.Warning("GitHub API 请求失败: {StatusCode} - {ReasonPhrase}",
                    response.StatusCode, response.ReasonPhrase);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.Debug("GitHub API 响应: {Content}", content);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var releases = JsonSerializer.Deserialize<List<GitHubReleaseResponse>>(content, options);

            if (releases == null || releases.Count == 0)
            {
                _logger.Warning("GitHub API 返回空响应");
                return null;
            }

            // 找到最新版本
            var latestRelease = releases.First();
            return ParseGitHubRelease(latestRelease);
        }
        catch (HttpRequestException ex)
        {
            _logger.Error(ex, "网络请求失败");
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "获取最新版本失败");
            return null;
        }
    }

    /// <summary>
    /// 解析 GitHub Release 响应
    /// </summary>
    private UpdateInfo ParseGitHubRelease(GitHubReleaseResponse response)
    {
        var updateInfo = new UpdateInfo
        {
            Version = CleanTagName(response.TagName),
            ReleaseName = response.Name ?? response.TagName,
            ReleaseDate = response.PublishedAt,
            IsPrerelease = response.Prerelease,
            ReleaseNotes = response.Body ?? string.Empty,
            IsMandatory = false
        };

        // 查找合适的下载资产
        var asset = response.Assets
            .FirstOrDefault(a => a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                                 a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

        if (asset != null)
        {
            updateInfo.DownloadUrl = asset.BrowserDownloadUrl;
            updateInfo.FileSize = asset.Size;
            updateInfo.AssetName = asset.Name;
            _logger.Debug("找到下载资产: {Name}, URL: {Url}, Size: {Size}", 
                asset.Name, asset.BrowserDownloadUrl, asset.Size);
        }
        else
        {
            _logger.Warning("未找到合适的下载资产");
        }

        _logger.Debug("解析发布信息: {Version}, 预发布: {IsPrerelease}, 资产: {AssetName}",
            updateInfo.Version, updateInfo.IsPrerelease, updateInfo.AssetName);

        return updateInfo;
    }

    /// <summary>
    /// 清理标签前缀
    /// </summary>
    private static string CleanTagName(string tagName)
    {
        if (string.IsNullOrEmpty(tagName))
            return string.Empty;

        return tagName.StartsWith("v", StringComparison.OrdinalIgnoreCase)
            ? tagName[1..]
            : tagName;
    }

    /// <summary>
    /// GitHub Release API 响应模型
    /// </summary>
    private class GitHubReleaseResponse
    {
        public string Url { get; set; } = string.Empty;
        public string TagName { get; set; } = string.Empty;
        public string? Name { get; set; }
        public bool Prerelease { get; set; }
        public DateTime PublishedAt { get; set; }
        public string? Body { get; set; }
        public List<GitHubAsset> Assets { get; set; } = new();
    }

    /// <summary>
    /// GitHub 资产模型
    /// </summary>
    private class GitHubAsset
    {
        public string Name { get; set; } = string.Empty;
        public string BrowserDownloadUrl { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}
