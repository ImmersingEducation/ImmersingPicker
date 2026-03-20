using System.Runtime.Serialization;

namespace ImmersingPicker.Core.Exceptions;

[Serializable]
public class ClassIslandIPCInitializationFailed : Exception
{
    public ClassIslandIPCInitializationFailed() : base() { }
    public ClassIslandIPCInitializationFailed(string message) : base(message) { }
    public ClassIslandIPCInitializationFailed(string message, Exception inner) : base(message, inner) { }
    protected ClassIslandIPCInitializationFailed(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}