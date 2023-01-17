using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

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
            classFileIdx.AsByteArray().CopyTo(Memory.Slice(addr));
            return addr;
        }

        public static int CloneObject(int address)
        {
            int cFileIdx = Memory.Slice(address, 4).ToArray().ToInt();
            bool isArray = Memory.Slice(address + ArrayMarkOffset, 4).ToArray().ToInt() != 0;

            int numBytes;
            if (isArray)
            {
                int itemTypeClassObjAddr = Memory.Slice(8, 4).ToArray().ToInt();
                int doubleClassObject = ClassObjectManager.GetClassObjectAddr("double");
                int longClassObject = ClassObjectManager.GetClassObjectAddr("long");
                int itemSize = itemTypeClassObjAddr == doubleClassObject || itemTypeClassObjAddr == longClassObject ? 8 : 4;

                int itemCount = Memory.Slice(ArrayLengthOffset, 4).ToArray().ToInt();

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
            objectCFileIdx.AsByteArray().CopyTo(Memory.Slice(addr)); // Store type
            Memory.Span[addr + ArrayMarkOffset] = 1; // Mark as array
            itemTypeClassObjAddr.AsByteArray().CopyTo(Memory.Slice(addr + ArrayItemTypeOffset)); // Store item type
            itemCount.AsByteArray().CopyTo(Memory.Slice(addr + ArrayLengthOffset)); // Store length

            return addr;
        }

        public static HeapObject GetObject(int address)
        {
            if (address == 0) return null;

            bool isArray = Memory.Slice(address + ArrayMarkOffset, 4).ToArray().ToInt() != 0;

            if (isArray)
            {
                return GetArray(address);
            }

            return new HeapObject(address);
        }

        public static HeapArray GetArray(int address)
        {
            if (address == 0) return null;

            int itemTypeClassObjAddr = Memory.Slice(address + ArrayItemTypeOffset, 4).ToArray().ToInt();
            int doubleClassObject = ClassObjectManager.GetClassObjectAddr("double");
            int longClassObject = ClassObjectManager.GetClassObjectAddr("long");
            int shortClassObject = ClassObjectManager.GetClassObjectAddr("short");
            int charClassObject = ClassObjectManager.GetClassObjectAddr("char");
            int byteClassObject = ClassObjectManager.GetClassObjectAddr("byte");
            int booleanClassObject = ClassObjectManager.GetClassObjectAddr("boolean");

            int itemSize;
            if (itemTypeClassObjAddr == doubleClassObject || itemTypeClassObjAddr == longClassObject)
            {
                itemSize = 8;
            }
            else if (itemTypeClassObjAddr == shortClassObject || itemTypeClassObjAddr == charClassObject)
            {
                itemSize = 2;
            }
            else if (itemTypeClassObjAddr == byteClassObject || itemTypeClassObjAddr == booleanClassObject)
            {
                itemSize = 1;
            }
            else itemSize = 4;

            return new HeapArray(address, itemSize);
        }

        public static byte GetByte(int address) => Memory.Slice(address, 1).ToArray()[0];
        public static short GetShort(int address) => Memory.Slice(address, 2).ToArray().ToShort();
        public static int GetInt(int address) => Memory.Slice(address, 4).ToArray().ToInt();
        public static long GetLong(int address) => Memory.Slice(address, 8).ToArray().ToLong();
        public static Span<byte> GetSpan(int address, int length) => Memory.Slice(address, length).Span;
        //public static Memory<byte> GetMemorySlice(int startAddress, int size) => Memory.Slice(startAddress, size);

        public static void PutByte(int address, byte value) => Memory.Slice(address, 1).Span[0] = value;
        public static void PutShort(int address, short value) => value.AsByteArray().CopyTo(Memory.Slice(address, 2));
        public static void PutInt(int address, int value) => value.AsByteArray().CopyTo(Memory.Slice(address, 4));
        public static void PutLong(int address, long value) => value.AsByteArray().CopyTo(Memory.Slice(address, 8));
        public static void PutData(int address, byte[] data) => data.CopyTo(Memory.Slice(address, data.Length));

        public static void Fill(long address, long numOfBytes, byte value)
        {
            Memory.Slice((int)address, (int)numOfBytes).Span.Fill(value);
        }

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
            byte[] newMemory = new byte[Memory.Length * 2];
            Memory.ToArray().CopyTo((Memory<byte>)newMemory);
            Memory = new Memory<byte>(newMemory);
        }
    }
}
