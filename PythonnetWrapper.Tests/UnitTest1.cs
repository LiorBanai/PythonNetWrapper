using System.IO;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Python.Runtime;
using PythonnetWrapper.Interfaces;

namespace PythonnetWrapper.Tests
{
    [TestClass]
    public class UnitTest1
    {
        public static IContainer Container { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<PythonNet>().As<IPythonEngine>().InstancePerLifetimeScope();
            builder.RegisterType<PythonEngineController>().As<IPythonEngineController>().
                WithParameters(new[]
                {
                    new NamedParameter("pathToVirtualEnv", @"C:\Users\liorb\PycharmProjects\pythonProject\venv"),
                    new NamedParameter("pythonExecutableFolder",@"C:\Users\liorb\AppData\Local\Programs\Python\Python37"),
                    new NamedParameter("pythonDll","python37.dll"),
                    new NamedParameter("enableLogging",true)
                })
                .InstancePerLifetimeScope();
            Container = builder.Build();
            Container.Resolve<IPythonEngineController>().Initialize();
            Container.Resolve<IPythonEngine>().Initialize(Container);
        }
        [TestMethod]
        public void TestMethod1()
        {
            var controller = Container.Resolve<IPythonEngineController>();
            var filename = Path.Combine(Directory.GetCurrentDirectory(), @"pythonScripts\testpythonnet.py");
            PyList types = new PyList();
            types.Append(new PyInt(1));
            types.Append(new PyInt(2));
            PyList timestamps = new PyList();
            timestamps.Append(new PyInt(100));
            timestamps.Append(new PyInt(200));

            var res = controller.ExecuteMethod(filename, "testvalue", out _, timestamps, types);

        }

        
    }
}
