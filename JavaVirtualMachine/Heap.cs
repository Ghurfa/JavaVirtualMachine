using JavaVirtualMachine.Heaps;
using System.Buffers.Binary;
using System.Net;
using System.Runtime.InteropServices;

namespace JavaVirtualMachine
{
    public static class Heap
    {
        private readonly static IHeap Implementation = new NoncollectingHeap(4096);

        public static int ObjectFieldOffset => Implementation.ObjectFieldOffset;
        public static int ObjectFieldSize => Implementation.ObjectFieldSize;
        public static int ArrayMarkOffset => Implementation.ArrayMarkOffset;
        public static int ArrayItemTypeOffset => Implementation.ArrayItemTypeOffset;
        public static int ArrayLengthOffset => Implementation.ArrayLengthOffset;
        public static int ArrayBaseOffset => Implementation.ArrayBaseOffset;

        public static int CreateObject(int classFileIdx) => Implementation.CreateObject(classFileIdx);
        public static int CreateArray(int[] arr, int itemTypeClassObjAddr) => Implementation.CreateArray(arr, itemTypeClassObjAddr);
        public static int CreateArray(int itemSize, int itemCount, int itemTypeClassObjAddr) => Implementation.CreateArray(itemSize, itemCount, itemTypeClassObjAddr);
        public static int CloneObject(int address) => Implementation.CloneObject(address);

        public static HeapObject GetObject(int address) => Implementation.GetObject(address);
        public static HeapArray GetArray(int address) => Implementation.GetArray(address);

        public static byte GetByte(int address) => Implementation.GetByte(address);
        public static short GetShort(int address) => Implementation.GetShort(address);
        public static int GetInt(int address) => Implementation.GetInt(address);
        public static long GetLong(int address) => Implementation.GetLong(address);
        public static Span<byte> GetSpan(int address, int length) => Implementation.GetSpan(address, length);

        public static void PutByte(int address, byte value) => Implementation.PutByte(address, value);
        public static void PutShort(int address, short value) => Implementation.PutShort(address, value);
        public static void PutInt(int address, int value) => Implementation.PutInt(address, value);
        public static void PutLong(int address, long value) => Implementation.PutLong(address, value);

        public static int AllocateMemory(long size) => Implementation.AllocateMemory(size);
    }
}
