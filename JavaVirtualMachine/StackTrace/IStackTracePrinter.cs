using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
