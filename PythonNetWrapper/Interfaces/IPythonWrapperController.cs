using Python.Runtime;

namespace PythonNetWrapper.Interfaces
{
    public interface IPythonWrapperController
    {
        void Initialize();
        void ShutDown();
        PyObject? ImportScript(string fileName, out string log);
        PyObject? RunScript(string script, out string log);
        PyObject? ExecuteMethod(string fileName, string methodName, out string log, params PyObject[] args);
        PyObject? ExecuteMethodOnScriptObject(PyObject script, string methodName, out string log, params PyObject[] args);
    }
}
