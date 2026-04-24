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
        LoadStudent(student);
    }

    private void LoadStudent(Student student)
    {
        NameTextBox.Text = student.Name;
        IdTextBox.Text = student.Id.ToString();
        SeatRowTextBox.Text = student.SeatRow.ToString();
        SeatColumnTextBox.Text = student.SeatColumn.ToString();
        InitialWeightTextBox.Text = student.InitialWeight.ToString("0.00");
        SelectedAmountTextBox.Text = student.SelectedAmount.ToString();
    }

    public Student GetUpdatedStudent()
    {
        if (_student == null && !_isNewStudent) return null!;

        if (_isNewStudent)
        {
            _student = new Student();
        }

        _student.Name = NameTextBox.Text ?? string.Empty;

        if (int.TryParse(IdTextBox.Text, out int id))
            _student.Id = id;

        if (int.TryParse(SeatRowTextBox.Text, out int seatRow))
            _student.SeatRow = seatRow;

        if (int.TryParse(SeatColumnTextBox.Text, out int seatColumn))
            _student.SeatColumn = seatColumn;

        if (double.TryParse(InitialWeightTextBox.Text, out double initialWeight))
            _student.InitialWeight = initialWeight;

        if (int.TryParse(SelectedAmountTextBox.Text, out int selectedAmount))
            _student.SelectedAmount = selectedAmount;

        return _student;
    }
}
