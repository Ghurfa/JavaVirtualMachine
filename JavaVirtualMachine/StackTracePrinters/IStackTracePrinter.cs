using JavaVirtualMachine.ConstantPoolItems;

namespace JavaVirtualMachine.StackTracePrinters
{
    internal interface IStackTracePrinter
    {
        void PrintMethodCall(MethodInfo method, Span<int> args, CInterfaceMethodRefInfo? interfaceMethod = null);
        void PrintMethodReturn(MethodInfo method, int returnValue);
        void PrintMethodReturn(MethodInfo method, long returnValue);
        void PrintMethodReturn(MethodInfo method);
        void PrintMethodThrewException(MethodInfo method, int exception);
    }
}
