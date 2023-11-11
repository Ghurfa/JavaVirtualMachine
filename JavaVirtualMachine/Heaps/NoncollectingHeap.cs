using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace JavaVirtualMachine.Heaps
{
    internal class NoncollectingHeap : IHeap
    {
        private Memory<byte> Memory;
        private int itemIndex = 16;

        public int ObjectFieldOffset => 8;
        public int ObjectFieldSize => 8;
        public int ArrayMarkOffset => 4;
        public int ArrayItemTypeOffset => 8;
        public int ArrayLengthOffset => 12;
        public int ArrayBaseOffset => 16;

        public NoncollectingHeap(int startSize = 4096)
        {
            Memory = new Memory<byte>(new byte[startSize]);
        }

        public int CreateObject(int classFileIdx)
        {
            ClassFile cFile = ClassFileManager.ClassFiles[classFileIdx];
            int numBytes = ObjectFieldOffset + ObjectFieldSize * cFile.InstanceFields.Count;
            int addr = AllocateMemory(numBytes);
            BinaryPrimitives.WriteInt32LittleEndian(Memory.Slice(addr).Span, classFileIdx);
            return addr;
        }

        public int CreateArray(int[] arr, int itemTypeClassObjAddr)
        {
            int addr = CreateArray(4, arr.Length, itemTypeClassObjAddr);

            Span<int> arrData = MemoryMarshal.Cast<byte, int>(Memory.Slice(addr + ArrayBaseOffset).Span); // Store data
            arr.CopyTo(arrData);
            return addr;
        }

        public int CreateArray(int itemSize, int itemCount, int itemTypeClassObjAddr)
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

        public int CloneObject(int address)
        {
            int cFileIdx = BinaryPrimitives.ReadInt32LittleEndian(Memory.Slice(address).Span);
            bool isArray = BinaryPrimitives.ReadInt32LittleEndian(Memory.Slice(address + ArrayMarkOffset).Span) != 0;

            int numBytes;
            if (isArray)
            {
                HeapArray array = new(address, this);
                numBytes = ArrayBaseOffset + array.ItemSize * array.Length;
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

        public HeapObject GetObject(int address)
        {
            if (address == 0) return null;

            bool isArray = BinaryPrimitives.ReadInt32LittleEndian(Memory.Slice(address + ArrayMarkOffset).Span) != 0;

            if (isArray)
            {
                return new HeapArray(address, this);
            }

            return new HeapObject(address, this);
        }

        public HeapArray GetArray(int address)
        {
            if (address == 0) return null;

            return new HeapArray(address, this);
        }

        public byte GetByte(int address) => Memory.Slice(address).Span[0];
        public short GetShort(int address) => BinaryPrimitives.ReadInt16LittleEndian(Memory.Slice(address).Span);
        public int GetInt(int address) => BinaryPrimitives.ReadInt32LittleEndian(Memory.Slice(address).Span);
        public long GetLong(int address) => BinaryPrimitives.ReadInt64LittleEndian(Memory.Slice(address).Span);
        public Span<byte> GetSpan(int address, int length) => Memory.Slice(address, length).Span;

        public void PutByte(int address, byte value) => Memory.Slice(address, 1).Span[0] = value;
        public void PutShort(int address, short value) => BinaryPrimitives.WriteInt16LittleEndian(Memory.Slice(address).Span, value);
        public void PutInt(int address, int value) => BinaryPrimitives.WriteInt32LittleEndian(Memory.Slice(address).Span, value);
        public void PutLong(int address, long value) => BinaryPrimitives.WriteInt64LittleEndian(Memory.Slice(address).Span, value);

        public int AllocateMemory(long size)
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

        private void ExpandMemory()
        {
            Memory<byte> newMemory = new(new byte[Memory.Length * 2]);
            Memory.CopyTo(newMemory);
            Memory = newMemory;
        }
    }
}
