using JavaVirtualMachine.Attributes;
using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace JavaVirtualMachine
{
    public class ClassFile
    {
        public string Name;
        public ushort AccessFlags { get; private set; }

        public CPInfo[] Constants { get; private set; }

        public Dictionary<(string name, string descriptor), FieldValue> StaticFieldsDictionary;
        public FieldInfo[] FieldsInfo;
        public FieldValue[] ObjectFields;
        public Dictionary<(string, string), int> InstanceSlots;

        public Dictionary<(string name, string descriptor), MethodInfo> MethodDictionary { get; private set; }
        //public Dictionary<(string name, string descriptor), MethodInfo> InstanceMethodDictionary { get; private set; }

        public CClassInfo SuperClassInfo;

        private ClassFile superClass;
        bool setSuperClass = false;
        public ClassFile SuperClass
        {
            get
            {
                if (!setSuperClass)
                {
                    setSuperClass = true;
                    if (SuperClassInfo != null)
                    {
                        superClass = ClassFileManager.GetClassFile(SuperClassInfo.Name);
                    }
                }
                return superClass;
            }
        }

        public string[] InterfaceNames;
        public AttributeInfo[] Attributes;

        public bool Deprecated;
        public byte[] RawAnnotations;

        public ClassFile(string path)
        {
            ReadOnlySpan<byte> data = File.ReadAllBytes(path);

            uint magic = data.ReadFour();
            if (magic != 0xCAFEBABE) throw new InvalidDataException();

            ushort minorVersion = data.ReadTwo();

            ushort majorVersion = data.ReadTwo();

            readConstantPool(ref data);

            AccessFlags = data.ReadTwo();

            ushort thisClass = data.ReadTwo();

            ushort super = data.ReadTwo();
            if (super != 0)
            {
                SuperClassInfo = (CClassInfo)Constants[super];
            }

            readInterfaces(ref data);

            StaticFieldsDictionary = new Dictionary<(string name, string descriptor), FieldValue>();
            readFields(ref data);

            MethodDictionary = new Dictionary<(string, string), MethodInfo>();
            readMethods(ref data);
            readAttributes(ref data);

            updateConstantPoolWithAttributes();

            Name = ((CClassInfo)Constants[thisClass]).Name;

            if (data.Length != 0) throw new InvalidDataException();
        }
        public bool IsInterface()
        {
            return (AccessFlags & 0x200) != 0;
        }

        public IEnumerable<FieldInfo> InstanceFields()
        {
            for (int i = 0; i < FieldsInfo.Length; i++)
            {
                if(!FieldsInfo[i].HasFlag(FieldInfoFlag.Static))
                {
                    yield return FieldsInfo[i];
                }
            }
        }

        private void readConstantPool(ref ReadOnlySpan<byte> data)
        {
            ushort count = data.ReadTwo();
            Constants = new CPInfo[count];
            for (int i = 1; i < count; i++)
            {
                byte tag = data[0];
                switch (tag)
                {
                    case 1:
                        Constants[i] = new CUtf8Info(ref data);
                        break;
                    case 3:
                        Constants[i] = new CIntegerInfo(ref data);
                        break;
                    case 4:
                        Constants[i] = new CFloatInfo(ref data);
                        break;
                    case 5:
                        Constants[i] = new CLongInfo(ref data);
                        i++;
                        break;
                    case 6:
                        Constants[i] = new CDoubleInfo(ref data);
                        i++;
                        break;
                    case 7:
                        Constants[i] = new CClassInfo(ref data);
                        break;
                    case 8:
                        Constants[i] = new CStringInfo(ref data);
                        break;
                    case 9:
                        Constants[i] = new CFieldRefInfo(ref data);
                        break;
                    case 10:
                        Constants[i] = new CMethodRefInfo(ref data);
                        break;
                    case 11:
                        Constants[i] = new CInterfaceMethodRefInfo(ref data);
                        break;
                    case 12:
                        Constants[i] = new CNameAndTypeInfo(ref data);
                        break;
                    case 15:
                        Constants[i] = new CMethodHandleInfo(ref data);
                        break;
                    case 16:
                        Constants[i] = new CMethodTypeInfo(ref data);
                        break;
                    case 18:
                        Constants[i] = new CInvokeDynamicInfo(ref data);
                        break;
                    default:
                        throw new InvalidOperationException($"Invalid Tag {tag}");
                }
            }
            for (int i = 1; i < count; i++)
            {
                Constants[i]?.Update(Constants);
            }
        }
        private void readInterfaces(ref ReadOnlySpan<byte> data)
        {
            ushort interfaceCount = data.ReadTwo();
            InterfaceNames = new string[interfaceCount];
            for (int i = 0; i < interfaceCount; i++)
            {
                ushort index = data.ReadTwo();
                CClassInfo classInfo = (CClassInfo)Constants[index];
                //Load interface?

                InterfaceNames[i] = classInfo.Name;
            }
        }
        private void readFields(ref ReadOnlySpan<byte> data)
        {
            ushort fieldsCount = data.ReadTwo();
            FieldInfo[] temp = new FieldInfo[fieldsCount];
            LinkedList<FieldInfo> fieldsLinkedList = new LinkedList<FieldInfo>();
            LinkedList<FieldValue> instanceFieldValues = new LinkedList<FieldValue>();
            InstanceSlots = new Dictionary<(string, string), int>();
            for (int i = 0; i < fieldsCount; i++)
            {
                temp[i] = new FieldInfo(ref data, Constants);
                if (temp[i].HasFlag(FieldInfoFlag.Static))
                {
                    switch (temp[i].Descriptor[0])
                    {
                        case 'B':
                        case 'C':
                        case 'S':
                        case 'Z':
                        case 'F':
                        case 'I':
                            StaticFieldsDictionary.Add((temp[i].Name, temp[i].Descriptor), new FieldNumber(0));
                            break;
                        case 'D':
                        case 'J':
                            StaticFieldsDictionary.Add((temp[i].Name, temp[i].Descriptor), new FieldLargeNumber(0));
                            break;
                        case 'L':
                        case '[':
                            StaticFieldsDictionary.Add((temp[i].Name, temp[i].Descriptor), new FieldReferenceValue(0));
                            break;
                        default:
                            throw new InvalidDataException();
                    }
                }
                else
                {
                    switch (temp[i].Descriptor[0])
                    {
                        case 'B':
                        case 'C':
                        case 'S':
                        case 'Z':
                        case 'F':
                        case 'I':
                            instanceFieldValues.AddLast(new FieldNumber(0));
                            break;
                        case 'D':
                        case 'J':
                            instanceFieldValues.AddLast(new FieldLargeNumber(0));
                            break;
                        case 'L':
                        case '[':
                            instanceFieldValues.AddLast(new FieldReferenceValue(0));
                            break;
                        default:
                            throw new InvalidDataException();
                    }
                    InstanceSlots.Add((temp[i].Name, temp[i].Descriptor), InstanceSlots.Count);
                }
                fieldsLinkedList.AddLast(temp[i]);
            }

            ClassFile cFile = this;
            while (cFile.SuperClassInfo != null)
            {
                cFile = ClassFileManager.GetClassFile(cFile.SuperClassInfo.Name);
                foreach (FieldInfo fieldValue in cFile.InstanceFields())
                {
                    //if (fieldValue.HasFlag(FieldInfoFlag.Private)) continue;
                    bool exists = false;
                    foreach ((string name, string descriptor) in InstanceSlots.Keys)
                    {
                        if (name == fieldValue.Name)
                        {
                            exists = true;
                            break;
                        }
}
                    if (exists) continue;

                    switch (fieldValue.Descriptor[0])
                    {
                        case 'B':
                        case 'C':
                        case 'S':
                        case 'Z':
                        case 'F':
                        case 'I':
                            instanceFieldValues.AddLast(new FieldNumber(0));
                            break;
                        case 'D':
                        case 'J':
                            instanceFieldValues.AddLast(new FieldLargeNumber(0));
                            break;
                        case 'L':
                        case '[':
                            instanceFieldValues.AddLast(new FieldReferenceValue(0));
                            break;
                        default:
                            throw new InvalidDataException();
                    }
                    InstanceSlots.Add((fieldValue.Name, fieldValue.Descriptor), InstanceSlots.Count);
                }
            }

            FieldsInfo = new FieldInfo[fieldsLinkedList.Count];
            fieldsLinkedList.CopyTo(FieldsInfo, 0);

            ObjectFields = new FieldValue[instanceFieldValues.Count];
            instanceFieldValues.CopyTo(ObjectFields, 0);
        }
        private void readMethods(ref ReadOnlySpan<byte> data)
        {
            ushort methodsCount = data.ReadTwo();
            MethodInfo[] temp = new MethodInfo[methodsCount];
            for (int i = 0; i < methodsCount; i++)
            {
                temp[i] = new MethodInfo(ref data, this);
                MethodDictionary.Add((temp[i].Name, temp[i].Descriptor), temp[i]);
            }
        }
        private void readAttributes(ref ReadOnlySpan<byte> data)
        {
            ushort attributesCount = data.ReadTwo();
            Attributes = new AttributeInfo[attributesCount];
            for (int i = 0; i < attributesCount; i++)
            {
                ushort nameIndexNonSwapped = MemoryMarshal.Cast<byte, ushort>(data)[0];
                ushort nameIndex = nameIndexNonSwapped.SwapEndian();
                string name = ((CUtf8Info)Constants[nameIndex]).String;
                switch (name)
                {
                    case "InnerClasses":
                        Attributes[i] = new InnerClassesAttribute(ref data, Constants);
                        break;
                    case "EnclosingMethod":
                        Attributes[i] = new EnclosingMethodAttribute(ref data, Constants);
                        break;
                    case "Synthetic":
                        Attributes[i] = new SyntheticAttribute(ref data, Constants);
                        break;
                    case "Signature":
                        Attributes[i] = new SignatureAttribute(ref data, Constants);
                        break;
                    case "SourceFile":
                        Attributes[i] = new SourceFileAttribute(ref data, Constants);
                        break;
                    case "SourceDebugExtension":
                        throw new NotImplementedException();
                    case "Deprecated":
                        Attributes[i] = new DeprecatedAttribute(ref data, Constants);
                        Deprecated = true;
                        break;
                    case "RuntimeVisibleAnnotations":
                        Attributes[i] = new RuntimeVisibleAnnotationsAttribute(ref data, Constants);
                        break;
                    case "RuntimeInvisibleAnnotations":
                        throw new NotImplementedException();
                    case "BootstrapMethods":
                        Attributes[i] = new BootstrapMethodsAttribute(ref data, Constants);
                        break;
                    default:
                        Attributes[i] = new AttributeInfo(ref data, Constants);
                        break;
                }
            }
        }
        private void updateConstantPoolWithAttributes()
        {
            for (int i = 1; i < Constants.Length; i++)
            {
                if (Constants[i]?.GetType() == typeof(CInvokeDynamicInfo))
                {
                    ((CInvokeDynamicInfo)Constants[i]).UpdateWithAttributes(Attributes);
                }
            }
        }
    }
}