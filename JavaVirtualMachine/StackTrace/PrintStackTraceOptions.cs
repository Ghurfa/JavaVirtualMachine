using System.Text.Json.Serialization;

namespace JavaVirtualMachine.StackTrace
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    internal enum PrintStackTraceOptions
    {
        None,
        Window,
        File
    }
}
