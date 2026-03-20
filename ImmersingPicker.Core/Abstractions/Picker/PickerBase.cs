using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Core.Abstractions.Picker;

public abstract class PickerBase
{
    public abstract string Name { get; set; }
    public abstract bool NeedStore { get; set; }

    protected readonly Random _random = new();

    protected Clazz _clazz;

    public PickerBase(Clazz clazz)
    {
        _clazz = clazz;
        if (clazz != null)
        {
            _clazz.Pickers.Add(Name, this);
        }
    }

    protected abstract History PickLogic(int amount);

    public History Pick(int amount)
    {
        History history = PickLogic(amount);
        if (!NeedStore) return history;

        _clazz.AddHistory(history);
        foreach (Student student in history.Students)
        {
            student.LastSelectedTime = history.CreateTime;
            student.SelectedAmount++;
        }

        return history;
    }
}