namespace ImmersingPicker.Core.Models;

public struct UsbDriveInfo
{
    public string VolumeSerialNumber { get; set; }
    public string DriveLetter { get; set; }
    public string VolumeLabel { get; set; }

    public static bool operator ==(UsbDriveInfo a, UsbDriveInfo b)
    {
        return a.VolumeSerialNumber == b.VolumeSerialNumber &&
               a.VolumeSerialNumber != string.Empty &&
               b.VolumeSerialNumber != string.Empty;
    }

    public static bool operator !=(UsbDriveInfo a, UsbDriveInfo b)
    {
        return !(a == b);
    }

    public override bool Equals(object? obj)
    {
        if (obj is UsbDriveInfo other)
        {
            return this == other;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return VolumeSerialNumber?.GetHashCode() ?? 0;
    }
}
