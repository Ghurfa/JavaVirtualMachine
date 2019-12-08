using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace JavaVirtualMachine.Attributes
{
    public struct ExceptionHandlerInfo
    {
        //pc = ip
        public readonly ushort StartPc;
        public readonly ushort EndPc;
        public readonly ushort HandlerPc;
        public readonly ushort CatchType;
        public readonly CClassInfo CatchClassType;
        public ExceptionHandlerInfo(ref ReadOnlySpan<byte> data, CPInfo[] constants)
        {
            StartPc = data.ReadTwo();
            EndPc = data.ReadTwo();
            HandlerPc = data.ReadTwo();
            CatchType = data.ReadTwo();
            if(CatchType != 0)
            {
                CatchClassType = (CClassInfo)constants[CatchType];
            }
            else
            {
                CatchClassType = null;
            }
        }
    }

    public class CodeAttribute : AttributeInfo
    {
        public readonly ushort MaxStack;
        public readonly ushort MaxLocals;
        public readonly ReadOnlyMemory<byte> Code;

        readonly ushort ExceptionTableLength;
        public readonly ExceptionHandlerInfo[] ExceptionTable;
        public readonly ushort AttributesCount;
        public readonly AttributeInfo[] AttributeInfo;

        public CodeAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
            ReadOnlySpan<byte> infoAsSpan = info.AsSpan();

            MaxStack = infoAsSpan.ReadTwo();
            MaxLocals = infoAsSpan.ReadTwo();

            uint codeLength = infoAsSpan.ReadFour();
            Code = infoAsSpan.Slice(0, (int)codeLength).ToArray();
            infoAsSpan = infoAsSpan.Slice((int)codeLength);

            ExceptionTableLength = infoAsSpan.ReadTwo();
            ExceptionTable = new ExceptionHandlerInfo[ExceptionTableLength];
            for (int i = 0; i < ExceptionTableLength; i++)
            {
                ExceptionTable[i] = new ExceptionHandlerInfo(ref infoAsSpan, constants);
            }

            AttributesCount = infoAsSpan.ReadTwo();
            AttributeInfo = new AttributeInfo[AttributesCount];
            for (int i = 0; i < AttributesCount; i++)
            {
                ushort nameIndexNonSwapped = MemoryMarshal.Cast<byte, ushort>(infoAsSpan)[0];
                ushort nameIndex = nameIndexNonSwapped.SwapEndian();
                string name = ((CUtf8Info)constants[nameIndex]).String;
                switch (name)
                {
                    case "StackMapTable":
                        AttributeInfo[i] = new StackMapTableAttribute(ref infoAsSpan, constants);
                        break;
                    case "LineNumberTable":
                        AttributeInfo[i] = new LineNumberTableAttribute(ref infoAsSpan, constants);
                        break;
                    case "LocalVariableTable":
                        AttributeInfo[i] = new LocalVariableTableAttribute(ref infoAsSpan, constants);
                        break;
                    case "LocalVariableTypeTable":
                        AttributeInfo[i] = new LocalVariableTypeTableAttribute(ref infoAsSpan, constants);
                        break;
                    default:
                        AttributeInfo[i] = new AttributeInfo(ref infoAsSpan, constants);
                        break;

                }
            }
        }
    }
}
    