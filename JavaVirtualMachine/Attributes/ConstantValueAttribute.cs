using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.Attributes
{
    public class ConstantValueAttribute : AttributeInfo
    {
        public ushort ConstantValueIndex;
        public CPInfo ConstantValue;
        public ConstantValueAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
            ReadOnlySpan<byte> infoAsSpan = info.AsSpan();

            ConstantValueIndex = infoAsSpan.ReadTwo();
            ConstantValue = constants[ConstantValueIndex];
        }
    }
}
