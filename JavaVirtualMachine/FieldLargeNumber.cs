using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine
{
    class FieldLargeNumber : FieldValue
    {
        public long Value;
        public FieldLargeNumber(long value)
        {
            Value = value;
            Data = new Memory<byte>(value.AsByteArray());
        }
    }
}
