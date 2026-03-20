using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Core.Abstractions.Storage;

public interface IClazzStorageService
{
    public void SaveClasses(List<Clazz> classes);
    public void LoadClasses();

    public void SaveClasses()
    {
        SaveClasses(Clazz.Classes);
    }
}