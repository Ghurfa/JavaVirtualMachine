using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine
{
    public class BootstrapMethod
    {
        public CMethodHandleInfo MethodHandle;
        public CPInfo[] Arguments;
        public BootstrapMethod(ref ReadOnlySpan<byte> data, CPInfo[] constants)
        {
            ushort bootstrapMethodRef = data.ReadTwo();
            MethodHandle = (CMethodHandleInfo)constants[bootstrapMethodRef];

            ushort numBootstrapArguments = data.ReadTwo();
            Arguments = new CPInfo[numBootstrapArguments];
            for (int i = 0; i < numBootstrapArguments; i++)
            {
                ushort index = data.ReadTwo();
                Arguments[i] = constants[index];
            }
        }
    }
}
