﻿using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.Attributes
{
    public class DeprecatedAttribute : AttributeInfo
    {
        public DeprecatedAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
        }
    }
}
