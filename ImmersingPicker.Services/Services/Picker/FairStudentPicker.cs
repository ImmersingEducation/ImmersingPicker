using ImmersingPicker.Core;
using ImmersingPicker.Core.Abstractions;
using ImmersingPicker.Core.Abstractions.Picker;
using ImmersingPicker.Core.Exceptions;
using ImmersingPicker.Core.Models;
using Serilog;

namespace ImmersingPicker.Services.Services.Picker;

public class FairStudentPicker(Clazz clazz) : PickerBase(clazz)
{
    private static readonly ILogger _logger = Log.ForContext<FairStudentPicker>();
    
    public override string Name { get; set; } = "FairStudentPicker";
    public override bool NeedStore { get; set; } = true;

    private int CalculateRange(List<Student> needed)
    {
        _logger.Verbose("计算选中次数范围");
        Student maxE = needed.MaxBy(s => s.SelectedAmount)
            ?? throw new NoAvailableStudentException();
        Student minE = needed.MinBy(s => s.SelectedAmount)
            ?? throw new NoAvailableStudentException();
        int range = maxE.SelectedAmount - minE.SelectedAmount;
        _logger.Debug("选中次数范围：{Range} (最大：{Max}, 最小：{Min})", range, maxE.SelectedAmount, minE.SelectedAmount);
        return range;
    }

    private List<Student> CalculateAvailablePickStudents()
    {
        _logger.Debug("开始计算可抽选学生列表");
        List<Student> tmpStudents = [.._clazz.Students];
        int availableRange = CalculateRange(tmpStudents);
        
        _logger.Verbose("初始范围：{Range}, 参数阈值：{Threshold}", availableRange, AppSettings.Instance.WeightCalculationParam9);
        while (availableRange > AppSettings.Instance.WeightCalculationParam9 && tmpStudents.Count >= AppSettings.Instance.WeightCalculationParam10)
        {
            var maxStudent = tmpStudents.MaxBy(s => s.SelectedAmount);
            _logger.Debug("移除选中次数最多的学生：{Name}, 次数：{Count}", maxStudent?.Name, maxStudent?.SelectedAmount);
            tmpStudents.Remove(maxStudent!);
            availableRange = CalculateRange(tmpStudents);
        }

        double average = _clazz.Students.Sum(s => s.SelectedAmount) / Convert.ToDouble(_clazz.Students.Count);
        _logger.Verbose("平均选中次数：{Average}", average);
        
        int removedCount = tmpStudents.RemoveAll(s => s.SelectedAmount > average);
        _logger.Debug("移除高于平均值的学生数：{Count}, 剩余可抽选学生：{Remaining}", removedCount, tmpStudents.Count);
        
        return tmpStudents;
    }

