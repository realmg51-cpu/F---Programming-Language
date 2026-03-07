using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fminusminus.Utils.Package
{
    /// <summary>
    /// Manages all F-- packages - only loads what user imports
    /// </summary>
    public class PackageManager
    {
        private static PackageManager? _instance;
        private readonly Dictionary<string, BasePackage> _loadedPackages = new();
        private readonly Dictionary<string, Type> _availablePackages = new();
        
        private PackageManager()
        {
            // Scan for available packages
            DiscoverPackages();
        }
        
        public static PackageManager Instance => _instance ??= new PackageManager();
        
        /// <summary>
        /// Discover all available packages in the assembly
        /// </summary>
        private void DiscoverPackages()
        {
            var packageTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(BasePackage)) && !t.IsAbstract);
            
            foreach (var type in packageTypes)
            {
                try
                {
                    var instance = Activator.CreateInstance(type) as BasePackage;
                    if (instance != null)
                    {
                        _availablePackages[instance.Name] = type;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to load package {type.Name}: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Import a package by name
        /// </summary>
        public BasePackage? ImportPackage(string packageName)
        {
            // Check if already loaded
            if (_loadedPackages.TryGetValue(packageName, out var loadedPackage))
            {
                Console.WriteLine($"📦 Package '{packageName}' already loaded (v{loadedPackage.Version})");
                return loadedPackage;
            }
            
            // Check if available
            if (!_availablePackages.TryGetValue(packageName, out var packageType))
            {
                Console.WriteLine($"❌ Package '{packageName}' not found");
                return null;
            }
            
            // Load package
            try
            {
                var package = Activator.CreateInstance(packageType) as BasePackage;
                if (package != null)
                {
                    package.Initialize();
                    _loadedPackages[packageName] = package;
                    Console.WriteLine($"📦 Loaded package '{packageName}' v{package.Version}");
                    return package;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to load package '{packageName}': {ex.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Import multiple packages
        /// </summary>
        public void ImportPackages(IEnumerable<string> packageNames)
        {
            foreach (var name in packageNames)
            {
                ImportPackage(name);
            }
        }
        
        /// <summary>
        /// Check if package is loaded
        /// </summary>
        public bool IsPackageLoaded(string packageName)
        {
            return _loadedPackages.ContainsKey(packageName);
        }
        
        /// <summary>
        /// Get loaded package
        /// </summary>
        public BasePackage? GetPackage(string packageName)
        {
            return _loadedPackages.TryGetValue(packageName, out var package) ? package : null;
        }
        
        /// <summary>
        /// Call a method in a package
        /// </summary>
        public object? CallMethod(string packageName, string methodName, object?[] args)
        {
            var package = GetPackage(packageName);
            if (package == null)
            {
                throw new Exception($"Package '{packageName}' not loaded. Did you forget to import it?");
            }
            
            return package.CallMethod(methodName, args);
        }
        
        /// <summary>
        /// Get all loaded packages
        /// </summary>
        public IEnumerable<string> GetLoadedPackages() => _loadedPackages.Keys;
        
        /// <summary>
        /// Get all available packages
        /// </summary>
        public IEnumerable<string> GetAvailablePackages() => _availablePackages.Keys;
        
        /// <summary>
        /// Clear all loaded packages
        /// </summary>
        public void ClearPackages()
        {
            _loadedPackages.Clear();
        }
    }
}
