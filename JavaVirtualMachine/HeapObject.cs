using JavaVirtualMachine.Heaps;

namespace JavaVirtualMachine
{
    public class HeapObject
    {
        public int Address { get; private set; }
        protected IHeap OwnerHeap;

        private ClassFile? _classFile;
        public ClassFile ClassFile => _classFile ??= ClassFileManager.ClassFiles[Heap.GetInt(Address)];

        public HeapObject(int address, IHeap heap)
        {
            Address = address;
            _classFile = null;
            OwnerHeap = heap;
        }

        public int GetField(string name, string descriptor)
        {
            int slot = ClassFile.InstanceFields.FindIndex(x => x.Name == name && x.Descriptor == descriptor);
            return OwnerHeap.GetInt(Address + OwnerHeap.ObjectFieldOffset + OwnerHeap.ObjectFieldSize * slot);
        }

        public int GetField(int slot)
        {
            int offset = OwnerHeap.ObjectFieldOffset + (OwnerHeap.ObjectFieldSize * slot);
            return OwnerHeap.GetInt(Address + offset);
        }

        public long GetFieldLong(string name, string descriptor)
        {
            int slot = ClassFile.InstanceFields.FindIndex(x => x.Name == name && x.Descriptor == descriptor);
            return OwnerHeap.GetLong(Address + OwnerHeap.ObjectFieldOffset + OwnerHeap.ObjectFieldSize * slot);
        }

        public long GetFieldLong(int slot)
        {
            int offset = OwnerHeap.ObjectFieldOffset + (OwnerHeap.ObjectFieldSize * slot);
            return OwnerHeap.GetLong(Address + offset);
        }

        public void SetField(string name, string descriptor, int value)
        {
            int slot = ClassFile.InstanceFields.FindIndex(x => x.Name == name && x.Descriptor == descriptor);
            OwnerHeap.PutInt(Address + OwnerHeap.ObjectFieldOffset + (OwnerHeap.ObjectFieldSize * slot), value);
        }

        public void SetFieldLong(string name, string descriptor, long value)
        {
            int slot = ClassFile.InstanceFields.FindIndex(x => x.Name == name && x.Descriptor == descriptor);
            OwnerHeap.PutLong(Address + OwnerHeap.ObjectFieldOffset + (OwnerHeap.ObjectFieldSize * slot), value);
        }

        public bool IsInstance(int classObjectAddr)
        {
            HeapObject classToCastTo = OwnerHeap.GetObject(classObjectAddr);

            string from;
            if (this is HeapArray arrToCast)
            {
                int nameStrAddr = OwnerHeap.GetObject(arrToCast.ItemTypeClassObjAddr).GetField(2);
                from = "[L" + JavaHelper.ReadJavaString(nameStrAddr).Replace('.', '/') + ';';
            }
            else
            {
                from = 'L' + ClassFile.Name + ";";
            }

            string to = JavaHelper.ReadJavaString(classToCastTo.GetField(2));

            //https://docs.oracle.com/javase/specs/jvms/se12/html/jvms-6.html#jvms-6.5.instanceof

            while (true)
            {
                if (from[0] != '[')
                {
                    bool fromPrimitive = false;
                    ClassFile fromCFile = null;
                    try
                    {
                        fromCFile = ClassFileManager.GetClassFile(from);
                    }
                    catch (FileNotFoundException)
                    {
                        fromPrimitive = true;
                    }

                    if (to[0] != '[')
                    {
                        bool toPrimitive = false;
                        ClassFile toCFile = null;
                        try
                        {
                            toCFile = ClassFileManager.GetClassFile(to);
                        }
                        catch (FileNotFoundException)
                        {
                            toPrimitive = true;
                        }

                        if (fromPrimitive || toPrimitive)
                        {
                            if (fromPrimitive && toPrimitive)
                            {
                                to = JavaHelper.PrimitiveFullName(to);
                                if (to[0] != 'L') to = "L" + to + ";";
                                return from == to;
                            }
                            return false;
                        }
                        else
                        {
                            if (!toCFile.IsInterface())
                            {
                                return fromCFile.IsSubClassOf(toCFile);
                            }
                            else
                            {
                                return fromCFile.ImplementsInterface(toCFile);
                            }
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    if (to[0] != '[')
                    {
                        ClassFile toCFile = ClassFileManager.GetClassFile(to);
                        if (!toCFile.IsInterface())
                        {
                            return toCFile.Name == "java/lang/Object";
                        }
                        else
                        {
                            //https://docs.oracle.com/javase/specs/jls/se8/html/jls-4.html#jls-4.10
                            return toCFile.Name == "java/lang/Cloneable" || toCFile.Name == "java/io/Serializable";
                        }
                    }
                    else
                    {
                        string toItemType = to.Substring(1);
                        string fromItemType = from.Substring(1);
                        if (toItemType == fromItemType)
                        {
                            return true;
                        }
                        else
                        {
                            to = toItemType;
                            from = fromItemType;
                        }
                    }
                }
            }
        }
    }
}