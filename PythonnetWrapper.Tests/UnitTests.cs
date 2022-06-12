using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Python.Runtime;
using PythonNetWrapper.Interfaces;

namespace PythonNetWrapper.Tests
{
    [TestClass]
    public class UnitTests
    {
        public static IContainer Container { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            string pythonLocation = Environment.GetEnvironmentVariable("pythonLocation");
            if (string.IsNullOrEmpty(pythonLocation))
            {
                pythonLocation = @"C:\Users\liorb\AppData\Local\Programs\Python\Python37";
            }
            var builder = new ContainerBuilder();
            builder.RegisterType<PythonWrapperNet>().As<IPythonWrapperEngine>().InstancePerLifetimeScope();
            builder.RegisterType<PythonWrapperController>().As<IPythonWrapperController>().
                WithParameters(new[]
                {
                    new NamedParameter("pathToVirtualEnv", @""),
                    new NamedParameter("pythonExecutableFolder",pythonLocation),
                    new NamedParameter("pythonDll","python37.dll"),
                    new NamedParameter("enableLogging",true)
                })
                .InstancePerLifetimeScope();
            Container = builder.Build();
            Container.Resolve<IPythonWrapperController>().Initialize();
            Container.Resolve<IPythonWrapperEngine>().Initialize(Container);
        }
        [TestMethod]
        public void TestMethodReturnInteger()
        {
            var controller = Container.Resolve<IPythonWrapperController>();
            var filename = Path.Combine(Directory.GetCurrentDirectory(), @"pythonScripts\testpythonnet.py");
            PyList types = new PyList();
            types.Append(new PyInt(1));
            types.Append(new PyInt(2));
            PyList timestamps = new PyList();
            timestamps.Append(new PyInt(100));
            timestamps.Append(new PyInt(200));

            var res = controller.ExecuteMethod<Int32>(filename, "return3int", out _, timestamps, types);
            Assert.AreEqual(3, res);
        }
        [TestMethod]
        public void TestMethodReturnBool()
        {
            var controller = Container.Resolve<IPythonWrapperController>();
            var filename = Path.Combine(Directory.GetCurrentDirectory(), @"pythonScripts\testpythonnet.py");
            PyList types = new PyList();
            types.Append(new PyInt(1));
            types.Append(new PyInt(2));
            PyList timestamps = new PyList();
            timestamps.Append(new PyInt(100));
            timestamps.Append(new PyInt(200));

            var res = controller.ExecuteMethod<bool>(filename, "returntruebool", out _);
            Assert.AreEqual(true, res);
        }

        [TestMethod]
        public void TestMethodNoReturnValue()
        {
            var controller = Container.Resolve<IPythonWrapperController>();
            var filename = Path.Combine(Directory.GetCurrentDirectory(), @"pythonScripts\testpythonnet.py");
            PyList types = new PyList();
            types.Append(new PyInt(1));
            types.Append(new PyInt(2));
            PyList timestamps = new PyList();
            timestamps.Append(new PyInt(100));
            timestamps.Append(new PyInt(200));

            var res = controller.ExecuteMethod<PyObject>(filename, "noreturnvalue", out _);
            Assert.AreEqual(PyObject.None, res);
        }

    }
}
