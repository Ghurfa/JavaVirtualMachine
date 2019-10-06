using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.Attributes
{
    public class ElementValue
    {
        //https://docs.oracle.com/javase/specs/jvms/se7/html/jvms-4.html#jvms-4.7.16.1
        public byte Tag;

        //If tag is B, C, D, F, I, J, S, Z, or s (primitive or string)
        public ushort ConstValueIndex;
        public CPInfo ConstantValue;

        //If tag is e (enum)
        public ushort TypeNameIndex;
        public ushort ConstNameIndex;
        public string TypeName;
        public string ConstName;

        //If tag is c (class)
        public ushort ClassInfoIndex;
        public CClassInfo ClassInfo;

        //If tag is @ (annotation type)
        public Annotation AnnotationValue;

        //If tag is [ (array)
        public ushort NumValues;
        ElementValue[] Values;

        public ElementValue(ref ReadOnlySpan<byte> data, CPInfo[] Constants)
        {
            Tag = data.ReadOne();
            switch ((char)Tag)
            {
                case 'B':
                case 'C':
                case 'D':
                case 'F':
                case 'I':
                case 'J':
                case 'S':
                case 'Z':
                case 's':
                    ConstValueIndex = data.ReadTwo();
                    ConstantValue = Constants[ConstValueIndex];
                    break;
                case 'e':
                    TypeNameIndex = data.ReadTwo();
                    ConstNameIndex = data.ReadTwo();
                    TypeName = ((CUtf8Info)Constants[TypeNameIndex]).String;
                    ConstName = ((CUtf8Info)Constants[ConstNameIndex]).String;
                    break;
                case 'c':
                    ClassInfoIndex = data.ReadTwo();
                    ClassInfo = (CClassInfo)Constants[ClassInfoIndex];
                    break;
                case '@':
                    AnnotationValue = new Annotation(ref data, Constants);
                    break;
                case '[':
                    NumValues = data.ReadTwo();
                    Values = new ElementValue[NumValues];
                    for(int i = 0; i < NumValues; i++)
                    {
                        Values[i] = new ElementValue(ref data, Constants); 
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public struct Annotation
    {
        public ushort TypeIndex;
        public ushort NumElementValuePairs;
        public (ushort elementNameIndex, ElementValue value)[] ElementValuePairs;
        public Annotation(ref ReadOnlySpan<byte> data, CPInfo[] constants)
        {
            TypeIndex = data.ReadTwo();
            NumElementValuePairs = data.ReadTwo();
            ElementValuePairs = new (ushort, ElementValue)[NumElementValuePairs];
            for (int i = 0; i < NumElementValuePairs; i++)
            {
                ElementValuePairs[i] = (data.ReadTwo(), new ElementValue(ref data, constants));
            }
        }
    }

    public class RuntimeVisibleAnnotationsAttribute : AttributeInfo
    {
        public ushort NumAnnotations;
        public Annotation[] Annotations;

        public RuntimeVisibleAnnotationsAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
            ReadOnlySpan<byte> infoAsSpan = info.AsSpan();
            NumAnnotations = infoAsSpan.ReadTwo();
            Annotations = new Annotation[NumAnnotations];
            for (int i = 0; i < NumAnnotations; i++)
            {
                Annotations[i] = new Annotation(ref infoAsSpan, constants);
            }
        }
    }
}
