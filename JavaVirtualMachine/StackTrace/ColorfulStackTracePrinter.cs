using JavaVirtualMachine.ConstantPoolInfo;
using System.IO.Pipes;

namespace JavaVirtualMachine.StackTrace
{
    internal class ColorfulStackTracePrinter : IStackTracePrinter
    {
        public static int Depth = 0;
        const int Spacing = 2;
        const byte DebugDefaultColor = (byte)ConsoleColor.DarkGray;
        const byte NativeMethodColor = (byte)ConsoleColor.Green;
        const byte ExceptionThrownColor = (byte)ConsoleColor.Yellow;

        //Specially printed
        const byte arrayBracketColor = (byte)ConsoleColor.Red;
        const byte nullColor = (byte)ConsoleColor.DarkRed;
        const byte stringColor = (byte)ConsoleColor.DarkGreen;
        const byte classObjColor = (byte)ConsoleColor.DarkBlue;
        const byte booleanColor = (byte)ConsoleColor.DarkRed;
        const byte charColor = (byte)ConsoleColor.DarkGreen;

        //Objects
        const byte classNameColor = (byte)ConsoleColor.Blue;
        const byte separatorColor = (byte)ConsoleColor.White;
        const byte objAddrColor = (byte)ConsoleColor.Cyan;

        //Numbers - could be confused with each other
        const byte byteColor = (byte)ConsoleColor.DarkGreen;
        const byte floatColor = (byte)ConsoleColor.DarkBlue;
        const byte integerColor = (byte)ConsoleColor.White;
        const byte shortColor = (byte)ConsoleColor.DarkYellow;
        const byte longColor = (byte)ConsoleColor.DarkMagenta;
        const byte doubleColor = (byte)ConsoleColor.DarkRed;

        private string LeftPad => new string(' ', Depth * Spacing);

        private Action<byte, string> PrintWithColor;

        public ColorfulStackTracePrinter(Action<byte, string> printWithColor)
        {
            PrintWithColor = printWithColor;
        }

        public void PrintMethodCall(MethodInfo method, int[] args, CInterfaceMethodRefInfo? interfaceMethod = null)
        {
            byte methodColor = method.HasFlag(MethodInfoFlag.Native) ? NativeMethodColor : DebugDefaultColor;

            PrintWithColor(methodColor, $"{LeftPad}{method.ClassFile.Name}.{method.Name}");
            PrintArgs(methodColor, method.Descriptor, method.HasFlag(MethodInfoFlag.Static), args);

            if (interfaceMethod != null)
            {
                PrintWithColor(methodColor,$"   (interface {interfaceMethod.ClassName})\n");
            }
            else
            {
                PrintWithColor(methodColor, "\n");
            }
            Depth++;
        }

        public void PrintMethodReturn(MethodInfo method, int returnValue)
        {
            Depth--;
            PrintWithColor(DebugDefaultColor, LeftPad + "Returned ");

            string returnType = method.Descriptor.Split(')')[1];
            PrintValue(returnType, returnValue);
            PrintWithColor(DebugDefaultColor, "\n");
        }

        public void PrintMethodReturn(MethodInfo method, long returnValue)
        {
            Depth--;
            PrintWithColor(DebugDefaultColor, LeftPad + "Returned ");

            MethodInfo methodInfo = Program.MethodFrameStack.Peek().MethodInfo;
            string returnType = methodInfo.Descriptor.Split(')')[1];
            PrintWideValue(returnType[0], returnValue);
            PrintWithColor(DebugDefaultColor, "\n");
        }

        public void PrintMethodReturn(MethodInfo method)
        {
            Depth--;
            PrintWithColor(DebugDefaultColor, LeftPad + "Returned void\n");
        }

        public void PrintMethodThrewException(MethodInfo method, JavaException exception)
        {
            Depth--;
            PrintWithColor(ExceptionThrownColor, $"{LeftPad}Threw {exception.ClassFile.Name} ({exception.Message})\n");
        }

