using Avalonia.Controls;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Controls;

public partial class StudentEditCardDialog : UserControl
{
    private Student? _student;
    private readonly bool _isNewStudent;

    public StudentEditCardDialog()
    {
        InitializeComponent();
        _isNewStudent = true;
        DialogTitleText.Text = "新建学生";
    }

    public StudentEditCardDialog(Student student)
    {
        InitializeComponent();
        _student = student;
        _isNewStudent = false;
        DialogTitleText.Text = "编辑学生信息";
        IdNumericUpDown.IsReadOnly = true;
        LoadStudent(student);
    }

    private void LoadStudent(Student student)
    {
        NameTextBox.Text = student.Name;
        IdNumericUpDown.Value = student.Id;
        SeatRowNumericUpDown.Value = student.SeatRow;
        SeatColumnNumericUpDown.Value = student.SeatColumn;
        InitialWeightNumericUpDown.Value = (decimal)student.InitialWeight;
        SelectedAmountNumericUpDown.Value = student.SelectedAmount;
    }

    public Student GetUpdatedStudent()
    {
        if (_student == null && !_isNewStudent) return null!;

        if (_isNewStudent)
        {
            _student = new Student();
        }

        _student.Name = NameTextBox.Text ?? string.Empty;

        if (IdNumericUpDown.Value.HasValue)
            _student.Id = (int)IdNumericUpDown.Value.Value;

        if (SeatRowNumericUpDown.Value.HasValue)
            _student.SeatRow = (int)SeatRowNumericUpDown.Value.Value;

        if (SeatColumnNumericUpDown.Value.HasValue)
            _student.SeatColumn = (int)SeatColumnNumericUpDown.Value.Value;

        if (InitialWeightNumericUpDown.Value.HasValue)
            _student.InitialWeight = (double)InitialWeightNumericUpDown.Value.Value;

        return _student;
    }
}
