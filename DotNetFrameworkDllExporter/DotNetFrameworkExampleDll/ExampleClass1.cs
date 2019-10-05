namespace DotNetFrameworkExampleDll
{
    using System;
    using System.Collections.Generic;

    public class ExampleClass1
    {
        public static char ExampleStaticVariable;

        public long ExampleInstanceVariable;

        public InnerGenericTypes.DoubleInnerClass DoubleInner;

        public InnerGenericTypes.OuterClass<string> CustomGenericField;

        public InnerGenericTypes.OuterClass<string>.InnerNonGeneric CustomInnerGenericField;

        public InnerGenericTypes.OuterClass<string>.InnerGenericClass<int, string, char> CustomInnerDoubleGenericField;

        protected int protectedVar;
        
        public int ExampleAutoProperty { get; set; }

        public int OnlyGet { get => throw new NotImplementedException(); }

        public int OnlySet { set => throw new NotImplementedException(); }

        public int ReadOnlyProperty { get; }

        public byte ExampleFullProperty { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public static void ExampleStaticVoidMethod()
        {
            throw new NotImplementedException();
        }

        public void ExampleVoidMethod()
        {
            throw new NotImplementedException();
        }

        public void ExampleParametrizedMethod()
        {
            throw new NotImplementedException();
        }

        public class InnerClass2
        {
            public int Number { get; set; }
        }
    }

    public class Text2<TPar>
    {
        public InnerGenericTypes.OuterClass<string>.InnerGenericClass<int, TPar, char> Partial;

        public void TestRefMethod(ref int a)
        {
            a = 99;
        }
        
        public void TestOutMethod(out int a)
        {
            a = 100;
        }

        public void TestOutOwnMethod(out ExampleClass1 a)
        {
            a = new ExampleClass1();
        }

        public void TestOutListMethod(out List<TPar> a)
        {
            a = new List<TPar>();
        }
    }
}
