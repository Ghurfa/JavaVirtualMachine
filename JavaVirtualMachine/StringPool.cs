using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine
{
    public static class StringPool
    {
        public static Dictionary<string, int> StringAddresses { get; } = new();
    }
}
