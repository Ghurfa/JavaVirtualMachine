using System.Text.Json.Serialization;

namespace JavaVirtualMachine.StackTracePrinters
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    internal enum PrintStackTraceOptions
    {
        None,
        Window,
        File
    }
}
