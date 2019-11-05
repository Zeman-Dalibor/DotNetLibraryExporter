namespace DotNetFrameworkExampleDll
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Example Class for testing purpose.
    /// </summary>
    public class ExampleClass1
    {
        /// <summary>
        /// Example variable for testing purpose.
        /// </summary>
        public static char ExampleStaticVariable;

        /// <summary>
        /// Example variable for testing purpose.
        /// </summary>
        public long ExampleInstanceVariable;

        /// <summary>
        /// Example variable for testing purpose.
        /// </summary>
        public InnerGenericTypes.DoubleInnerClass DoubleInner;

        /// <summary>
        /// Example variable for testing purpose.
        /// </summary>
        public InnerGenericTypes.OuterClass<string> CustomGenericField;

        /// <summary>
        /// Example variable for testing purpose.
        /// </summary>
        public InnerGenericTypes.OuterClass<string>.InnerNonGeneric CustomInnerGenericField;

        /// <summary>
        /// Example variable for testing purpose.
        /// </summary>
        public InnerGenericTypes.OuterClass<string>.InnerGenericClass<int, string, char> CustomInnerDoubleGenericField;

        /// <summary>
        /// Example variable for testing purpose.
        /// </summary>
        protected int protectedVar;

        /// <summary>
        /// Gets or sets example property for testing purpose.
        /// </summary>
        public int ExampleAutoProperty { get; set; }

        /// <summary>
        /// Gets example property for testing purpose.
        /// </summary>
        public int OnlyGet
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Sets example property for testing purpose.
        /// </summary>
        public int OnlySet
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        // public int ReadOnlyProperty { get; }
        /*
         * Check for new version.
         * */

        /// <summary>
        /// Gets or sets example property for testing purpose.
        /// </summary>
        public byte ExampleFullProperty
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Example method for testing purpose.
        /// </summary>
        public static void ExampleStaticVoidMethod()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Example method for testing purpose.
        /// </summary>
        public void ExampleVoidMethod()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Example method for testing purpose.
        /// </summary>
        public void ExampleParametrizedMethod()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Example inner class for testing purpose.
        /// </summary>
        public class InnerClass2
        {
            /// <summary>
            /// Example property for testing purpose.
            /// </summary>
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
