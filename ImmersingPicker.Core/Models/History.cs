namespace ImmersingPicker.Core.Models;

public class History
{
    public DateTime CreateTime { get; set; }
    public string Selector { get; set; }
    public List<Student> Students { get; set; }

    public History()
    {
        CreateTime = DateTime.Now;
        Selector = string.Empty;
        Students = new List<Student>();
    }

    public History(DateTime createTime, string selector, List<Student> students)
    {
        CreateTime = createTime;
        Selector = selector;
        Students = students;
    }
}