using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine
{
    public class AttributeInfo
    {
        public ushort AttributeNameIndex { get; private set; }
        public uint AttributeLength { get; private set; }
        public byte[] info;
        public string Name;
        public AttributeInfo(ref ReadOnlySpan<byte> data, CPInfo[] constants)
        {
            AttributeNameIndex = data.ReadTwo();
            AttributeLength = data.ReadFour();
            info = new byte[AttributeLength];
            data.Slice(0, (int)AttributeLength).CopyTo(info);
            data = data.Slice((int)AttributeLength);
            Name = ((CUtf8Info)constants[AttributeNameIndex]).String;
        }
    }
}
