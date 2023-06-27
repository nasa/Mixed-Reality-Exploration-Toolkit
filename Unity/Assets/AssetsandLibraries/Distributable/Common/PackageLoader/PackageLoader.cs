// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace GOV.NASA.GSFC.XR
{
    /**
     * Package Loader
     * 
     * Encapsulates helper functions for packages, including location of the root
     * package directory and plugin loading.
     */
    public class PackageLoader
    {
#if !HOLOLENS_BUILD
#region DLLImports
        [DllImport("kernel32.dll", EntryPoint = "SetDllDirectory", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool I_SetDllDirectory(string lpPathName);
#endregion
#endif

        /**
         * Adds the package plugin path to the DLL search path
         * 
         * @param packagePath The root package path used to append the plugin subdirectories
         */
        private static void SetDLLPath(string packagePath)
        {
            var dllPath = "";

            // Make sure we locate the correct plugin directory
            string[] pluginsPath =  Directory.GetDirectories(packagePath, "Plugins", System.IO.SearchOption.AllDirectories);
            if (pluginsPath.Length == 0)
            {
                string message = "[PackageLoader] Plugins directory not found.";
                Debug.LogError(message);
                throw new FileNotFoundException(message);
            }

#if UNITY_EDITOR_32
            // Find the 32 bit architecture dir (x86)
            string[] archDirs = Directory.GetDirectories(pluginsPath[0], "x86", System.IO.SearchOption.AllDirectories);
            if (archDirs.Length == 0)
            {
                string message = "[PackageLoader] 32-bit plugins directory not found.";
                Debug.LogError(message);
                throw new FileNotFoundException(message);
            }
            Debug.Log("[PackageLoader] Detected a 32-bit architecture. Will run 32-bit plugin.");
            dllPath = archDirs[0].Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
#elif UNITY_EDITOR_64
            // Find the 64 bit architecture dir (x64 or x86_64)
            string[] archDirs = Directory.GetDirectories(pluginsPath[0], "*64", System.IO.SearchOption.AllDirectories);
            if (archDirs.Length == 0)
            {
                string message = "[PackageLoader] 64-bit plugins directory not found.";
                Debug.LogError(message);
                throw new FileNotFoundException(message);
            }
            Debug.Log("[PackageLoader] Detected a 64-bit architecture. Will run 64-bit plugin.");
            dllPath = archDirs[0].Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
#else // Player
            // Just use the plugins directory
            dllPath = pluginsPath[0];
            Debug.Log("[PackageLoader] Unknown architecture. Assuming runtime player.");
#endif

// SetDllDirectory only supports one directory, so also add the directory to the path
#if !HOLOLENS_BUILD
            string currentPath = Environment.GetEnvironmentVariable("PATH");
            if (!currentPath.Contains(dllPath))
            {
                if (!currentPath.EndsWith(";"))
                {
                    currentPath += ";";
                }
                Environment.SetEnvironmentVariable("PATH", currentPath + dllPath);
                I_SetDllDirectory(dllPath);
                Debug.Log("[PackageLoader] Plugin directory added to PATH: " + dllPath);
                Debug.Log("[PackageLoader] Current PATH: " + Environment.GetEnvironmentVariable("PATH"));
            }
#endif
        }

        /**
         * Obtains the root package path relative to the supplied data path.
         * 
         * @param dataPath The data path to be used to search for the supplied package name
         * @param packagename The name of the package to search
         * 
         * @return The fully qualified package path, or NULL if not found
         */
        public static string GetPackagePath(string dataPath, string packageName)
        {
#if (HOLOLENS_BUILD && !UNITY_EDITOR)
            return Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "MRET");
#endif

            string[] res = Directory.GetDirectories(dataPath, packageName, System.IO.SearchOption.AllDirectories);
            if (res.Length == 0)
            {
                Debug.LogWarning("[PackageLoader] Problem locating the package: '" + packageName + "'. Defaulting to data path: " + dataPath);

                // Default to the data path
                res = new string[1];
                res[0] = dataPath;
            }
            return res[0].Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        /**
         * Initializes the package plugin.
         * 
         * @param packagePath The root package path used to initialize the plugin directory
         * 
         * @see #GetPackagePath
         */
        public static void InitializePackagePlugin(string packagePath)
        {
#if HOLOLENS_BUILD
            return;
#endif
            if (string.IsNullOrEmpty(packagePath))
            {
                Debug.LogError("[PackageLoader] Invalid package path supplied");
                return;
            }

            // Set the DLL path
            SetDLLPath(packagePath);
        }

    }
}