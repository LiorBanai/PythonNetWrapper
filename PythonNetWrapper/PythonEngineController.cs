﻿using System;
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
        private IntPtr pythonThreads;
        private bool enableLogging;
        public PythonEngineController(IPythonEngine pythonEngine, string pathToVirtualEnv, string pythonExecutableFolder, bool enableLogging)
        {
            _pythonEngine = pythonEngine;
            this.pathToVirtualEnv = pathToVirtualEnv;
            this.pythonExecutableFolder = pythonExecutableFolder;
            this.enableLogging = enableLogging;
        }
        public void Initialize()
        {
            var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process); 
            // PythonEngine.PythonPath = path;
            string pathEnv = string.Empty;
            var searchPAth = new List<string>();
            if (!string.IsNullOrEmpty(pathToVirtualEnv))
            {
                string pathToLib = Path.Combine(pathToVirtualEnv, "lib");
                string pathToPackages = Path.Combine(pathToLib, "site-packages");
                string env = $"{pathToPackages};{pathToLib}";
                Environment.SetEnvironmentVariable("PYTHONPATH", env, EnvironmentVariableTarget.Process);
                pathEnv = path = $"{path};{pathToPackages};{pathToLib}";
                //Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
                searchPAth.Add(pathToLib);
                searchPAth.Add(pathToPackages);
                searchPAth.Add(@"D:\git\kcm_algo");
            }

            if (!string.IsNullOrEmpty(pythonExecutableFolder))
            {
                pathEnv = $"{path};{pythonExecutableFolder}";
            }

            if (!string.IsNullOrEmpty(pathEnv))
            {
                Environment.SetEnvironmentVariable("PATH", pathEnv, EnvironmentVariableTarget.Process);
            }

            //Environment.SetEnvironmentVariable("PATH", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            //Environment.SetEnvironmentVariable("PYTHONHOME", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            //Environment.SetEnvironmentVariable("PYTHONPATH", $"{pathToVirtualEnv}\\Lib\\site-packages;{pathToVirtualEnv}\\Lib", EnvironmentVariableTarget.Process);


            // Python.Runtime.PythonEngine.PythonHome = pathToVirtualEnv;
            //Python.Runtime.PythonEngine.PythonPath = ppath;
            //Python.Runtime.PythonEngine.Initialize();
            //pythonThreads = Python.Runtime.PythonEngine.BeginAllowThreads();
            var paths = _pythonEngine.PythonPaths();
            //Python.Runtime.PythonEngine.PythonHome = pathToVirtualEnv;
            _pythonEngine.SetSearchPath(searchPAth);
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
           return _pythonEngine.ExecuteMethod(fileName, methodName,out log,args);
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
                Python.Runtime.PythonEngine.EndAllowThreads(pythonThreads);
            }
            Python.Runtime.PythonEngine.Shutdown();
        }
    }
}
