/*
 * BaseDir.cs - Class for defining base directories' locations.
 * Created on: 11:00 25-12-2021
 * Author    : itsmevjnk
 */

using System.IO;
using System.Reflection;

namespace HRngBackend
{
    public static class BaseDir
    {
        /// <summary>
        ///  The base directory where common data (e.g. config files) is stored.
        /// </summary>
        public static string CommonBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        ///  The base directory where platform-specific data (e.g. browser or driver) is stored.
        /// </summary>
        public static string PlatformBase = Path.Combine(CommonBase, OSCombo.Combo);

        /// <summary>
        ///  Class constructor. Creates the PlatformBase directory if it doesn't exist.
        /// </summary>
        static BaseDir()
        {
            Directory.CreateDirectory(PlatformBase);
        }
    }
}
