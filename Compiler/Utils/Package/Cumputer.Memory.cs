using System;

namespace Fminusminus.Utils.Package
{
    /// <summary>
    /// Memory package - memory management with limits
    /// </summary>
    public class MemoryPackage : BasePackage
    {
        public override string Name => "memory";
        public override string Version => "1.0.0";
        public override string Description => "Secure memory management for F--";
        
        private long _totalMemory = 1024; // MB
        private long _usedMemory = 256;
        private long _memoryLeft => _totalMemory - _usedMemory;
        
        private int _allocCount;
        private const int MaxAllocations = 1000;
        private const int MaxAllocationSize = 512; // MB
        private const int MinAllocationSize = 1; // MB

        public override void Initialize()
        {
            _methods["GetTotal"] = args => _totalMemory;
            _methods["GetUsed"] = args => _usedMemory;
            _methods["GetFree"] = args => _memoryLeft;
            
            _methods["PrintTotal"] = args =>
            {
                Console.WriteLine($"Total Memory: {_totalMemory} MB");
                return null;
            };
            
            _methods["PrintUsed"] = args =>
            {
                Console.WriteLine($"Used Memory: {_usedMemory} MB");
                return null;
            };
            
            _methods["PrintFree"] = args =>
            {
                Console.WriteLine($"Free Memory: {_memoryLeft} MB");
                return null;
            };
            
            _methods["Allocate"] = args =>
            {
                if (args.Length > 0)
                {
                    int size = Convert.ToInt32(args[0]);
                    return Allocate(size);
                }
                return null;
            };
            
            _methods["Free"] = args =>
            {
                if (args.Length > 0)
                {
                    int size = Convert.ToInt32(args[0]);
                    return Free(size);
                }
                return null;
            };
            
            _methods["GC"] = args =>
            {
                GC();
                return null;
            };
            
            _methods["GetMemoryInfo"] = args => GetMemoryInfo();
        }

        private bool Allocate(int size)
        {
            // Validate size
            if (size < MinAllocationSize || size > MaxAllocationSize)
            {
                Console.WriteLine($"⚠️ Allocation size must be between {MinAllocationSize} and {MaxAllocationSize} MB");
                return false;
            }

            // Check allocation count
            if (_allocCount++ > MaxAllocations)
            {
                Console.WriteLine("⚠️ Too many allocations. Please free some memory first.");
                return false;
            }

            // Check if enough memory
            if (_usedMemory + size > _totalMemory)
            {
                Console.WriteLine($"⚠️ Not enough memory. Available: {_memoryLeft} MB");
                return false;
            }

            _usedMemory = Math.Min(_totalMemory, _usedMemory + size);
            Console.WriteLine($"📦 Allocated {size} MB (Total used: {_usedMemory} MB)");
            return true;
        }

        private bool Free(int size)
        {
            if (size < MinAllocationSize || size > MaxAllocationSize)
            {
                Console.WriteLine($"⚠️ Free size must be between {MinAllocationSize} and {MaxAllocationSize} MB");
                return false;
            }

            if (_usedMemory - size < 256) // Can't go below base memory
            {
                Console.WriteLine("⚠️ Cannot free below base memory (256 MB)");
                return false;
            }

            _usedMemory = Math.Max(256, _usedMemory - size);
            _allocCount = Math.Max(0, _allocCount - 1);
            
            Console.WriteLine($"🔄 Freed {size} MB (Total used: {_usedMemory} MB)");
            return true;
        }

        private void GC()
        {
            _usedMemory = 256; // Reset to base
            _allocCount = 0;
            System.GC.Collect();
            Console.WriteLine("🧹 Garbage collection completed");
        }

        private string GetMemoryInfo()
        {
            return $@"
╔══════════════════════════════════════╗
║         MEMORY INFORMATION           ║
╠══════════════════════════════════════╣
║ Total:  {_totalMemory,8} MB                    ║
║ Used:   {_usedMemory,8} MB                    ║
║ Free:   {_memoryLeft,8} MB                    ║
║ Usage:  {(_usedMemory * 100 / _totalMemory),7}%                    ║
║ Allocs: {_allocCount,7} / {MaxAllocations, -7}               ║
╚══════════════════════════════════════╝";
        }
    }
}
