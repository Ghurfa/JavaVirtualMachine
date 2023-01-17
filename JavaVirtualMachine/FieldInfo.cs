using JavaVirtualMachine.Attributes;
using JavaVirtualMachine.ConstantPoolItems;
using System.Runtime.InteropServices;

namespace JavaVirtualMachine
{
    public enum FieldInfoFlag
    {
        Public = 0x0001,	//Declared public; may be accessed from outside its package.
        Private = 0x0002,	//Declared private; accessible only within the defining class and other classes belonging to the same nest(§5.4.4).
        Protected = 0x0004,	//Declared protected; may be accessed within subclasses.
        Static = 0x0008,	//Declared static.
        Final = 0x0010,	    //Declared final; never directly assigned to after object construction(JLS §17.5).
        Volatile = 0x0040,	//Declared volatile; cannot be cached.
        Transient = 0x0080,	//Declared transient; not written or read by a persistent object manager.
        Synthetic = 0x1000,	//Declared synthetic; not present in the source code.
        Enum = 0x4000,	    //Declared as an element of an enum.
    }

    public class FieldInfo
    {
        public readonly ushort AccessFlags;
        public readonly ushort NameIndex;
        public readonly ushort DescriptorIndex;
        public readonly ushort AttributesCount;
        public readonly AttributeInfo[] AttributeInfo;
        public readonly string Name;
        public readonly string Descriptor;

        public FieldInfo(ref ReadOnlySpan<byte> data, CPInfo[] constants)
        {
            AccessFlags = data.ReadTwo();
            NameIndex = data.ReadTwo();
            DescriptorIndex = data.ReadTwo();
            AttributesCount = data.ReadTwo();
            AttributeInfo = new AttributeInfo[AttributesCount];
            for (int i = 0; i < AttributesCount; i++)
            {
                ushort nameIndexNonSwapped = MemoryMarshal.Cast<byte, ushort>(data)[0];
                ushort nameIndex = nameIndexNonSwapped.SwapEndian();
                string name = ((CUtf8Info)constants[nameIndex]).String;
                switch (name)
                {
                    case "ConstantValue":
                        AttributeInfo[i] = new ConstantValueAttribute(ref data, constants);
                        break;
                    case "Synthetic":
                        AttributeInfo[i] = new SyntheticAttribute(ref data, constants);
                        break;
                    case "Signature":
                        AttributeInfo[i] = new SignatureAttribute(ref data, constants);
                        break;
                    case "Deprecated":
                        AttributeInfo[i] = new DeprecatedAttribute(ref data, constants);
                        break;
                    case "RuntimeVisibleAnnotations":
                        AttributeInfo[i] = new RuntimeVisibleAnnotationsAttribute(ref data, constants);
                        break;
                    case "RuntimeInvisibleAnnotations":
                        throw new NotImplementedException();
                    default:
                        AttributeInfo[i] = new AttributeInfo(ref data, constants);
                        break;

                }
            }
            Name = ((CUtf8Info)constants[NameIndex]).String;
            Descriptor = ((CUtf8Info)constants[DescriptorIndex]).String;
        }

        public bool HasFlag(FieldInfoFlag flag)
        {
            return (AccessFlags & (int)flag) != 0;
        }
    }
}
