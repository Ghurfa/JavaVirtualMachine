using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.Attributes
{
    public struct LocalVariableTypeInfo
    {
        public ushort StartPc;
        public ushort Length;
        public ushort NameIndex;
        public ushort SignatureIndex;
        public ushort Index;

        public string Name;
        public string Signature;

        public LocalVariableTypeInfo(ref ReadOnlySpan<byte> data, CPInfo[] constants)
        {
            StartPc = data.ReadTwo();
            Length = data.ReadTwo();
            NameIndex = data.ReadTwo();
            SignatureIndex = data.ReadTwo();
            Index = data.ReadTwo();

            Name = ((CUtf8Info)constants[NameIndex]).String;
            Signature = ((CUtf8Info)constants[SignatureIndex]).String;
        }
    }

    public class LocalVariableTypeTableAttribute : AttributeInfo
    {
        ushort LocalVariableTypeTableLength;
        LocalVariableTypeInfo[] LocalVariableTypeTable;
        public LocalVariableTypeTableAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
            ReadOnlySpan<byte> infoAsSpan = info.AsSpan();
            LocalVariableTypeTableLength = infoAsSpan.ReadTwo();
            LocalVariableTypeTable = new LocalVariableTypeInfo[LocalVariableTypeTableLength];
            for(int i = 0; i < LocalVariableTypeTableLength; i++)
            {
                LocalVariableTypeTable[i] = new LocalVariableTypeInfo(ref infoAsSpan, constants);

            }
        }
    }
}
