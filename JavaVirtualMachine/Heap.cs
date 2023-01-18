using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace JavaVirtualMachine
{
    static class Heap
    {
        private static Memory<byte> Memory = new Memory<byte>(new byte[4096]);
        private static int itemIndex = 16;

        public const int ObjectFieldOffset = 8;
        public const int ObjectFieldSize = 8;
        public const int ArrayMarkOffset = 4;
        public const int ArrayItemTypeOffset = 8;
        public const int ArrayLengthOffset = 12;
        public const int ArrayBaseOffset = 16;

        public static int CreateObject(int classFileIdx)
        {
            ClassFile cFile = ClassFileManager.ClassFiles[classFileIdx];
            int numBytes = 8 + 8 * cFile.InstanceFields.Count;
            int addr = AllocateMemory(numBytes);
            BinaryPrimitives.WriteInt32LittleEndian(Memory.Slice(addr).Span, classFileIdx);
            return addr;
        }

        public static int CloneObject(int address)
        {
            int cFileIdx = BinaryPrimitives.ReadInt32LittleEndian(Memory.Slice(address).Span);
            bool isArray = BinaryPrimitives.ReadInt32LittleEndian(Memory.Slice(address + ArrayMarkOffset).Span) != 0;

            int numBytes;
            if (isArray)
            {
                int itemTypeClassObjAddr = BinaryPrimitives.ReadInt32LittleEndian(Memory.Slice(address + 8).Span);
                int itemSize = GetArrayItemSize(itemTypeClassObjAddr);
                int itemCount = BinaryPrimitives.ReadInt32LittleEndian(Memory.Slice(address + ArrayLengthOffset).Span);

                numBytes = ArrayBaseOffset + itemSize * itemCount;
            }
            else
            {
                ClassFile cFile = ClassFileManager.ClassFiles[cFileIdx];
                numBytes = ObjectFieldOffset + ObjectFieldSize * cFile.InstanceFields.Count;
            }

            int newObjAddr = AllocateMemory(numBytes);
            Memory.Slice(address, numBytes).CopyTo(Memory.Slice(newObjAddr));
            return newObjAddr;
        }

        public static int CreateArray(int[] arr, int itemTypeClassObjAddr)
        {
            int addr = CreateArray(4, arr.Length, itemTypeClassObjAddr);

            Span<int> arrData = MemoryMarshal.Cast<byte, int>(Memory.Slice(addr + ArrayBaseOffset).Span); // Store data
            arr.CopyTo(arrData);
            return addr;
        }

        public static int CreateArray(int itemSize, int itemCount, int itemTypeClassObjAddr)
        {
            int numBytes = ArrayBaseOffset + itemSize * itemCount;
            int addr = AllocateMemory(numBytes);
            int objectCFileIdx = ClassFileManager.GetClassFileIndex("java/lang/Object");
            BinaryPrimitives.WriteInt32LittleEndian(Memory.Slice(addr).Span, objectCFileIdx); // Store type
            Memory.Span[addr + ArrayMarkOffset] = 1; // Mark as array
            BinaryPrimitives.WriteInt32LittleEndian(Memory.Slice(addr + ArrayItemTypeOffset).Span, itemTypeClassObjAddr); // Store item type
            BinaryPrimitives.WriteInt32LittleEndian(Memory.Slice(addr + ArrayLengthOffset).Span, itemCount); // Store length

            return addr;
        }

        public static HeapObject GetObject(int address)
        {
            if (address == 0) return null;

            bool isArray = BinaryPrimitives.ReadInt32LittleEndian(Memory.Slice(address + ArrayMarkOffset).Span) != 0;

            if (isArray)
            {
                return GetArray(address);
            }

            return new HeapObject(address);
        }

        public static HeapArray GetArray(int address)
        {
            if (address == 0) return null;

            int itemTypeClassObjAddr = BinaryPrimitives.ReadInt32LittleEndian(Memory.Slice(address + ArrayItemTypeOffset).Span);

            return new HeapArray(address, GetArrayItemSize(itemTypeClassObjAddr));
        }

        public static byte GetByte(int address) => Memory.Slice(address).Span[0];
        public static short GetShort(int address) => BinaryPrimitives.ReadInt16LittleEndian(Memory.Slice(address).Span);
        public static int GetInt(int address) => BinaryPrimitives.ReadInt32LittleEndian(Memory.Slice(address).Span);
        public static long GetLong(int address) => BinaryPrimitives.ReadInt64LittleEndian(Memory.Slice(address).Span);
        public static Span<byte> GetSpan(int address, int length) => Memory.Slice(address, length).Span;
        //public static Memory<byte> GetMemorySlice(int startAddress, int size) => Memory.Slice(startAddress, size);

        public static void PutByte(int address, byte value) => Memory.Slice(address, 1).Span[0] = value;
        public static void PutShort(int address, short value) => BinaryPrimitives.WriteInt16LittleEndian(Memory.Slice(address).Span, value);
        public static void PutInt(int address, int value) => BinaryPrimitives.WriteInt32LittleEndian(Memory.Slice(address).Span, value);
        public static void PutLong(int address, long value) => BinaryPrimitives.WriteInt64LittleEndian(Memory.Slice(address).Span, value);

        public static int AllocateMemory(long size)
        {
            if (itemIndex + size > Memory.Length)
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

        private static void ExpandMemory()
        {
            Memory<byte> newMemory = new(new byte[Memory.Length * 2]);
            Memory.CopyTo(newMemory);
            Memory = newMemory;
        }

        private static int GetArrayItemSize(int itemTypeClassObjAddr)
        {
            int doubleClassObject = ClassObjectManager.GetClassObjectAddr("double");
            int longClassObject = ClassObjectManager.GetClassObjectAddr("long");
            int shortClassObject = ClassObjectManager.GetClassObjectAddr("short");
            int charClassObject = ClassObjectManager.GetClassObjectAddr("char");
            int byteClassObject = ClassObjectManager.GetClassObjectAddr("byte");
            int booleanClassObject = ClassObjectManager.GetClassObjectAddr("boolean");

            if (itemTypeClassObjAddr == doubleClassObject || itemTypeClassObjAddr == longClassObject)
            {
                return 8;
            }
            else if (itemTypeClassObjAddr == shortClassObject || itemTypeClassObjAddr == charClassObject)
            {
                return 2;
            }
            else if (itemTypeClassObjAddr == byteClassObject || itemTypeClassObjAddr == booleanClassObject)
            {
                return 1;
            }
            else return 4;
        }
    }
}
