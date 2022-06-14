using System;
using System.Collections.Generic;
using Python.Runtime;

namespace PythonNetWrapper.Interfaces
{
    public interface IPythonWrapperController
    {
        /// <summary>
        /// Initialize the Python Engine (must called first)
        /// </summary>
        void Initialize();
        /// <summary>
        /// Shutdown the Python Engine
        /// </summary>
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
        /// <summary>
        /// Call Py.Import on the GetFileNameWithoutExtension of the file.
        /// </summary>
        /// <param name="fileName">The file name of the py file to import</param> <param name="log">Output log print method (if Logger is setup)</param>
        /// <returns></returns>
        PyObject ImportScript(string fileName, out string log);
        /// <summary>
        /// Executes a Python command text or script 
        /// </summary>
        /// <typeparam name="T">the return type of the result (Or PyObject if no result are returned)</typeparam>
        /// <param name="textCommandOrScript">The text to execute</param>
        /// <param name="log">Output log print method (if Logger is setup)</param>
        /// <returns>The result (or PyObject that is PyObject.None)</returns>
        T ExecuteCommandOrScript<T>(string textCommandOrScript, out string log);
        /// <summary>
        /// Import the file name and call the specific method
        /// </summary>
        /// <typeparam name="T">The return type of the result (Or PyObject, that is PyObject.None, if no result are returned)</typeparam>
        /// <param name="fileName"></param>
        /// <param name="methodName"></param>
        /// <param name="log">Output log print method (if Logger is setup)</param>
        /// <param name="args">The arguments to the method</param>
        /// <returns>The result of the method (or PyObject that is PyObject.None if no result is returned)</returns>
        T ExecuteMethod<T>(string fileName, string methodName, out string log, params PyObject[] args);
        /// <summary>
        /// Execute a method on an imported script Object
        /// </summary>
        /// <typeparam name="T">The return type of the result (Or PyObject, that is PyObject.None, if no result are returned)</typeparam>
        /// <param name="script">the imported script Object</param>
        /// <param name="methodName">The method to call on the script object</param>
        /// <param name="log">Output log print method (if Logger is setup)</param>
        /// <param name="args">The arguments to the method</param>
        /// <returns></returns>
        T ExecuteMethodOnScriptObject<T>(PyObject script, string methodName, out string log, params PyObject[] args);
    }
}
