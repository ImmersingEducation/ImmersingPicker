using System.Reflection;
using Serilog;

namespace ImmersingPicker.Services.Services;

public class VersionServices
{
    private static readonly ILogger _logger = Log.ForContext(typeof(VersionServices));
    
    public static VersionInfo CurrentVersion = new VersionInfo(Assembly.GetExecutingAssembly().GetName().Version);

    static VersionServices()
    {
        _logger.Information("初始化版本服务，当前版本：{Version}", VersionString(CurrentVersion));
    }

    public static string VersionString(VersionInfo info)
    {
        var version = $"{info.Major}.{info.Minor}.{info.Patch}" +
                     $"{info.Step switch { VersionStep.ALPHA => "-Alpha", VersionStep.BETA => "-Beta", _ => "" }} " +
                     $"codename {info.Codename}";
        _logger.Verbose("版本字符串：{Version}", version);
        return version;
    }

    public enum VersionStep
    {
        ALPHA, BETA, GA
    }

    public struct VersionInfo
    {
        public static Dictionary<Tuple<int, int>, string> Codenames = new()
        {
            { new(0, 1), "Ellen" },
            { new (0, 2), "Ellen" },
            { new(0, 3), "Ellen" }
        };

        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public VersionStep Step { get; set; }
        public string Codename { get; set; }

        public VersionInfo(Version version)
        {
            _logger.Verbose("创建版本信息");
            Major = version?.Major ?? 0;
            Minor = version?.Minor ?? 0;
            Patch = version?.Build ?? 0;
            Step = version?.Revision switch
            {
                1 => VersionStep.ALPHA,
                2 => VersionStep.BETA,
                _ => VersionStep.GA
            };
            Codename = Codenames.TryGetValue(new Tuple<int, int>(Major, Minor), out var codename) ? codename : "Belle?";
            _logger.Debug("版本信息：{Major}.{Minor}.{Patch} ({Step}), 代号：{Codename}", 
                Major, Minor, Patch, Step, Codename);
        }
    }
}