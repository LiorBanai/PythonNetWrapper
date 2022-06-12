using System;
using System.Collections.Generic;
using System.IO;
using Python.Runtime;
using PythonNetWrapper.Interfaces;

namespace PythonNetWrapper
{
    public class PythonWrapperController : IPythonWrapperController
    {
        private readonly IPythonWrapperEngine _pythonWrapperEngine;
        private string pathToVirtualEnv;
        private string pythonExecutableFolder;
        private string pythonDll = "python37.dll";
        private IntPtr pythonThreads;
        private bool enableLogging;

        public PythonWrapperController(IPythonWrapperEngine pythonWrapperEngine, string pathToVirtualEnv, string pythonExecutableFolder, string pythonDll = "python37.dll", bool enableLogging = true)
        {
            _pythonWrapperEngine = pythonWrapperEngine;
            this.pathToVirtualEnv = pathToVirtualEnv;
            this.pythonExecutableFolder = pythonExecutableFolder;
            if (!string.IsNullOrEmpty(pythonDll))
            {
                this.pythonDll = pythonDll;
            }
            this.enableLogging = enableLogging;
        }

        public PythonWrapperController(string pathToVirtualEnv, string pythonExecutableFolder, string pythonDll = "python37.dll", bool enableLogging = true) : this(new PythonWrapperNet(), pathToVirtualEnv, pythonExecutableFolder, pythonDll, enableLogging)
        {
        }
        public void Initialize()
        {
            var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            string pathEnv = string.Empty;
            var searchPAth = new List<string>();
            if (!string.IsNullOrEmpty(pathToVirtualEnv) && Directory.Exists(pathToVirtualEnv))
            {
                string pathToLib = Path.Combine(pathToVirtualEnv, "lib");
                string pathToPackages = Path.Combine(pathToLib, "site-packages");
                string env = $"{pathToPackages};{pathToLib}";
                Environment.SetEnvironmentVariable("PYTHONPATH", env, EnvironmentVariableTarget.Process);
                pathEnv = path = $"{path};{pathToPackages};{pathToLib}";
                searchPAth.Add(pathToLib);
                searchPAth.Add(pathToPackages);
            }

            if (!string.IsNullOrEmpty(pythonExecutableFolder) && Directory.Exists(pythonExecutableFolder))
            {
                pathEnv = $"{path};{pythonExecutableFolder}";
                string pythonPath = Path.Combine(pythonExecutableFolder, pythonDll);
                if (File.Exists(pythonPath))
                    Runtime.PythonDLL = pythonPath;

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
            pythonThreads = PythonEngine.BeginAllowThreads();
            _pythonWrapperEngine.SetSearchPath(searchPAth);
            var paths = _pythonWrapperEngine.PythonPaths();
            if (enableLogging)
            {
                _pythonWrapperEngine.SetupLogger();
            }
        }

        public PyObject? RunScript(string script, out string log)
        {
            return _pythonWrapperEngine.ExecuteCommand(script, out log);
        }

        public PyObject? ImportScript(string fileName, out string log)
        {
            return _pythonWrapperEngine.ImportScript(fileName, out log);
        }

        public PyObject? ExecuteMethod(string fileName, string methodName, out string log, params PyObject[] args)
        {
            return _pythonWrapperEngine.ExecuteMethod(fileName, methodName, out log, args);
        }

        public PyObject? ExecuteMethodOnScriptObject(PyObject script, string methodName, out string log,
            params PyObject[] args)
        {
            return _pythonWrapperEngine.ExecuteMethodOnScriptObject(script, methodName, out log, args);

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
