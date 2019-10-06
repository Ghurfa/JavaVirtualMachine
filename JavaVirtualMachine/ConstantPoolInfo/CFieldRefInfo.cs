using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.ConstantPoolInfo
{
    public class CFieldRefInfo : CRefInfo
    {
        public CFieldRefInfo(ref ReadOnlySpan<byte> span) : base(ref span) { }
    }
}
