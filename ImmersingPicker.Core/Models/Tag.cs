namespace ImmersingPicker.Core.Models;

public struct Tag : IEquatable<Tag>
{
    public string Name { get; set; }

    public Tag(string name)
    {
        Name = name;
    }

    public static bool operator ==(Tag tag1, Tag tag2)
    {
        return tag1.Name == tag2.Name;
    }

    public static bool operator !=(Tag tag1, Tag tag2)
    {
        return !(tag1 == tag2);
    }

    public bool Equals(Tag other)
    {
        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        return obj is Tag other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}