namespace ImmersingPicker.Core.Models.SecRandom;

public class SecRandomStudentWithoutName
{
    public int Id { get; set; }
    public string Gender { get; set; }
    public string Group { get; set; }
    public bool Exists { get; set; }
    public List<string> Tags { get; set; }
}

public class SecRandomStudent
{
    public string Name { get; set; }
    public int Id { get; set; }
    public string Gender { get; set; }
    public string Group { get; set; }
    public bool Exists { get; set; }
    public List<string> Tags { get; set; }

    public SecRandomStudent(string name, SecRandomStudentWithoutName studentWithoutName)
    {
        Name = name;
        Id = studentWithoutName.Id;
        Gender = studentWithoutName.Gender;
        Group = studentWithoutName.Group;
        Exists = studentWithoutName.Exists;
        Tags = studentWithoutName.Tags;
    }
}