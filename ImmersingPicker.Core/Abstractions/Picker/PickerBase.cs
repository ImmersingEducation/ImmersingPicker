using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Core.Abstractions.Picker;

public abstract class PickerBase
{
    public abstract string Name { get; set; }
    public abstract bool NeedStore { get; set; }

    protected readonly Random _random = new();

    public PickerBase()
    {
    }

    protected abstract History PickLogic(Clazz clazz, int amount);

    public History Pick(Clazz clazz, int amount)
    {
        History history = PickLogic(clazz, amount);
        if (!NeedStore) return history;

        clazz.AddHistory(history);
        foreach (Student student in history.Students)
        {
            student.LastSelectedTime = history.CreateTime;
            student.SelectedAmount++;
        }

        return history;
    }
}
