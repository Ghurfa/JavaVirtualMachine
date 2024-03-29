﻿using JavaVirtualMachine.ConstantPoolItems;

namespace JavaVirtualMachine.Attributes
{
    public class RuntimeVisibleParameterAnnotationsAttribute : AttributeInfo
    {
        public RuntimeVisibleParameterAnnotationsAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
            throw new NotImplementedException();
        }
    }
}
