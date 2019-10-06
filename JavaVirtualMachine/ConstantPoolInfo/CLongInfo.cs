using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.ConstantPoolInfo
{
    class CLongInfo : CPInfo
    {
        public uint HighBytes { get; private set; }
        public uint LowBytes { get; private set; }
        public readonly long LongValue;
        public CLongInfo(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            HighBytes = span.ReadFour();
            LowBytes = span.ReadFour();
            LongValue = ((long)HighBytes << 32) | LowBytes;
        }

        public override void Update(CPInfo[] constants) { }
    }
}
