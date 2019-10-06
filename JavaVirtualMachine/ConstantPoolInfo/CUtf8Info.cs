using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace JavaVirtualMachine.ConstantPoolInfo
{
    class CUtf8Info : CPInfo
    {
        public readonly ushort Length;
        public readonly string String;
        public CUtf8Info(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            Length = span.ReadTwo();
            ReadOnlySpan<byte> temp = span.Slice(0, Length);
            String = Encoding.UTF8.GetString(temp.ToArray());
            span = span.Slice(Length);
        }
        public override void Update(CPInfo[] constants) { }
    }
}
