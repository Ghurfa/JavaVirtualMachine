using JavaVirtualMachine.ConstantPoolItems;

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
            if (name[0] == 'L')
            {
                name = name[1..^1]; // Convert Ljava.name.thingy; to java.name.thingy
            }

            if (name.Length == 1)
            {
                name = JavaHelper.PrimitiveFullName(name);
            }
            else
            {
                name = name.Replace('/', '.');
            }

            if (!classObjects.TryGetValue(name, out int addr))
            {
                addr = Heap.CreateObject(ClassFileManager.GetClassFileIndex("java/lang/Class"));
                classObjects.Add(name, addr);
                HeapObject obj = Heap.GetObject(addr);
                obj.SetField("name", "Ljava/lang/String;", JavaHelper.CreateJavaStringLiteral(name));
            }
            return addr;
        }
    }
}
