using System;

namespace ExternalProcessFunctions.Services
{
    public class EnvironmentManager : IEnvironmentManager
    {
        public bool Is64Bit => Environment.Is64BitOperatingSystem;
        
    }
}