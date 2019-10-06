using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace JavaVirtualMachine
{
    static class Heap
    {
        //todo: heap
        static Memory<byte> memory = new Memory<byte>(new byte[4096]);
        static int itemIndex = 8;
        static Dictionary<int, HeapItem> heapItems = new Dictionary<int, HeapItem>()
        {
            {0, null }
        };
        public static int AddItem(HeapItem obj)
        {
            heapItems.Add(obj.Address, obj);
            return obj.Address;
        }
        public static HeapItem GetItem(int index)
        {
            return heapItems[index];
        }
        public static HeapObject GetObject(int index)
        {
            return (HeapObject)heapItems[index];
        }
        public static byte GetByte(int address)
        {
            return memory.Slice(address, 1).ToArray()[0];
        }
        public static int GetInt(int address)
        {
            return memory.Slice(address, 4).ToArray().ToInt();
        }
        public static long GetLong(int address)
        {
            return memory.Slice(address, 8).ToArray().ToLong();
        }
        public static void PutLong(long address, long value)
        {
            value.AsByteArray().CopyTo(memory.Slice((int)address, 8));
        }
        public static void PutData(long address, byte[] data)
        {
           data.CopyTo(memory.Slice((int)address, data.Length));
        }
        public static void Fill(long address, long numOfBytes, byte value)
        {
            memory.Slice((int)address, (int)numOfBytes).Span.Fill(value);
        }
        public static int AllocateMemory(long size)
        {
            if (itemIndex + size > memory.Length)
            {
                ExpandMemory();
            }
            int startOfMemory = itemIndex;
            itemIndex += (int)size;
            if (itemIndex % 8 != 0)
            {
                AllocateMemory(8 - (itemIndex % 8));
            }
            return startOfMemory;
        }
        public static Memory<byte> GetMemorySlice(int startAddress, int size)
        {
            return memory.Slice(startAddress, size);
        }
        private static void ExpandMemory()
        {
            byte[] newMemory = new byte[memory.Length * 2];
            memory.ToArray().CopyTo((Memory<byte>)newMemory);
            memory = new Memory<byte>(newMemory);
        }
    }
}
