using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine
{
    public static class StringPool
    {
        public static LinkedList<int> StringAddresses = new LinkedList<int>();
        public static int Intern(int stringObjToInternAddr)
        {
            foreach(int stringObjAddr in StringAddresses)
            {
                if(Utility.ReadJavaString(stringObjAddr) == Utility.ReadJavaString(stringObjToInternAddr))
                {
                    return stringObjAddr;
                }
            }
            StringAddresses.AddFirst(stringObjToInternAddr);
            return stringObjToInternAddr;
        }
    }
}
