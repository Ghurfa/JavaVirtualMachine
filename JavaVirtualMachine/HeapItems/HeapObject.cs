using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JavaVirtualMachine
{
    public class HeapObject : HeapItem
    {
        public ClassFile ClassFile;
        public static int FieldOffset = 8;
        public static int FieldSize = 8;
        private FieldValue[] fields;

        public HeapObject(ClassFile classFile)
        {
            ClassFile = classFile;
            fields = new FieldValue[ClassFile.ObjectFields.Length];
            ClassFile.ObjectFields.CopyTo(fields, 0);
            NumOfBytes = 8 + 8 * fields.Length;
            Address = Heap.AllocateMemory(NumOfBytes);
        }

        public FieldValue GetField(string name, string descriptor)
        {
            return GetField(ClassFile.InstanceSlots[(name, descriptor)]);
        }
        public FieldValue GetField(long slot)
        {
            return fields[slot];
        }
        public FieldValue GetFieldByOffset(long offset)
        {
            return fields[(offset - FieldOffset) / FieldSize];
        }

        public void SetField(string name, string descriptor, int value)
        {
            if (descriptor[0] == '[' || descriptor[0] == 'L')
            {
                SetField(name, descriptor, new FieldReferenceValue(value));
            }
            else
            {
                SetField(name, descriptor, new FieldNumber(value));
            }
        }
        public void SetField(string name, string descriptor, long value) => SetField(name, descriptor, new FieldLargeNumber(value));
        public void SetField(string name, string descriptor, HeapObject value) => SetField(name, descriptor, new FieldReferenceValue(value.Address));
        public void SetField(string name, string descriptor, FieldValue value)
        {
            int slot = ClassFile.InstanceSlots[(name, descriptor)];
            fields[slot] = value;
            SetFieldData(slot, value);
        }
        public void SetFieldBySlot(long slot, FieldValue value)
        {
            fields[slot] = value;
            SetFieldData((int)slot, value);
        }
        public void SetFieldByOffset(long offset, FieldValue value)
        {
            int slot = (int)((offset - FieldOffset) / FieldSize);
            fields[slot] = value;
            SetFieldData(slot, value);
        }
        public void SetFieldData(int slot, FieldValue value)
        {
            value.Data.CopyTo(Heap.GetMemorySlice(Address + 8 + 8 * slot, 8));
        }
        public virtual HeapObject Clone()
        {
            HeapObject clone = new HeapObject(ClassFile);
            for (int i = 0; i < fields.Length; i++)
            {
                clone.SetFieldBySlot(i, GetField(i));
            }
            return clone;
        }

        public bool IsInstance(int classObjectAddr)
        {
            HeapObject classToCastTo = Heap.GetObject(classObjectAddr);

            string from;
            if (this is HeapArray arrToCast)
            {
                FieldReferenceValue nameField = (FieldReferenceValue)(Heap.GetObject(arrToCast.ItemTypeClassObjAddr)).GetField(2);
                from = "[L" + JavaHelper.ReadJavaString(nameField).Replace('.', '/') + ';';
            }
            else
            {
                from = 'L' + ClassFile.Name + ";";
            }

            string to = JavaHelper.ReadJavaString((FieldReferenceValue)classToCastTo.GetField(2));

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
                    catch(FileNotFoundException)
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

                        if(fromPrimitive || toPrimitive)
                        {
                            if(fromPrimitive && toPrimitive)
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