        private void PrintArgs(byte methodColor, string descriptor, bool isStatic, int[] args)
        {
            int argIndex = 0;
            int i;
            PrintWithColor(methodColor, "(");
            if (!isStatic)
            {
                int callerAddr = args[0];
                if (Heap.GetObject(callerAddr) is HeapArray heapArr)
                {
                    PrintArrayValue(JavaHelper.ClassObjectName(heapArr.ItemTypeClassObjAddr), callerAddr);
                }
                else
                {
                    ClassFile argCFile = Heap.GetObject(callerAddr).ClassFile;
                    PrintObjectValue(argCFile.Name, callerAddr);
                }
                argIndex++;
            }
            for (i = 1; descriptor[i] != ')';)
            {
                if (!isStatic || i != 1)
                {
                    PrintWithColor(methodColor, ", ");
                }
                string argumentType = JavaHelper.ReadDescriptorArg(descriptor, ref i);
                if (argumentType[0] == 'J' || argumentType[0] == 'D')
                {
                    long argument = (args[argIndex], args[argIndex + 1]).ToLong();
                    PrintWideValue(argumentType[0], argument);
                    argIndex += 2;
                }
                else
                {
                    int argument = args[argIndex];
                    PrintValue(argumentType, argument);
                    argIndex++;
                }
            }
            PrintWithColor(methodColor, ")");
        }

        private void PrintWideValue(char typeDescriptor, long val)
        {
            if (typeDescriptor == 'J')
            {
                PrintWithColor(longColor, val.ToString());
            }
            else
            {
                PrintWithColor(doubleColor, JavaHelper.StoredDoubleToDouble(val).ToString());
            }
        }

        private void PrintArrayValue(string type, int address)
        {
            if (address != 0)
            {
                HeapArray heapArr = Heap.GetArray(address);
                string itemTypeFromArg = JavaHelper.ClassObjectName(heapArr.ItemTypeClassObjAddr);
                if (!JavaHelper.IsPrimitiveType(itemTypeFromArg))
                {
                    type = JavaHelper.ClassObjectName(heapArr.ItemTypeClassObjAddr).Replace('.', '/');
                }
            }

            int i = -1;
            do
            {
                PrintWithColor(arrayBracketColor, "[");
                i++;
            } while (type[i] == '[');

            if (type[i] == 'L')
            {
                PrintWithColor(classNameColor, type.Substring(i + 1, type.Length - i - 2));
            }
            else
            {
                PrintWithColor(classNameColor, type.Substring(i));
            }

            PrintWithColor(separatorColor, "/");

            if (address == 0)
            {
                PrintWithColor(nullColor, "Null");
            }
            else
            {
                PrintWithColor(objAddrColor, address.ToString());
            }
        }

        private void PrintObjectValue(string type, int address)
        {
            if (address == 0)
            {
                PrintWithColor(classNameColor, type);
                PrintWithColor(separatorColor, "/");
                PrintWithColor(nullColor, "Null");
            }
            else
            {
                ClassFile argCFile = Heap.GetObject(address).ClassFile;
                if (argCFile.Name == "java/lang/String")
                {
                    FieldReferenceValue charArr = (FieldReferenceValue)Heap.GetObject(address).GetField("value", "[C");
                    if (charArr.Address == 0)
                    {
                        PrintWithColor(classNameColor, Heap.GetObject(address).ClassFile.Name);
                    }
                    else
                    {
                        PrintWithColor(stringColor, '"' + JavaHelper.ReadJavaString(address) + '"');
                    }
                }
                else if (argCFile.Name == "java/lang/Class")
                {
                    PrintWithColor(classObjColor, JavaHelper.ClassObjectName(address));
                }
                else
                {
                    PrintWithColor(classNameColor, Heap.GetObject(address).ClassFile.Name);
                }

                PrintWithColor(separatorColor, "/");
                PrintWithColor(objAddrColor, address.ToString());
            }
        }

        private void PrintValue(string type, int val)
        {
            if (type[0] == 'L')
            {
                PrintObjectValue(type.Substring(1, type.Length - 2), val);
            }
            else if (type[0] == '[')
            {
                PrintArrayValue(type.Substring(1), val);
            }
            else if (type[0] == 'Z')
            {
                PrintWithColor(booleanColor, val != 0 ? "True" : "False");
            }
            else if (type[0] == 'C')
            {
                PrintWithColor(charColor, "'" + (char)val + "'");
            }
            else if (type[0] == 'F')
            {
                PrintWithColor(floatColor, JavaHelper.StoredFloatToFloat(val).ToString());
            }
            else
            {
                switch (type[0])
                {
                    case 'B':
                        PrintWithColor(byteColor, val.ToString());
                        break;
                    case 'I':
                        PrintWithColor(integerColor, val.ToString());
                        break;
                    case 'S':
                        PrintWithColor(shortColor, val.ToString());
                        break;
                }
            }
        }
    }
}
