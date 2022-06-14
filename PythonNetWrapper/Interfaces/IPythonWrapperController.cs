using System;
using System.Collections.Generic;
using Python.Runtime;

namespace PythonNetWrapper.Interfaces
{
    public interface IPythonWrapperController
    {
        void Initialize();
        void ShutDown();
        /// <summary>
        /// Add search paths (each one needs to ends with /)
        /// </summary>
        /// <param name="paths">list of paths</param>
        /// <returns>The os and sys paths after adding</returns>
        string AddSearchPaths(List<string> paths);
        /// <summary>
        /// the os and sys paths
        /// </summary>
        /// <returns></returns>
        string PythonPaths();
        PyObject ImportScript(string fileName, out string log);
        T RunScript<T>(string script, out string log);
        T ExecuteMethod<T>(string fileName, string methodName, out string log, params PyObject[] args);
        T ExecuteMethodOnScriptObject<T>(PyObject script, string methodName, out string log, params PyObject[] args);
    }
}
