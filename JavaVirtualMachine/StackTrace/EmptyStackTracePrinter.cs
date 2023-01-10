using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaVirtualMachine.StackTrace
{
    internal class EmptyStackTracePrinter : IStackTracePrinter
    {
        public static EmptyStackTracePrinter Instance { get; } = new();

        private EmptyStackTracePrinter()
        {

        }

        public void PrintMethodCall(MethodInfo method, int[] args, CInterfaceMethodRefInfo? interfaceMethod = null) { }

        public void PrintMethodReturn(MethodInfo method, int returnValue) { }

        public void PrintMethodReturn(MethodInfo method, long returnValue) { }

        public void PrintMethodReturn(MethodInfo method) { }

        public void PrintMethodThrewException(MethodInfo method, JavaException exception) { }
    }
}
