using JavaVirtualMachine.ConstantPoolItems;

namespace JavaVirtualMachine.Attributes
{
    public struct BootstrapMethod
    {
        public CMethodHandleInfo MethodHandle;
        public CPInfo[] Arguments;
        public BootstrapMethod(ref ReadOnlySpan<byte> data, CPInfo[] constants)
        {
            ushort bootstrapMethodRef = data.ReadTwo();
            MethodHandle = (CMethodHandleInfo)constants[bootstrapMethodRef];

            ushort numBootstrapArguments = data.ReadTwo();
            Arguments = new CPInfo[numBootstrapArguments];
            for (int i = 0; i < numBootstrapArguments; i++)
            {
                ushort index = data.ReadTwo();
                Arguments[i] = constants[index];
            }
        }
        public string GetArgumentsDescriptor()
        {
            string ret = "";
            foreach(CPInfo constant in Arguments)
            {
                if (constant is CClassInfo) ret += "Ljava/lang/Class;";
                else if (constant is CMethodHandleInfo) ret += "Ljava/lang/invoke/MethodHandle;";
                else if (constant is CMethodTypeInfo) ret += "Ljava/lang/invoke/MethodType;";
                else if (constant is CStringInfo) ret += "Ljava/lang/String;";
                else if (constant is CIntegerInfo) ret += "I";
                else if (constant is CLongInfo) ret += "J";
                else if (constant is CFloatInfo) ret += "F";
                else if (constant is CDoubleInfo) ret += "D";
                else throw new InvalidOperationException();
            }
            return ret;
        }
    }

    public class BootstrapMethodsAttribute : AttributeInfo
    {
        public ushort NumBootstrapMethods;
        public BootstrapMethod[] BootstrapMethods;
        public BootstrapMethodsAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
            ReadOnlySpan<byte> infoAsSpan = info.AsSpan();
            NumBootstrapMethods = infoAsSpan.ReadTwo();
            BootstrapMethods = new BootstrapMethod[NumBootstrapMethods];
            for(int i = 0; i < NumBootstrapMethods; i++)
            {
                BootstrapMethods[i] = new BootstrapMethod(ref infoAsSpan, constants);
            }
        }
    }
}
