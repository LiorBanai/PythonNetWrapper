using System;
using Python.Runtime;

namespace PythonNetWrapper.Interfaces
{
    public interface IPythonWrapperController
    {
        void Initialize();
        void ShutDown();
        T ImportScript<T>(string fileName, out string log);
        T RunScript<T>(string script, out string log);
        T ExecuteMethod<T>(string fileName, string methodName, out string log, params PyObject[] args);
        T ExecuteMethodOnScriptObject<T>(PyObject script, string methodName, out string log, params PyObject[] args);
    }
}
