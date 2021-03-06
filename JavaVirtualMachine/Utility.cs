﻿using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

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

        public static void Push(ref int[] stack, ref int stackPointer, int value)
        {
            stack[stackPointer++] = value;
        }
        public static void Push(ref int[] stack, ref int stackPointer, int[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                stack[stackPointer++] = values[i];
            }
        }
        public static void Push(ref int[] stack, ref int stackPointer, long value)
        {
            stack[stackPointer++] = (int)(value >> (8 * sizeof(int)));
            stack[stackPointer++] = (int)(value & 0xFFFFFFFF);
        }
        public static void Push(ref int[] stack, ref int stackPointer, float value)
        {
            stack[stackPointer++] = JavaHelper.FloatToStoredFloat(value);
        }
        public static void Push(ref int[] stack, ref int stackPointer, double value)
        {
            Push(ref stack, ref stackPointer, JavaHelper.DoubleToStoredDouble(value));
        }

        public static int PopInt(int[] stack, ref int stackPointer)
        {
            return stack[--stackPointer];
        }
        public static int PeekInt(int[] stack, int stackPointer, int offset = 0)
        {
            return stack[stackPointer - 1 - offset];
        }
        public static long PeekLong(int[] stack, int stackPointer, int offset = 0)
        {
            int lowInt = PeekInt(stack, stackPointer, offset);
            int highInt = PeekInt(stack, stackPointer, offset + 1);
            return (((long)highInt) << 32) | (lowInt & 0xFFFFFFFF);
        }
        public static long PopLong(int[] stack, ref int stackPointer)
        {
            int lowInt = PopInt(stack, ref stackPointer);
            int highInt = PopInt(stack, ref stackPointer);
            return (((long)highInt) << 32) | (lowInt & 0xFFFFFFFF);
        }
        public static float PopFloat(int[] stack, ref int stackPointer)
        {
            int storedValue = stack[--stackPointer];
            return JavaHelper.StoredFloatToFloat(storedValue);
        }
        public static double PopDouble(int[] stack, ref int stackPointer)
        {
            long storedValue = PopLong(stack, ref stackPointer);
            return JavaHelper.StoredDoubleToDouble(storedValue);
        }

        public static MethodFrame Peek(this Stack<MethodFrame> stack, int depth)
        {
            MethodFrame[] asArray = stack.ToArray();
            return asArray[depth];
        }

        public static long ToLong(this (int high, int low) pair)
        {
            return (((long)pair.high) << 32) | (pair.low & 0xFFFFFFFF);
        }
        public static (int high, int low) Split(this long value)
        {
            return ((int)(value >> 32), (int)(value & 0xFFFFFFFF));
        }

        public static byte[] AsByteArray(this long value)
        {
            byte[] array = new byte[sizeof(long) / sizeof(byte)];
            for(int i = 0; i < array.Length; i++)
            {
                int shiftAmount = 8 * (array.Length - i - 1);
                long shifted = value >> shiftAmount;

                array[i] = (byte)(shifted & 0xFF);
            }
            return array;
        }
        public static byte[] AsByteArray(this int value)
        {
            byte[] array = new byte[sizeof(int) / sizeof(byte)];
            for (int i = 0; i < array.Length; i++)
            {
                int shiftAmount = 8 * (array.Length - i - 1);
                long shifted = value >> shiftAmount;

                array[i] = (byte)(shifted & 0xFF);
            }
            return array;
        }

        public static int ToInt(this byte[] array)
        {
            if (array.Length != 4) throw new InvalidOperationException();
            int value = 0;
            for (int i = 0; i < sizeof(int) / sizeof(byte); i++)
            {
                value <<= 8;
                value |= array[i];
            }
            return value;
        }
        public static long ToLong(this byte[] array)
        {
            if (array.Length != 8) throw new InvalidOperationException();
            long value = 0;
            for (int i = 0; i < sizeof(long) / sizeof(byte); i++)
            {
                value <<= 8;
                value |= array[i];
            }
            return value;
        }
    }
}
