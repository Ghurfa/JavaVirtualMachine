using JavaVirtualMachine.ConstantPoolInfo;
using System.Reflection;

namespace JavaVirtualMachine.StackTrace
{
    internal class ColorfulStackTracePrinter : IStackTracePrinter
    {
        public static int Depth = 0;
        const int Spacing = 2;
        const ConsoleColor DebugDefaultColor = ConsoleColor.DarkGray;
        const ConsoleColor NativeMethodColor = ConsoleColor.Green;
        const ConsoleColor ExceptionThrownColor = ConsoleColor.Yellow;

        //Specially printed
        const ConsoleColor arrayBracketColor = ConsoleColor.Red;
        const ConsoleColor nullColor = ConsoleColor.DarkRed;
        const ConsoleColor stringColor = ConsoleColor.DarkGreen;
        const ConsoleColor classObjColor = ConsoleColor.DarkBlue;
        const ConsoleColor booleanColor = ConsoleColor.DarkRed;
        const ConsoleColor charColor = ConsoleColor.DarkGreen;

        //Objects
        const ConsoleColor classNameColor = ConsoleColor.Blue;
        const ConsoleColor separatorColor = ConsoleColor.White;
        const ConsoleColor objAddrColor = ConsoleColor.Cyan;

        //Numbers - could be confused with each other
        const ConsoleColor byteColor = ConsoleColor.DarkGreen;
        const ConsoleColor floatColor = ConsoleColor.DarkBlue;
        const ConsoleColor integerColor = ConsoleColor.White;
        const ConsoleColor shortColor = ConsoleColor.DarkYellow;
        const ConsoleColor longColor = ConsoleColor.DarkMagenta;
        const ConsoleColor doubleColor = ConsoleColor.DarkRed;

        //Singleton
        public static ColorfulStackTracePrinter Instance { get; } = new();

        private ColorfulStackTracePrinter()
        {

        }

        public void PrintMethodCall(MethodInfo method, int[] args, CInterfaceMethodRefInfo? interfaceMethod = null)
        {
            if (method.HasFlag(MethodInfoFlag.Native))
            {
                Console.ForegroundColor = NativeMethodColor;
            }
            else
            {
                Console.ForegroundColor = DebugDefaultColor;
            }
            Console.Write($"{new string(' ', Depth * Spacing)}{method.ClassFile.Name}.{method.Name}");
            PrintArgs(method.Descriptor, method.HasFlag(MethodInfoFlag.Static), args);
            if (interfaceMethod != null)
            {
                Console.Write($"   (interface {interfaceMethod.ClassName})");
            }
            else
            {
                Console.WriteLine();
            }
            Depth++;
        }

        public void PrintMethodReturn(MethodInfo method, int returnValue)
        {
            Depth--;
            Console.ForegroundColor = DebugDefaultColor;
            Console.CursorLeft = Depth * Spacing;
            Console.Write("Returned ");

            string returnType = method.Descriptor.Split(')')[1];
            PrintValue(returnType, returnValue);
            Console.WriteLine();
        }

        public void PrintMethodReturn(MethodInfo method, long returnValue)
        {
            Depth--;
            Console.ForegroundColor = DebugDefaultColor;
            Console.CursorLeft = Depth * Spacing;
            Console.Write("Returned ");

            MethodInfo methodInfo = Program.MethodFrameStack.Peek().MethodInfo;
            string returnType = methodInfo.Descriptor.Split(')')[1];
            PrintWideValue(returnType[0], returnValue);
            Console.WriteLine();
        }

        public void PrintMethodReturn(MethodInfo method)
        {
            Depth--;
            Console.ForegroundColor = DebugDefaultColor;
            Console.WriteLine($"{new string(' ', Depth * Spacing)}Returned void");
        }

        public void PrintMethodThrewException(MethodInfo method, JavaException exception)
        {
            Depth--;
            Console.ForegroundColor = ExceptionThrownColor;
            Console.WriteLine($"{new string(' ', Depth * Spacing)}Threw {exception.ClassFile.Name} ({exception.Message})");
        }

        private void PrintArgs(string descriptor, bool isStatic, int[] args)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            int argIndex = 0;
            int i;
            Console.Write("(");
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
                    Console.Write(", ");
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
            Console.ForegroundColor = originalColor;
            Console.Write(')');
        }

