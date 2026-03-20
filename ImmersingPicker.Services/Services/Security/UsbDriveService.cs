using System.Diagnostics;
using System.Runtime.InteropServices;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Services.Services.Security;

public static class UsbDriveService
{
    public static List<UsbDriveInfo> GetUsbDrives()
    {
        var drives = new List<UsbDriveInfo>();
        try
        {
#if WINDOWS
            return GetUsbDrivesWindows();
#elif MACOS
            return GetUsbDrivesMacOS();
#elif LINUX
            return GetUsbDrivesLinux();
#else
            return GetUsbDrivesFallback();
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"获取 U 盘列表失败: {ex.Message}");
            return drives;
        }
    }

    public static UsbDriveInfo? FindMatchingUsbDrive(UsbDriveInfo targetDrive)
    {
        var currentDrives = GetUsbDrives();
        foreach (var drive in currentDrives)
        {
            if (drive == targetDrive)
            {
                return drive;
            }
        }
        return null;
    }

#if WINDOWS
    private static List<UsbDriveInfo> GetUsbDrivesWindows()
    {
        var drives = new List<UsbDriveInfo>();
        var driveInfos = DriveInfo.GetDrives()
            .Where(d => d is { DriveType: DriveType.Removable, IsReady: true });

        foreach (var drive in driveInfos)
        {
            var serialNumber = GetVolumeSerialNumberWindows(drive.Name);
            drives.Add(new UsbDriveInfo
            {
                VolumeSerialNumber = serialNumber,
                DriveLetter = drive.Name,
                VolumeLabel = drive.VolumeLabel ?? "未命名"
            });
        }
        return drives;
    }

    private static string GetVolumeSerialNumberWindows(string driveLetter)
    {
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher(
                $"SELECT VolumeSerialNumber FROM Win32_LogicalDisk WHERE DeviceID = '{driveLetter.TrimEnd('\\')}'"
            );
            foreach (var obj in searcher.Get())
            {
                return obj["VolumeSerialNumber"]?.ToString() ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"获取 Windows 卷序列号失败: {ex.Message}");
        }
        return string.Empty;
    }
#endif

#if MACOS
    private static List<UsbDriveInfo> GetUsbDrivesMacOS()
    {
        var drives = new List<UsbDriveInfo>();
        
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "/usr/sbin/diskutil",
                Arguments = "list external physical",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return drives;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("/dev/disk"))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        var devicePath = parts[0];
                        var mountInfo = GetMountInfoMacOS(devicePath);
                        if (mountInfo != null)
                        {
                            drives.Add(mountInfo.Value);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"获取 macOS U 盘列表失败: {ex.Message}");
        }

        return drives;
    }

    private static UsbDriveInfo? GetMountInfoMacOS(string devicePath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "/usr/sbin/diskutil",
                Arguments = $"info {devicePath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return null;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            string volumeName = "";
            string mountPoint = "";
            string serialNumber = "";

            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("Volume Name:"))
                {
                    volumeName = line.Split(':')[1].Trim();
                }
                else if (line.Contains("Mount Point:"))
                {
                    mountPoint = line.Split(':')[1].Trim();
                }
                else if (line.Contains("Volume UUID:"))
                {
                    serialNumber = line.Split(':')[1].Trim();
                }
            }

            if (!string.IsNullOrEmpty(mountPoint))
            {
                return new UsbDriveInfo
                {
                    VolumeSerialNumber = serialNumber,
                    DriveLetter = mountPoint,
                    VolumeLabel = volumeName
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"获取 macOS 挂载信息失败: {ex.Message}");
        }

        return null;
    }
#endif

#if LINUX
    private static List<UsbDriveInfo> GetUsbDrivesLinux()
    {
        var drives = new List<UsbDriveInfo>();
        
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"lsblk -o NAME,MOUNTPOINT,LABEL,UUID -J -d\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return drives;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var jsonStart = output.IndexOf('[');
            var jsonEnd = output.LastIndexOf(']');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonArray = output.Substring(jsonStart, jsonEnd - jsonStart + 1);
                ParseLsblkOutput(jsonArray, drives);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"获取 Linux U 盘列表失败: {ex.Message}");
        }

        return drives;
    }

    private static void ParseLsblkOutput(string jsonArray, List<UsbDriveInfo> drives)
    {
        try
        {
            var lines = jsonArray.Split(new[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Contains("\"name\""))
                {
                    string name = "", mountpoint = "", label = "", uuid = "";

                    var parts = line.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        var keyValue = part.Split(new[] { ':' }, 2);
                        if (keyValue.Length == 2)
                        {
                            var key = keyValue[0].Trim().Trim('"');
                            var value = keyValue[1].Trim().Trim('"');

                            switch (key)
                            {
                                case "name":
                                    name = value;
                                    break;
                                case "mountpoint":
                                    mountpoint = value;
                                    break;
                                case "label":
                                    label = value;
                                    break;
                                case "uuid":
                                    uuid = value;
                                    break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(mountpoint) && mountpoint != "null")
                    {
                        drives.Add(new UsbDriveInfo
                        {
                            VolumeSerialNumber = uuid,
                            DriveLetter = mountpoint,
                            VolumeLabel = string.IsNullOrEmpty(label) ? "未命名" : label
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"解析 Linux lsblk 输出失败: {ex.Message}");
        }
    }
#endif

#if !WINDOWS && !MACOS && !LINUX
    private static List<UsbDriveInfo> GetUsbDrivesFallback()
    {
        var drives = new List<UsbDriveInfo>();
        var driveInfos = DriveInfo.GetDrives()
            .Where(d => d is { DriveType: DriveType.Removable, IsReady: true });

        foreach (var drive in driveInfos)
        {
            drives.Add(new UsbDriveInfo
            {
                VolumeSerialNumber = string.Empty,
                DriveLetter = drive.Name,
                VolumeLabel = drive.VolumeLabel ?? "未命名"
            });
        }
        return drives;
    }
#endif
}
