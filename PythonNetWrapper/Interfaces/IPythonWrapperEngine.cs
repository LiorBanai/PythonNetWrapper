using System;
using System.Collections.Generic;
using Autofac;
using Python.Runtime;

namespace PythonNetWrapper.Interfaces
{
    public interface IPythonWrapperEngine : IDisposable
    {
        // executes a Python command
        T ExecuteCommand<T>(string command, bool throwOnErrors, out string log);
        PyObject ImportScript(string fileName, bool throwOnErrors, out string log);
        T ExecuteMethod<T>(string fileName,string methodName, bool throwOnErrors, out string log, params PyObject[] args);
        T ExecuteMethodOnScriptObject<T>(PyObject script, string methodName, bool throwOnErrors, out string log, params PyObject[] args);
        // sets an object in Python's scope
        void SetVariable(string name, object value);
        void SetupLogger(bool throwOnErrors);

        // Python's search path
        IList<string> SearchPaths();
        string PythonPaths();

        // Sets search paths
        void SetSearchPath(IList<string> paths);

        // initializes this engine with the app container
        void Initialize(IContainer appContainer);
    }
}
