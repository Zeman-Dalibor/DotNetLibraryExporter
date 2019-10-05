namespace DotNetFrameworkExampleDll
{
    public class InnerGenericTypes
    {
        public class DoubleInnerClass
        {
        }

        public class OuterClass<TValue>
        {
            public class InnerGenericClass<TKey, T1, T2>
            {
                // None
            }

            public class InnerNonGeneric
            {
                // None
            }
        }
    }
}
