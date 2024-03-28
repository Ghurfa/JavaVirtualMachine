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
