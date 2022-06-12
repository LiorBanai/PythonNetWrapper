using System;
using System.Collections.Generic;
using System.IO;
using Python.Runtime;
using PythonnetWrapper.Interfaces;

namespace PythonnetWrapper
{
    public class PythonEngineController : IPythonEngineController
    {
        private readonly IPythonEngine _pythonEngine;
        private string pathToVirtualEnv;
        private string pythonExecutableFolder;
        private string pythonDll;
        private IntPtr pythonThreads;
        private bool enableLogging;
        
        public PythonEngineController(IPythonEngine pythonEngine, string pathToVirtualEnv, string pythonExecutableFolder, string pythonDll = "python37.dll", bool enableLogging = true)
        {
            _pythonEngine = pythonEngine;
            this.pathToVirtualEnv = pathToVirtualEnv;
            this.pythonExecutableFolder = pythonExecutableFolder;
            if (!string.IsNullOrEmpty(pythonDll))
            {
                this.pythonDll = pythonDll;
            }
            this.enableLogging = enableLogging;
        }

        public PythonEngineController(string pathToVirtualEnv, string pythonExecutableFolder, string pythonDll = "python37.dll", bool enableLogging = true) : this(new PythonNet(), pathToVirtualEnv, pythonExecutableFolder, pythonDll, enableLogging)
        {
        }
        public void Initialize()
        {
            var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            string pathEnv = string.Empty;
            var searchPAth = new List<string>();
            if (!string.IsNullOrEmpty(pathToVirtualEnv))
            {
                string pathToLib = Path.Combine(pathToVirtualEnv, "lib");
                string pathToPackages = Path.Combine(pathToLib, "site-packages");
                string env = $"{pathToPackages};{pathToLib}";
                Environment.SetEnvironmentVariable("PYTHONPATH", env, EnvironmentVariableTarget.Process);
                pathEnv = path = $"{path};{pathToPackages};{pathToLib}";
                searchPAth.Add(pathToLib);
                searchPAth.Add(pathToPackages);
            }

            if (!string.IsNullOrEmpty(pythonExecutableFolder))
            {
                pathEnv = $"{path};{pythonExecutableFolder}";
                Runtime.PythonDLL = Path.Combine(pythonExecutableFolder, pythonDll);

            }

            if (!string.IsNullOrEmpty(pathEnv))
            {
                Environment.SetEnvironmentVariable("PATH", pathEnv, EnvironmentVariableTarget.Process);
            }

            //Environment.SetEnvironmentVariable("PATH", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            //Environment.SetEnvironmentVariable("PYTHONHOME", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            //Environment.SetEnvironmentVariable("PYTHONPATH", $"{pathToVirtualEnv}\\Lib\\site-packages;{pathToVirtualEnv}\\Lib", EnvironmentVariableTarget.Process);

            PythonEngine.PythonPath = PythonEngine.PythonPath + Path.PathSeparator +
                                                     Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);
            PythonEngine.PythonHome = pathToVirtualEnv;
            PythonEngine.Initialize();
            _pythonEngine.SetSearchPath(searchPAth);
            var paths = _pythonEngine.PythonPaths();
            if (enableLogging)
            {
                _pythonEngine.SetupLogger();
            }
        }

        public PyObject RunScript(string script, out string log)
        {
            return _pythonEngine.ExecuteCommand(script, out log);
        }

        public PyObject ImportScript(string fileName, out string log)
        {
            return _pythonEngine.ImportScript(fileName, out log);
        }

        public PyObject ExecuteMethod(string fileName, string methodName, out string log, params PyObject[] args)
        {
            return _pythonEngine.ExecuteMethod(fileName, methodName, out log, args);
        }

        public PyObject ExecuteMethodOnScriptObject(PyObject script, string methodName, out string log,
            params PyObject[] args)
        {
            return _pythonEngine.ExecuteMethodOnScriptObject(script, methodName, out log, args);

        }

        public void ShutDown()
        {
            if (pythonThreads != IntPtr.Zero)
            {
                PythonEngine.EndAllowThreads(pythonThreads);
            }
            PythonEngine.Shutdown();
        }
    }
}
