using System.Runtime.InteropServices;

namespace JavaVirtualMachine
{
    public static class Utility
    {
        public static ushort SwapEndian(this ref ushort data) => data = (ushort)((data << 8) | (data >> 8));
        public static uint SwapEndian(this ref uint data) => data = ((data << 24) | ((data << 8) & 0xFF0000) | ((data >> 8) & 0xFF00) | (data >> 24));
        public static byte ReadOne(this ref ReadOnlySpan<byte> span)
        {
            byte nibble = span[0];
            span = span.Slice(1);
            return nibble;
        }
        public static ushort ReadTwo(this ref ReadOnlySpan<byte> span)
        {
            ushort dwarf = MemoryMarshal.Cast<byte, ushort>(span)[0];
            span = span.Slice(2);
            return dwarf.SwapEndian();
        }
        public static ushort ReadTwo(this ref ReadOnlyMemory<byte> memory)
        {
            ushort dwarf = MemoryMarshal.Cast<byte, ushort>(memory.Span)[0];
            memory = memory.Slice(2);
            return dwarf.SwapEndian();
        }
        public static uint ReadFour(this ref ReadOnlySpan<byte> span)
        {
            uint num = MemoryMarshal.Cast<byte, uint>(span)[0];
            span = span.Slice(4);
            return num.SwapEndian();
        }
        public static uint ReadFour(this ref ReadOnlyMemory<byte> memory)
        {
            uint num = MemoryMarshal.Cast<byte, uint>(memory.Span)[0];
            memory = memory.Slice(4);
            return num.SwapEndian();
        }

        public static void Push(ref Span<int> stack, ref int stackPointer, int value)
        {
            stack[stackPointer++] = value;
        }
        public static void Push(ref Span<int> stack, ref int stackPointer, Span<int> values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                stack[stackPointer++] = values[i];
            }
        }
        public static void Push(ref Span<int> stack, ref int stackPointer, long value)
        {
            stack[stackPointer++] = (int)(value >> (8 * sizeof(int)));
            stack[stackPointer++] = (int)(value & 0xFFFFFFFF);
        }
        public static void Push(ref Span<int> stack, ref int stackPointer, float value)
        {
            stack[stackPointer++] = JavaHelper.FloatToStoredFloat(value);
        }
        public static void Push(ref Span<int> stack, ref int stackPointer, double value)
        {
            Push(ref stack, ref stackPointer, JavaHelper.DoubleToStoredDouble(value));
        }

        public static int PopInt(Span<int> stack, ref int stackPointer)
        {
            return stack[--stackPointer];
        }

        public static int PeekInt(Span<int> stack, int stackPointer, int offset = 0)
        {
            return stack[stackPointer - 1 - offset];
        }

        public static long PeekLong(Span<int> stack, int stackPointer, int offset = 0)
        {
            int lowInt = PeekInt(stack, stackPointer, offset);
            int highInt = PeekInt(stack, stackPointer, offset + 1);
            return (((long)highInt) << 32) | (lowInt & 0xFFFFFFFF);
        }

        public static long PopLong(Span<int> stack, ref int stackPointer)
        {
            int lowInt = PopInt(stack, ref stackPointer);
            int highInt = PopInt(stack, ref stackPointer);
            return (((long)highInt) << 32) | (lowInt & 0xFFFFFFFF);
        }

        public static float PopFloat(Span<int> stack, ref int stackPointer)
        {
            int storedValue = stack[--stackPointer];
            return JavaHelper.StoredFloatToFloat(storedValue);
        }

        public static double PopDouble(Span<int> stack, ref int stackPointer)
        {
            long storedValue = PopLong(stack, ref stackPointer);
            return JavaHelper.StoredDoubleToDouble(storedValue);
        }

        public static long ToLong(this (int high, int low) pair)
        {
            return (((long)pair.high) << 32) | (pair.low & 0xFFFFFFFF);
        }

        public static (int high, int low) Split(this long value)
        {
            return ((int)(value >> 32), (int)(value & 0xFFFFFFFF));
        }
    }
}
