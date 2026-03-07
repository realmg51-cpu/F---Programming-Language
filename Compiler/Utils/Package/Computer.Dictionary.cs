using System;
using System.Collections.Generic;
using System.Linq;

namespace Fminusminus.Utils.Package
{
    /// <summary>
    /// Dictionary package - key-value store with limits
    /// </summary>
    public class DictionaryPackage : BasePackage
    {
        public override string Name => "dictionary";
        public override string Version => "1.0.0";
        public override string Description => "Secure dictionary operations for F--";
        
        private Dictionary<string, Dictionary<string, object>> _dictionaries = new();
        
        private const int MaxDictCount = 100;
        private const int MaxDictSize = 10000;
        private const int MaxKeyLength = 256;
        private const int MaxValueLength = 10000;
        
        private int _totalOperations;
        private const int MaxOperations = 100000;

        public override void Initialize()
        {
            _methods["Create"] = args =>
            {
                if (args.Length > 0 && args[0] is string name)
                {
                    return CreateDictionary(name);
                }
                return false;
            };
            
            _methods["Set"] = args =>
            {
                if (args.Length >= 3 && args[0] is string dictName && 
                    args[1] is string key)
                {
                    return Set(dictName, key, args[2]);
                }
                return false;
            };
            
            _methods["Get"] = args =>
            {
                if (args.Length >= 2 && args[0] is string dictName && 
                    args[1] is string key)
                {
                    return Get(dictName, key);
                }
                return null;
            };
            
            _methods["Has"] = args =>
            {
                if (args.Length >= 2 && args[0] is string dictName && 
                    args[1] is string key)
                {
                    return Has(dictName, key);
                }
                return false;
            };
            
            _methods["Remove"] = args =>
            {
                if (args.Length >= 2 && args[0] is string dictName && 
                    args[1] is string key)
                {
                    return Remove(dictName, key);
                }
                return false;
            };
            
            _methods["Clear"] = args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    Clear(dictName);
                }
                return null;
            };
            
            _methods["Keys"] = args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    return GetKeys(dictName);
                }
                return Array.Empty<string>();
            };
            
            _methods["Values"] = args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    return GetValues(dictName);
                }
                return Array.Empty<object>();
            };
            
            _methods["Count"] = args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    return GetCount(dictName);
                }
                return 0;
            };
            
            _methods["Print"] = args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    PrintDictionary(dictName);
                }
                return null;
            };
            
            _methods["Exists"] = args =>
            {
                if (args.Length > 0 && args[0] is string dictName)
                {
                    return DictionaryExists(dictName);
                }
                return false;
            };
        }

        private bool CheckRateLimit()
        {
            if (_totalOperations++ > MaxOperations)
            {
                Console.WriteLine("⚠️ Too many dictionary operations. Please slow down.");
                return false;
            }
            return true;
        }

        private bool ValidateKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
                
            if (key.Length > MaxKeyLength)
            {
                Console.WriteLine($"⚠️ Key too long (max {MaxKeyLength} characters)");
                return false;
            }
            
            // Block keys that could be used for injection
            if (key.Contains("..") || key.Contains("/") || key.Contains("\\"))
            {
                Console.WriteLine("⚠️ Invalid key characters");
                return false;
            }
            
            return true;
        }

        private bool ValidateValue(object? value)
        {
            if (value == null) return true;
            
            string strValue = value.ToString() ?? "";
            if (strValue.Length > MaxValueLength)
            {
                Console.WriteLine($"⚠️ Value too long (max {MaxValueLength} characters)");
                return false;
            }
            
            return true;
        }

        private bool CreateDictionary(string name)
        {
            if (!CheckRateLimit()) return false;

            if (_dictionaries.Count >= MaxDictCount)
            {
                Console.WriteLine($"⚠️ Maximum number of dictionaries reached ({MaxDictCount})");
                return false;
            }

            if (!ValidateKey(name)) return false;

            if (!_dictionaries.ContainsKey(name))
            {
                _dictionaries[name] = new Dictionary<string, object>();
                Console.WriteLine($"📚 Created dictionary: '{name}'");
                return true;
            }
            
            Console.WriteLine($"⚠️ Dictionary '{name}' already exists");
            return false;
        }

        private bool Set(string dictName, string key, object? value)
        {
            if (!CheckRateLimit()) return false;
            if (!ValidateKey(key)) return false;
            if (!ValidateValue(value)) return false;

            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                if (dict.Count >= MaxDictSize)
                {
                    Console.WriteLine($"⚠️ Dictionary '{dictName}' is full (max {MaxDictSize} items)");
                    return false;
                }

                dict[key] = value!;
                return true;
            }
            
            Console.WriteLine($"❌ Dictionary '{dictName}' not found");
            return false;
        }

        private object? Get(string dictName, string key)
        {
            if (!CheckRateLimit()) return null;
            if (!ValidateKey(key)) return null;

            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                return dict.TryGetValue(key, out var value) ? value : null;
            }
            return null;
        }

        private bool Has(string dictName, string key)
        {
            if (!CheckRateLimit()) return false;
            if (!ValidateKey(key)) return false;

            return _dictionaries.TryGetValue(dictName, out var dict) && dict.ContainsKey(key);
        }

        private bool Remove(string dictName, string key)
        {
            if (!CheckRateLimit()) return false;
            if (!ValidateKey(key)) return false;

            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                return dict.Remove(key);
            }
            return false;
        }

        private void Clear(string dictName)
        {
            if (!CheckRateLimit()) return;

            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                dict.Clear();
                Console.WriteLine($"🧹 Cleared dictionary: '{dictName}'");
            }
        }

        private string[] GetKeys(string dictName)
        {
            if (!CheckRateLimit()) return Array.Empty<string>();

            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                return dict.Keys.Take(100).ToArray(); // Limit preview
            }
            return Array.Empty<string>();
        }

        private object[] GetValues(string dictName)
        {
            if (!CheckRateLimit()) return Array.Empty<object>();

            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                return dict.Values.Take(100).ToArray(); // Limit preview
            }
            return Array.Empty<object>();
        }

        private int GetCount(string dictName)
        {
            if (!CheckRateLimit()) return 0;

            return _dictionaries.TryGetValue(dictName, out var dict) ? dict.Count : 0;
        }

        private void PrintDictionary(string dictName)
        {
            if (!CheckRateLimit()) return;

            if (_dictionaries.TryGetValue(dictName, out var dict))
            {
                Console.WriteLine($"\n📚 Dictionary: '{dictName}' ({dict.Count} items)");
                Console.WriteLine("========================================");
                
                if (dict.Count == 0)
                {
                    Console.WriteLine("   (empty)");
                }
                else
                {
                    int count = 0;
                    foreach (var kvp in dict)
                    {
                        if (count++ >= 20) // Limit display
                        {
                            Console.WriteLine($"   ... and {dict.Count - 20} more items");
                            break;
                        }

                        string valueStr = kvp.Value?.ToString() ?? "null";
                        if (valueStr.Length > 50)
                            valueStr = valueStr.Substring(0, 47) + "...";
                        
                        Console.WriteLine($"   🔑 {kvp.Key}: {valueStr}");
                    }
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine($"❌ Dictionary '{dictName}' not found");
            }
        }

        private bool DictionaryExists(string dictName)
        {
            return _dictionaries.ContainsKey(dictName);
        }
    }
}
