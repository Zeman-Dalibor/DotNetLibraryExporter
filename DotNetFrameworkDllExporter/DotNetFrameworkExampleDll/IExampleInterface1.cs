namespace DotNetFrameworkExampleDll
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal interface IExampleInterface1
    {
        List<string> ExampleSystemUsage { get; set; }

        Task<string> GetExampleTask();

        new int A { get; set; }
    }
}
