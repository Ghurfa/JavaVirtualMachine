using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.ConstantPoolInfo
{
    class CDoubleInfo : CPInfo
    {
        public uint HighBytes { get; private set; }
        public uint LowBytes { get; private set; }
        public readonly ulong LongValue;
        public readonly double DoubleValue;
        public CDoubleInfo(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            HighBytes = span.ReadFour();
            LowBytes = span.ReadFour();
            LongValue = (HighBytes, LowBytes).ToULong();
            unsafe
            {
                ulong temp = LongValue;
                DoubleValue = *(double*)(&temp);
            }
        }
        public override void Update(CPInfo[] constants) { }
    }
}
