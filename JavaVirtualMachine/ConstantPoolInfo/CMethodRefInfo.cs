using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.ConstantPoolInfo
{
    public class CMethodRefInfo : CRefInfo
    {
        public CMethodRefInfo(ref ReadOnlySpan<byte> span) : base(ref span) { }
    }
}
