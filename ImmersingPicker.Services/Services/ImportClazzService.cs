using ImmersingPicker.Core;
using ImmersingPicker.Core.Models;
using OfficeOpenXml;

namespace ImmersingPicker.Services.Services;

public class ImportClazzService
{
    public static List<Student> FromTxt(string path)
    {
        List<Student> students = new();
        string[] data = File.ReadAllLines(path);

        int id = 1;
        foreach (var s in data)
        {
            students.Add(new()
            {
                Id = id,
                Name = s,
                SeatRow = 0,
                SeatColumn = id,
                InitialWeight = 1.0,
                LastSelectedTime = null,
                SelectedAmount = 0,
                Weight = -1.0
            });
            id ++;
        }
        return students;
    }
}