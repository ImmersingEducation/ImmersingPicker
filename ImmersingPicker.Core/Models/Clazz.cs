using ImmersingPicker.Core.Abstractions.Picker;
using Serilog;

namespace ImmersingPicker.Core.Models;

public class Clazz
{
    private static readonly ILogger _logger = Log.ForContext<Clazz>();

    protected void OnStudentListChanged()
    {
        _logger.Verbose("学生列表变更事件触发，班级：{ClassName}", Name);
        StudentListChanged?.Invoke();
    }

    public static void OnCurrentClassChanged()
    {
        _logger.Verbose("当前班级变更事件触发，索引：{Index}", _currentClassIndex);
        CurrentClassChanged?.Invoke();
    }

    public event Action? StudentListChanged;
    public event Action? HistoriesChanged;
    public static event Action? CurrentClassChanged;

    private static int _currentClassIndex = -1;

    public string Name { get; set; }
    public List<Student> Students { get; set; }
    public List<History> Histories { get; set; }
    public Dictionary<string, PickerBase> Pickers { get; set; }

    public static List<Clazz> Classes { get; set; } = new();

    public static int CurrentClassIndex
    {
        get => _currentClassIndex;
        set
        {
            _currentClassIndex = value;
            OnCurrentClassChanged();
        }
    }

    static Clazz()
    {
        _logger.Information("初始化 Clazz 类");
        Classes = new List<Clazz>();
        _currentClassIndex = 0;
        _logger.Verbose("初始班级数量：{Count}", Classes.Count);
    }

    public static Clazz? GetCurrentClazz()
    {
        _logger.Verbose("获取当前班级，索引：{Index}", CurrentClassIndex);
        var clazz = GetClazz(CurrentClassIndex);
        if (clazz == null)
        {
            _logger.Warning("当前班级为空，索引：{Index}", CurrentClassIndex);
        }
        return clazz;
    }

    public static void SetCurrentClazz(string name)
    {
        var clazz = GetClazz(name);
        if (clazz is null) return;
        CurrentClassIndex = Classes.IndexOf(clazz);
    }

    public static Clazz? GetClazz(int index)
    {
        _logger.Verbose("获取班级，索引：{Index}", index);
        if (index < 0 || index >= Classes.Count)
        {
            _logger.Warning("索引超出范围：{Index}, 总数：{Count}", index, Classes.Count);
            return null;
        }
        return Classes[index];
    }

    public static Clazz? GetClazz(string name)
    {
        foreach (var @class in Classes)
        {
            if (@class.Name == name)
            {
                return @class;
            }
        }
        return null;
    }

    public static bool CheckIfNameExists(string name)
    {
        _logger.Verbose("检查班级名称是否存在：{Name}", name);
        var exists = Classes.Any(clazz => clazz.Name == name);
        if (exists)
        {
            _logger.Debug("班级名称已存在：{Name}", name);
        }
        return exists;
    }

    public Clazz()
    {
        _logger.Verbose("创建默认班级实例");
        Name = "Nameless-Clazz";
        Students = new List<Student>();
        Histories = new List<History>();
        Pickers = new Dictionary<string, PickerBase>();
    }

    public Clazz(string name)
    {
        _logger.Information("创建新班级：{Name}", name);
        Name = name;
        Students = new List<Student>();
        Histories = new List<History>();
        Pickers = new Dictionary<string, PickerBase>();

        Classes.Add(this);
        _logger.Debug("班级已添加到列表，当前总数：{Count}", Classes.Count);
    }

    public Clazz(string name, List<Student> students, List<History> histories, bool addToClasses = true)
    {
        _logger.Information("创建新班级：{Name}, 学生数：{StudentCount}, 历史数：{HistoryCount}", 
            name, students.Count, histories.Count);
        Name = name;
        Students = students;
        Histories = histories;
        Pickers = new Dictionary<string, PickerBase>();

        if (addToClasses)
        {
            Classes.Add(this);
            _logger.Debug("班级已添加到列表，当前总数：{Count}", Classes.Count);
        }
    }

    public override string ToString()
    {
        return Name;
    }

    public bool CheckIfIdExists(int id)
    {
        _logger.Verbose("检查学生 ID 是否存在：{Id}", id);
        bool flag = false;
        foreach (Student student in Students)
        {
            if (student.Id == id)
            {
                flag = true;
                _logger.Debug("学生 ID 已存在：{Id}", id);
                break;
            }
        }

        return flag;
    }

    public Student? FindStudentById(int id)
    {
        _logger.Verbose("查找学生 ID: {Id}", id);
        var student = Students.Find(s => s.Id == id);
        if (student == null)
        {
            _logger.Warning("未找到学生 ID: {Id}", id);
        }
        return student;
    }

    public void AddStudent(string name, int id, ValueTuple<int, int> seat)
    {
        _logger.Information("添加学生：{Name}, ID: {Id}, 座位：{Seat}", name, id, seat);
        if (!CheckIfIdExists(id))
        {
            Student s = new Student(name, id, seat.Item1, seat.Item2);
            Students.Add(s);
            _logger.Debug("学生添加成功，当前学生数：{Count}", Students.Count);
            OnStudentListChanged();
        }
        else
        {
            _logger.Warning("学生 ID 已存在，添加失败：{Id}", id);
        }
    }

    public void SetStudentInitialWeight(int id, double initialWeight)
    {
        _logger.Verbose("设置学生初始权重：ID: {Id}, 权重：{Weight}", id, initialWeight);
        if (CheckIfIdExists(id))
        {
            var student = FindStudentById(id);
            if (student != null)
            {
                student.InitialWeight = initialWeight;
                _logger.Debug("学生初始权重设置成功：{Id}, {Weight}", id, initialWeight);
            }
        }
        else
        {
            _logger.Warning("学生 ID 不存在，无法设置权重：{Id}", id);
        }
    }

    public void RemoveStudent(int id)
    {
        _logger.Information("删除学生：ID: {Id}", id);
        if (CheckIfIdExists(id))
        {
            var student = Students.First(s => s.Id == id);
            Students.Remove(student);
            _logger.Debug("学生删除成功，剩余学生数：{Count}", Students.Count);
            OnStudentListChanged();
        }
        else
        {
            _logger.Warning("学生 ID 不存在，删除失败：{Id}", id);
        }
    }

    public History? Pick(string picker, int amount)
    {
        _logger.Information("执行抽选：抽选器：{Picker}, 人数：{Amount}", picker, amount);
        if (!Pickers.ContainsKey(picker))
        {
            _logger.Error("抽选器不存在：{Picker}", picker);
            return null;
        }
        
        History history = Pickers[picker].Pick(amount);
        _logger.Debug("抽选完成，选中学生：{Students}", string.Join(", ", history.Students.Select(s => s.Name)));
        AddHistory(history);
        return history;
    }

    public void AddHistory(History history)
    {
        _logger.Verbose("添加历史记录：时间：{Time}, 抽选器：{Selector}, 学生数：{Count}", 
            history.CreateTime, history.Selector, history.Students.Count);
        Histories.Add(history);
        _logger.Debug("历史记录添加成功，当前历史数：{Count}", Histories.Count);
        HistoriesChanged?.Invoke();
    }
}