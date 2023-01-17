namespace JavaVirtualMachine
{
    public static class DebugWriter
    {
        const int Spacing = 2;
        const ConsoleColor DebugDefaultColor = ConsoleColor.DarkGray;

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

        //Fields
        const ConsoleColor fieldNameColor = ConsoleColor.Yellow;
        const ConsoleColor fieldTypeColor = ConsoleColor.Cyan;

        public static void WriteDebugMessage(string message)
        {
            Console.ForegroundColor = DebugDefaultColor;
            Console.WriteLine(message);
        }

        private static void WriteArrayValue(string itemType, int argument)
        {
            if (argument != 0)
            {
                HeapArray heapArr = Heap.GetArray(argument);
                string itemTypeFromArg = JavaHelper.ClassObjectName(heapArr.ItemTypeClassObjAddr);
                if (!JavaHelper.IsPrimitiveType(itemTypeFromArg))
                {
                    itemType = itemTypeFromArg.Replace('.', '/');
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

        private static void WriteObjectValue(string type, int address)
        {
            if (address == 0)
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
                HeapObject obj = Heap.GetObject(address);
                ClassFile argCFile = obj.ClassFile;
                if (argCFile.Name == "java/lang/String")
                {
                    Console.ForegroundColor = stringColor;
                    int charArrAddr = obj.GetField("value", "[C");
                    if (charArrAddr == 0)
                    {
                        Console.ForegroundColor = classNameColor;
                        Console.Write(obj.ClassFile.Name);
                    }
                    else
                    {
                        Console.Write('"' + JavaHelper.ReadJavaString(address) + '"');
                    }
                }
                else if (argCFile.Name == "java/lang/Class")
                {
                    Console.ForegroundColor = classObjColor;
                    Console.Write(JavaHelper.ClassObjectName(address));
                }
                else
                {
                    Console.ForegroundColor = classNameColor;
                    Console.Write(obj.ClassFile.Name);
                }

                Console.ForegroundColor = separatorColor;
                Console.Write('/');

                Console.ForegroundColor = objAddrColor;
                Console.Write(address);
            }
        }

        private static void WriteFieldValue(FieldInfo fieldInfo, long fieldValue)
        {
            Console.ForegroundColor = fieldTypeColor;
            Console.Write(fieldInfo.Descriptor);
            Console.ForegroundColor = separatorColor;
            Console.Write(':');
            switch (fieldInfo.Descriptor)
            {
                case "Z":
                    Console.ForegroundColor = booleanColor;
                    Console.Write(fieldValue != 0 ? "True" : "False");
                    break;
                case "C":
                    Console.ForegroundColor = charColor;
                    Console.Write("'" + (char)fieldValue + "'");
                    break;
                case "F":
                    Console.ForegroundColor = floatColor;
                    Console.Write(JavaHelper.StoredFloatToFloat((int)fieldValue));
                    break;
                case "B":
                    Console.ForegroundColor = byteColor;
                    Console.Write((byte)fieldValue);
                    break;
                case "I":
                    Console.ForegroundColor = integerColor;
                    Console.Write((int)fieldValue);
                    break;
                case "S":
                    Console.ForegroundColor = shortColor;
                    Console.Write((short)fieldValue);
                    break;
                case "J":
                    Console.ForegroundColor = longColor;
                    Console.Write(fieldValue);
                    break;
                case "D":
                    Console.ForegroundColor = doubleColor;
                    Console.Write(JavaHelper.StoredDoubleToDouble(fieldValue).ToString());
                    break;
                default:
                    if (fieldInfo.Descriptor[0] == '[')
                    {
                        WriteArrayValue(fieldInfo.Descriptor.Substring(1), (int)fieldValue);
                    }
                    else if (fieldInfo.Descriptor[0] == 'L')
                    {
                        WriteObjectValue(fieldInfo.Descriptor.Substring(1, fieldInfo.Descriptor.Length - 2), (int)fieldValue);
                    }
                    else throw new InvalidOperationException();

                    break;
            }
        }

        public static void PrintStack()
        {
            MethodFrame[] stack = new MethodFrame[Program.MethodFrameStack.Count];
            PrintStack(stack);
        }

        public static void PrintStack(MethodFrame[] stack)
        {
            Console.ForegroundColor = DebugDefaultColor;
            Console.WriteLine("\nStack:");
            Program.MethodFrameStack.CopyTo(stack, 0);
            for (int i = stack.Length - 1; i >= 0; i--)
            {
                Console.CursorLeft = (stack.Length - 1 - i) * Spacing;
                MethodFrame frame = stack[i];
                Console.WriteLine($"{frame.ClassFile.Name}.{frame.MethodInfo.Name}{frame.MethodInfo.Descriptor}");
            }
        }

        public static void PrintObject(int objAddr)
        {
            Console.WriteLine();
            HeapObject obj = Heap.GetObject(objAddr);

            string className = obj.ClassFile.Name;
            Console.ForegroundColor = classNameColor;
            Console.Write(className);
            Console.ForegroundColor = separatorColor;
            Console.Write('/');
            Console.ForegroundColor = objAddrColor;
            Console.WriteLine(objAddr);

            foreach (FieldInfo field in obj.ClassFile.InstanceFields)
            {
                Console.ForegroundColor = fieldNameColor;
                Console.Write(field.Name);
                Console.ForegroundColor = separatorColor;
                Console.Write('/');
                WriteFieldValue(field, obj.GetField(field.Name, field.Descriptor));
                Console.WriteLine();
            }
        }
    }
}
