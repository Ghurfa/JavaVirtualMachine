using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine
{
    public static class DebugWriter
    {
        public static int Depth = 0;
        public static bool WriteDebugMessages = true;
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

        //Fields
        const ConsoleColor fieldNameColor = ConsoleColor.Yellow;
        const ConsoleColor fieldTypeColor = ConsoleColor.Cyan;

        public static void WriteDebugMessage(string message)
        {
            Console.ForegroundColor = DebugDefaultColor;
            Console.WriteLine(message);
        }

        private static void WriteWideValue(char character, long argument)
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
        private static void WriteArrayValue(string itemType, int argument)
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
        private static void WriteObjectValue(string type, int argument)
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
        private static void WriteFieldValue(FieldInfo fieldInfo, FieldValue fieldValue)
        {
            Console.ForegroundColor = fieldTypeColor;
            Console.Write(fieldInfo.Descriptor);
            Console.ForegroundColor = separatorColor;
            Console.Write(':');
            switch (fieldValue)
            {
                case FieldNumber number:
                    switch (fieldInfo.Descriptor)
                    {
                        case "Z":
                            Console.ForegroundColor = booleanColor;
                            Console.Write(number.Value != 0 ? "True" : "False");
                            break;
                        case "C":
                            Console.ForegroundColor = charColor;
                            Console.Write("'" + (char)number.Value + "'");
                            break;
                        case "F":
                            Console.ForegroundColor = floatColor;
                            Console.Write(JavaHelper.StoredFloatToFloat(number.Value));
                            break;
                        case "B":
                            Console.ForegroundColor = byteColor;
                            Console.Write(number.Value);
                            break;
                        case "I":
                            Console.ForegroundColor = integerColor;
                            Console.Write(number.Value);
                            break;
                        case "S":
                            Console.ForegroundColor = shortColor;
                            Console.Write(number.Value);
                            break;
                    }
                    break;
                case FieldLargeNumber largeNumber:
                    WriteWideValue(fieldInfo.Descriptor[0], largeNumber.Value);
                    break;
                case FieldReferenceValue referenceValue:
                    if (fieldInfo.Descriptor[0] == '[')
                    {
                        WriteArrayValue(fieldInfo.Descriptor.Substring(1), referenceValue.Address);
                    }
                    else
                    {
                        WriteObjectValue(fieldInfo.Descriptor.Substring(1, fieldInfo.Descriptor.Length - 2), referenceValue.Address);
                    }
                    break;
                default:
                    throw new InvalidOperationException();
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
        public static void PrintObject(int objAddr) => PrintObject(Heap.GetObject(objAddr));
        public static void PrintObject(HeapObject obj)
        {
            Console.WriteLine();

            string className = obj.ClassFile.Name;
            Console.ForegroundColor = classNameColor;
            Console.Write(className);
            Console.ForegroundColor = separatorColor;
            Console.Write('/');
            Console.ForegroundColor = objAddrColor;
            Console.WriteLine(obj.Address);

            foreach (FieldInfo field in obj.ClassFile.InstanceFields())
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
