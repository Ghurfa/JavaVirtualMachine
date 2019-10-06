using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine
{
    class FieldReferenceValue : FieldValue
    {
        public int Address;
        public FieldReferenceValue(int address)
        {
            Address = address;
            Data = new Memory<byte>(((long)address).AsByteArray());
        }
    }
}
