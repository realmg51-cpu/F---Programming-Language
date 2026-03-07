using System;
using System.Collections.Generic;

namespace Fminusminus.Utils.Package
{
    /// <summary>
    /// Base class for all F-- packages
    /// </summary>
    public abstract class BasePackage
    {
        /// <summary>
        /// Package name (used in import statement)
        /// </summary>
        public abstract string Name { get; }
        
        /// <summary>
        /// Package version
        /// </summary>
        public virtual string Version => "1.0.0";
        
        /// <summary>
        /// Package description
        /// </summary>
        public virtual string Description => "";
        
        /// <summary>
        /// Methods available in this package
        /// </summary>
        protected Dictionary<string, Func<object?[], object?>> _methods = new();
        
        /// <summary>
        /// Initialize the package
        /// </summary>
        public virtual void Initialize()
        {
            // Override in derived classes
        }
        
        /// <summary>
        /// Check if method exists
        /// </summary>
        public bool HasMethod(string name) => _methods.ContainsKey(name);
        
        /// <summary>
        /// Call a method in this package
        /// </summary>
        public object? CallMethod(string name, object?[] args)
        {
            if (_methods.TryGetValue(name, out var method))
            {
                return method(args);
            }
            throw new Exception($"Method '{name}' not found in package '{Name}'");
        }
        
        /// <summary>
        /// Get all method names
        /// </summary>
        public IEnumerable<string> GetMethodNames() => _methods.Keys;
    }
}
