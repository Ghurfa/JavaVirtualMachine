using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.ConstantPoolInfo
{
    public class CInterfaceMethodRefInfo : CRefInfo
    {
        public CInterfaceMethodRefInfo(ref ReadOnlySpan<byte> span) : base(ref span) { }
    }
}
