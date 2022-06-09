using System;
using System.Collections.Generic;
using Autofac;
using Python.Runtime;

namespace PythonnetWrapper.Interfaces
{
    public interface IPythonEngine : IDisposable
    {
        // executes a Python command
        PyObject ExecuteCommand(string command, out string log);
        PyObject ImportScript(string fileName, out string log);
        PyObject ExecuteMethod(string fileName,string methodName, out string log, params PyObject[] args);
        PyObject ExecuteMethodOnScriptObject(PyObject script, string methodName, out string log, params PyObject[] args);
        // sets an object in Python's scope
        void SetVariable(string name, object value);
        void SetupLogger();

        // Python's search path
        IList<string> SearchPaths();
        string PythonPaths();

        // Sets search paths
        void SetSearchPath(IList<string> paths);

        // initializes this engine with the app container
        void Initialize(IContainer appContainer);
    }
}
