using System.Text.Json;
using ImmersingPicker.Core;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Core.Models.SecRandom;
using ImmersingPicker.Services.Services.Picker;
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

    public static List<StudentWithoutSeat> FromSecRandom(string path)
    {
        string clazzName = Path.GetFileNameWithoutExtension(path);
        string data = File.ReadAllText(path);

        Dictionary<string, SecRandomStudentWithoutName> studentWithoutNames =
            JsonSerializer.Deserialize<Dictionary<string, SecRandomStudentWithoutName>>(data) ?? new();

        List<SecRandomStudent> secRandomStudents = new();

        foreach (var pair in studentWithoutNames)
        {
            secRandomStudents.Add(new SecRandomStudent(pair.Key, pair.Value));
        }

        List<StudentWithoutSeat> studentWithoutSeats = secRandomStudents
            .Where(s => s.Exists)
            .Select(s => new StudentWithoutSeat { Name = s.Name, Id = s.Id })
            .ToList();
        return studentWithoutSeats;
    }
}