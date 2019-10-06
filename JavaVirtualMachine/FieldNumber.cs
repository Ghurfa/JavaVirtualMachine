using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine
{
    class FieldNumber : FieldValue
    {
        public int Value;
        public FieldNumber(int value)
        {
            Value = value;
            Data = new Memory<byte>(((long)value).AsByteArray());
        }
    }
}
