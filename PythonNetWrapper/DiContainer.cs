using System;
using System.Linq;
using Autofac;

namespace PythonNetWrapper
{
    class DiContainer
    {
        private readonly IContainer _container;

        public DiContainer(IContainer container)
        {
            this._container = container;
        }
        public object Resolve(string typeFullName)
        {
            var type = GetType(typeFullName);
            return _container.Resolve(type);
        }

        private Type GetType(string fullName)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).ToList();
            var res= types.FirstOrDefault(s => string.Equals(s.FullName, fullName, StringComparison.Ordinal));
            return res;
        }
    }
}