    private void CalculateWeight()
    {
        _logger.Debug("开始计算学生权重");
        List<Student> available = CalculateAvailablePickStudents();
        double average = _clazz.Students.Sum(s => s.SelectedAmount) / Convert.ToDouble(_clazz.Students.Count);
        
        _logger.Verbose("第一轮：重置权重");
        foreach (Student student in _clazz.Students)
        {
            double oldWeight = student.Weight;
            if (student.Weight <= student.InitialWeight || student.Weight <= 0)
            {
                student.Weight = student.InitialWeight;
            }
            else
            {
                student.Weight = Math.Log(student.Weight);
            }
            if (oldWeight != student.Weight)
            {
                _logger.Verbose("学生 {Name} 权重重置：{OldWeight} -> {NewWeight}", student.Name, oldWeight, student.Weight);
            }
        }
        
        _logger.Verbose("第二轮：计算综合权重");
        foreach (Student student in _clazz.Students)
        {
            double weightChange = 0;
            
            // 历史次数惩罚
            if (available.Contains(student))
            {
                var change = Math.Pow(Math.Abs(student.SelectedAmount - average), AppSettings.Instance.WeightCalculationParam1);
                if (student.SelectedAmount < average)
                {
                    change = -change;
                }
                student.Weight += change;
                weightChange += change;
                _logger.Verbose("学生 {Name} 历史次数惩罚：{Change}", student.Name, change);
            }
            else
            {
                student.Weight += AppSettings.Instance.WeightCalculationParam2;
                weightChange += AppSettings.Instance.WeightCalculationParam2;
                _logger.Verbose("学生 {Name} 未选中惩罚：{Change}", student.Name, AppSettings.Instance.WeightCalculationParam2);
            }

            // 时间衰减
            if (student.LastSelectedTime != null)
            {
                TimeSpan? span = DateTime.Now - student.LastSelectedTime;
                if (span.GetValueOrDefault().Days > AppSettings.Instance.WeightCalculationParam3)
                {
                    var timeBonus = Math.Pow(AppSettings.Instance.WeightCalculationParam4, span.GetValueOrDefault().Days);
                    student.Weight += timeBonus;
                    weightChange += timeBonus;
                    _logger.Verbose("学生 {Name} 时间衰减奖励：{Days}天 -> {Bonus}", student.Name, span.GetValueOrDefault().Days, timeBonus);
                }
                else
                {
                    student.Weight += AppSettings.Instance.WeightCalculationParam5;
                    weightChange += AppSettings.Instance.WeightCalculationParam5;
                    _logger.Verbose("学生 {Name} 时间内衰减：{Bonus}", student.Name, AppSettings.Instance.WeightCalculationParam5);
                }
            }
            else
            {
                student.Weight += AppSettings.Instance.WeightCalculationParam6;
                weightChange += AppSettings.Instance.WeightCalculationParam6;
                _logger.Verbose("学生 {Name} 从未被选中奖励：{Bonus}", student.Name, AppSettings.Instance.WeightCalculationParam6);
            }

            // 随机因子
            var randomInt = _random.Next(AppSettings.Instance.WeightCalculationParam7, AppSettings.Instance.WeightCalculationParam8);
            var randomDouble = _random.NextDouble();
            var randomFactor = Convert.ToDouble(randomInt) + randomDouble;
            student.Weight += randomFactor;
            weightChange += randomFactor;
            
            _logger.Debug("学生 {Name} 最终权重：{Weight} (变化：{Change})", student.Name, student.Weight, weightChange);
        }
    }

    protected override History PickLogic(int amount)
    {
        _logger.Information("开始执行公平抽选，班级：{ClassName}, 抽取数量：{Amount}", _clazz.Name, amount);
        
        try
        {
            _logger.Debug("计算学生权重");
            CalculateWeight();
            
            _logger.Debug("创建优先级队列");
            PriorityQueue<Student, double> pq = new(
                Comparer<double>.Create((x, y) => y.CompareTo(x))
            );
            
            _logger.Verbose("打乱学生顺序并入队");
            foreach (Student student in _clazz.Students.OrderBy(_ => _random.Next()).ToList())
            {
                pq.Enqueue(student, student.Weight);
            }
            _logger.Debug("优先级队列创建完成，队列大小：{Size}", pq.Count);

            _logger.Information("开始从优先级队列中抽取学生");
            List<Student> picked = new();
            for (int i = 0; i < amount && pq.Count > 0; i++)
            {
                var student = pq.Dequeue();
                picked.Add(student);
                _logger.Verbose("抽取第 {Index} 个学生：{Name}, 权重：{Weight}", i + 1, student.Name, student.Weight);
            }

            _logger.Debug("对抽取结果按 ID 排序");
            picked.Sort((s1, s2) => s1.Id.CompareTo(s2.Id));

            _logger.Information("抽选完成，结果：{PickedStudents}", string.Join(", ", picked.Select(s => s.Name)));
            
            // 更新被选中学生的统计信息
            foreach (var student in picked)
            {
                student.LastSelectedTime = DateTime.Now;
                student.SelectedAmount++;
                _logger.Verbose("更新学生 {Name} 统计：选中次数 -> {Count}", student.Name, student.SelectedAmount);
            }

            var history = new History(DateTime.Now, Name, picked);
            _logger.Information("创建历史记录成功");
            return history;
        }
        catch (NoAvailableStudentException ex)
        {
            _logger.Error(ex, "没有可用的学生进行抽选");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "公平抽选过程中发生错误");
            throw;
        }
    }
}