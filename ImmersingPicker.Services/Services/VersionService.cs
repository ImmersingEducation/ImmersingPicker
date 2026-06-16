using System.Reflection;
using Serilog;

namespace ImmersingPicker.Services.Services;

public class VersionService
{
    private static readonly ILogger _logger = Log.ForContext(typeof(VersionService));
    
    public static VersionInfo CurrentVersion = new VersionInfo(Assembly.GetExecutingAssembly().GetName().Version);

    static VersionService()
    {
        _logger.Information("初始化版本服务，当前版本：{Version}", VersionString(CurrentVersion));
    }

    public static string VersionString(VersionInfo info)
    {
        var version = $"{info.Major}.{info.Minor}.{info.Build}" +
                     $"{info.Revision}" +
                     $"codename {info.Codename}";
        _logger.Verbose("版本字符串：{Version}", version);
        return version;
    }

    public struct VersionInfo
    {
        public static Dictionary<Tuple<int, int>, string> Codenames = new()
        {
            { new(0, 1), "Ellen" },
            { new (0, 2), "Ellen" },
            { new(0, 3), "Ellen" },
            { new(0, 4), "Ellen" }
        };

        public int Major { get; set; }
        public int Minor { get; set; }
        public int Build { get; set; }
        public int Revision { get; set; }
        public string Codename { get; set; }

        public VersionInfo(Version version)
        {
            _logger.Verbose("创建版本信息");
            Major = version?.Major ?? 0;
            Minor = version?.Minor ?? 0;
            Build = version?.Build ?? 0;
            Revision = version?.Revision ?? 0;
            Codename = Codenames.TryGetValue(new Tuple<int, int>(Major, Minor), out var codename) ? codename : "Belle?";
        }
    }
}