        private void PrintWideValue(char character, long argument)
        {
            if (character == 'J')
            {
                Console.ForegroundColor = longColor;
                Console.Write(argument.ToString());
            }
            else
            {
                Console.ForegroundColor = doubleColor;
                Console.Write(JavaHelper.StoredDoubleToDouble(argument).ToString());
            }
        }

        private void PrintArrayValue(string itemType, int argument)
        {
            if (argument != 0)
            {
                HeapArray heapArr = Heap.GetArray(argument);
                string itemTypeFromArg = JavaHelper.ClassObjectName(heapArr.ItemTypeClassObjAddr);
                if (!JavaHelper.IsPrimitiveType(itemTypeFromArg))
                {
                    itemType = JavaHelper.ClassObjectName(heapArr.ItemTypeClassObjAddr).Replace('.', '/');
                }
            }
            Console.ForegroundColor = arrayBracketColor;
            int i = -1;
            do
            {
                Console.Write('[');
                i++;
            } while (itemType[i] == '[');

            Console.ForegroundColor = classNameColor;
            if (itemType[i] == 'L')
            {
                Console.Write(itemType.Substring(i + 1, itemType.Length - i - 2));
            }
            else
            {
                Console.Write(itemType.Substring(i));
            }

            Console.ForegroundColor = separatorColor;
            Console.Write("/");

            if (argument == 0)
            {
                Console.ForegroundColor = nullColor;
                Console.Write("Null");
            }
            else
            {
                Console.ForegroundColor = objAddrColor;
                Console.Write(argument);
            }
        }

        private void PrintObjectValue(string type, int argument)
        {
            if (argument == 0)
            {
                Console.ForegroundColor = classNameColor;
                Console.Write(type);

                Console.ForegroundColor = separatorColor;
                Console.Write('/');

                Console.ForegroundColor = nullColor;
                Console.Write("Null");
            }
            else
            {
                ClassFile argCFile = Heap.GetObject(argument).ClassFile;
                if (argCFile.Name == "java/lang/String")
                {
                    Console.ForegroundColor = stringColor;
                    FieldReferenceValue charArr = (FieldReferenceValue)Heap.GetObject(argument).GetField("value", "[C");
                    if (charArr.Address == 0)
                    {
                        Console.ForegroundColor = classNameColor;
                        Console.Write(Heap.GetObject(argument).ClassFile.Name);
                    }
                    else
                    {
                        Console.Write('"' + JavaHelper.ReadJavaString(argument) + '"');
                    }
                }
                else if (argCFile.Name == "java/lang/Class")
                {
                    Console.ForegroundColor = classObjColor;
                    Console.Write(JavaHelper.ClassObjectName(argument));
                }
                else
                {
                    Console.ForegroundColor = classNameColor;
                    Console.Write(Heap.GetObject(argument).ClassFile.Name);
                }
                Console.ForegroundColor = separatorColor;
                Console.Write('/');

                Console.ForegroundColor = objAddrColor;
                Console.Write(argument);
            }
        }

        private void PrintValue(string argumentType, int argument)
        {
            if (argumentType[0] == 'L')
            {
                PrintObjectValue(argumentType.Substring(1, argumentType.Length - 2), argument);
            }
            else if (argumentType[0] == '[')
            {
                PrintArrayValue(argumentType.Substring(1), argument);
            }
            else if (argumentType[0] == 'Z')
            {
                Console.ForegroundColor = booleanColor;
                Console.Write(argument != 0 ? "True" : "False");
            }
            else if (argumentType[0] == 'C')
            {
                Console.ForegroundColor = charColor;
                Console.Write("'" + (char)argument + "'");
            }
            else if (argumentType[0] == 'F')
            {
                Console.ForegroundColor = floatColor;
                Console.Write(JavaHelper.StoredFloatToFloat(argument));
            }
            else
            {
                switch (argumentType[0])
                {
                    case 'B':
                        Console.ForegroundColor = byteColor;
                        break;
                    case 'I':
                        Console.ForegroundColor = integerColor;
                        break;
                    case 'S':
                        Console.ForegroundColor = shortColor;
                        break;
                }
                Console.Write(argument);
            }
        }
    }
}
