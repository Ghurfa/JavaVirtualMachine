using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.Attributes
{
    public struct BootstrapMethod
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

    public class BootstrapMethodsAttribute : AttributeInfo
    {
        public ushort NumBootstrapMethods;
        public BootstrapMethod[] BootstrapMethods;
        public BootstrapMethodsAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
            ReadOnlySpan<byte> infoAsSpan = info.AsSpan();
            NumBootstrapMethods = infoAsSpan.ReadTwo();
            BootstrapMethods = new BootstrapMethod[NumBootstrapMethods];
            for(int i = 0; i < NumBootstrapMethods; i++)
            {
                BootstrapMethods[i] = new BootstrapMethod(ref infoAsSpan, constants);
            }
        }
    }
}
