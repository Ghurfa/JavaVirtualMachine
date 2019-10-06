using System;
using Xunit;
using JavaVirtualMachine;
using JavaVirtualMachine.ConstantPoolInfo;

namespace JVMUnitTests
{
    public class UnitTests
    {
        /*[Theory]
        [InlineData((sbyte)0)]
        [InlineData((sbyte)-1)]
        [InlineData(SByte.MaxValue)]
        [InlineData(SByte.MinValue)]
        public void bipushTest(sbyte value)
        {
            byte[] code = { 0x00, 0x01,                 //Max Stack
                            0x00, 0x00,                 //Max Locals
                            0x00, 0x00, 0x00, 0x03,     //Code length
                            0x10, (byte)value, 0xAC};   //Code
            MethodFrame methodFrame = new MethodFrame(new ReadOnlyMemory<byte>(code), new ClassObject(@"C:\Users\VisitorDL11\Documents\LorenzoLopezComputerArchitecture\GradleProject\build\classes\java\main\Program.class"));
            int ret = methodFrame.Execute();
            Assert.Equal(value, ret);
        }
        [Theory]
        [InlineData(0)]
        [InlineData((short)-1)]
        [InlineData(short.MaxValue)]
        [InlineData(short.MinValue)]
        public void sipushTest(short value)
        {
            byte[] code = { 0x00, 0x01,                                             //Max Stack
                            0x00, 0x00,                                             //Max Locals
                            0x00, 0x00, 0x00, 0x04,                                 //Code Length
                            0x11, (byte)(value >> 8), (byte)(value & 0xFF), 0xAC }; //Code
            MethodFrame methodFrame = new MethodFrame(new ReadOnlyMemory<byte>(code), new ClassObject(@"C:\Users\VisitorDL11\Documents\LorenzoLopezComputerArchitecture\GradleProject\build\classes\java\main\Program.class"));
            int ret = methodFrame.Execute();
            Assert.Equal(value, ret);
        }*/
    }
}
