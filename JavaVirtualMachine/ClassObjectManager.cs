using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine
{
    public static class ClassObjectManager
    {
        private static Dictionary<string, int> classObjects = new Dictionary<string, int>();
        public static int GetClassObjectAddr(ClassFile classFile)
        {
            return GetClassObjectAddr(classFile.Name);
        }

        public static int GetClassObjectAddr(CClassInfo classInfo)
        {
            return GetClassObjectAddr(classInfo.Name);
        }

        public static int GetClassObjectAddr(string name)
        {
            if (name[0] != '[')
            {
                for (int i = 0; i < name.Length; i++)
                {
                    if (name[i] == 'L')
                    {
                        name = name.Substring(0, i) + name.Substring(i + 1, name.Length - i - 2);
                    }
                    break;
                }
            }

            if (name.Length == 1)
            {
                name = JavaHelper.PrimitiveFullName(name);
            }
            else
            {
                name = name.Replace('/', '.');
            }

            int addr = 0;
            if (!classObjects.TryGetValue(name, out addr))
            {
                addr = Heap.CreateObject(ClassFileManager.GetClassFileIndex("java/lang/Class"));
                classObjects.Add(name, addr);
                HeapObject obj = Heap.GetObject(addr);
                if (name == "java.io.OutputStream")
                {

                }
                obj.SetField("name", "Ljava/lang/String;", JavaHelper.CreateJavaStringLiteral(name));
                ;
            }
            return addr;
        }
    }
}
