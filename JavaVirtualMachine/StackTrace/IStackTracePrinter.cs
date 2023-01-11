using JavaVirtualMachine.ConstantPoolInfo;

namespace JavaVirtualMachine.StackTrace
{
    internal interface IStackTracePrinter
    {
        void PrintMethodCall(MethodInfo method, int[] args, CInterfaceMethodRefInfo? interfaceMethod = null);
        void PrintMethodReturn(MethodInfo method, int returnValue);
        void PrintMethodReturn(MethodInfo method, long returnValue);
        void PrintMethodReturn(MethodInfo method);
        void PrintMethodThrewException(MethodInfo method, JavaException exception);
    }
}
