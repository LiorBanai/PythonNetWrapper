using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Python.Runtime;
using Python.Runtime.Codecs;
using PythonNetWrapper.Interfaces;

namespace PythonNetWrapper.Tests
{
    [TestClass]
    public class UnitTests
    {
        public static IContainer Container { get; set; }


        [ClassInitialize]
        public static void TestInitialize(TestContext context)
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
            Container.Resolve<IPythonWrapperEngine>().Initialize(Container,true,out _);
            PythonEngine.Initialize();

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
        public void TestMethodReturnlistOfInt()
        {
            var controller = Container.Resolve<IPythonWrapperController>();
            var filename = Path.Combine(Directory.GetCurrentDirectory(), @"pythonScripts\testpythonnet.py");
            var res = controller.ExecuteMethod<Int32[]>(filename, "returnintlist", out _);
            Assert.AreEqual(PyObject.None, res);
        }
        [TestMethod]
        public void TestMethodReturnlistAsIs()
        {
            var controller = Container.Resolve<IPythonWrapperController>();
            var filename = Path.Combine(Directory.GetCurrentDirectory(), @"pythonScripts\testpythonnet.py");
            var items = new List<PyObject>() { new PyInt(1), new PyInt(2), new PyInt(3) };
            ListDecoder codec=new ListDecoder();
            using var pyList = new PyList(items.ToArray());
            using var pyListType = pyList.GetPythonType();
            Assert.IsTrue(codec.CanDecode(pyListType, typeof(IList<bool>)));
            Assert.IsTrue(codec.CanDecode(pyListType, typeof(IList<int>)));
            Assert.IsFalse(codec.CanDecode(pyListType, typeof(IEnumerable)));
            Assert.IsFalse(codec.CanDecode(pyListType, typeof(IEnumerable<int>)));
            Assert.IsFalse(codec.CanDecode(pyListType, typeof(ICollection<float>)));
            Assert.IsFalse(codec.CanDecode(pyListType, typeof(bool)));
            Assert.IsFalse(codec.CanDecode(pyListType, typeof(List<int>)));

            //convert to list of int
            IList<int> intList = null;
             codec.TryDecode(pyList, out intList);
            var res = controller.ExecuteMethod<IList<int>>(filename, "returnPyListAsIs", out _, pyList);
            Assert.AreEqual(PyObject.None, res);
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

        [TestMethod]
        public void TestImportScript()
        {
            var controller = Container.Resolve<IPythonWrapperController>();
            var filename = Path.Combine(Directory.GetCurrentDirectory(), @"pythonScripts\testpythonnet.py");
            var script = controller.ImportScript(filename, out _);
            var noreturn = controller.ExecuteMethodOnScriptObject<PyObject>(script, "noreturnvalue", out _);
            Assert.AreEqual(PyObject.None, noreturn);
            PyList types = new PyList();
            types.Append(new PyInt(1));
            types.Append(new PyInt(2));
            PyList timestamps = new PyList();
            timestamps.Append(new PyInt(100));
            timestamps.Append(new PyInt(200));
            var intReturn = controller.ExecuteMethodOnScriptObject<Int32>(script, "return3int", out _, timestamps, types);
            Assert.AreEqual(3, intReturn);
            var boolReturn = controller.ExecuteMethodOnScriptObject<bool>(script, "returntruebool", out _);
            Assert.AreEqual(true, boolReturn);
        }
    }
}
