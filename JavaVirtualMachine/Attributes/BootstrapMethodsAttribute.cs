using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.Attributes
{
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
