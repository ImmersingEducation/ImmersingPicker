using System.Runtime.Serialization;

namespace ImmersingPicker.Core.Exceptions;

[Serializable]
public class NoAvailableStudentException : Exception
{
    public NoAvailableStudentException(): base() { }
    public NoAvailableStudentException(string message): base(message) { }
    public NoAvailableStudentException(string message, Exception inner): base(message, inner) { }
    protected NoAvailableStudentException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}