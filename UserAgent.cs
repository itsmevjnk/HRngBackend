﻿/*
 * UserAgent6.cs - User-Agent string generator functions.
 * Created on: 30-11-2021 17:15
 * Author    : itsmevjnk
 */

using System;

namespace HRngBackend
{
    public static class UserAgent
    {
        /* Private helper functions to keep our code modular */

        /// <summary>
        ///  Generates a random Microsoft Windows system information string.<br/>
        ///  The Windows NT version can be <c>6.1</c> (7), <c>6.2</c> (8), <c>6.3</c> (8.1), or <c>10.0</c> (10/11). Note that NT 6.0 and earlier (Vista and earlier) are not included; the Windows versions that are based on those are obsolete, and therefore, having something like Chrome 94 paired with Windows XP would be highly unrealistic.<br/>
        ///  The architecture can be either 32-bit (Win32/x86), 64-bit (Win64/x64), or WoW64 (32-bit binary running on 64-bit OS) (WOW64).
        /// </summary>
        /// <returns>A string containing the system information string.</returns>
        private static string UASystemWindows()
        {
            Random rand = new Random(); // Random generator object, available from System

            string[] versions = { "6.1", "6.2", "6.3", "10.0" }; // Windows NT kernel version
            int idx = rand.Next(versions.Length); // Generate random version index

            /* Select random architecture */
            switch (rand.Next(3))
            {
                case 0: return $"Windows NT {versions[idx]}; Win32; x86";
                case 1: return $"Windows NT {versions[idx]}; Win64; x64";
                case 2: return $"Windows NT {versions[idx]}; WOW64"; // 32-bit browser binary running on 64-bit OS installation
                default: return ""; // Not going to happen (unless rand.Next() is broken)
            }
        }

        /// <summary>
        ///  Generates a random macOS (formerly Mac OS X) system information string.<br/>
        ///  The macOS version is a random valid version from 10.10 (Yosemite) to 10.12.1 (Monterey). Older versions are not included as they're no longer supported by Chrome (and probably most common browsers).<br/>
        ///  (Probably) for compatibility purposes, Apple seems to have decided that Apple Silicon-based Macs would report as Intel-based Macs. However, if this changes in the future, we might need to modify our code a bit to include Apple Silicon architecture.
        /// </summary>
        /// <returns>A string containing the system information string.</returns>
        private static string UASystemOSX()
        {
            Random rand = new Random();

            /* All Mac OS X/macOS releases since 10.10 */
            string[] versions =
            {
                "10_10", "10_10_0", "10_10_1", "10_10_2", "10_10_3", "10_10_4", "10_10_5", // Yosemite. Note the 10_10_0 version; some browsers do that.
                "10_11", "10_11_0", "10_11_1", "10_11_2", "10_11_3", "10_11_4", "10_11_5", "10_11_6", // El Capitan
                "10_12", "10_12_0", "10_12_1", "10_12_2", "10_12_3", "10_12_4", "10_12_5", "10_12_6", // Sierra
                "10_13", "10_13_0", "10_13_1", "10_13_2", "10_13_3", "10_13_4", "10_13_5", "10_13_6", // High Sierra
                "10_14", "10_14_0", "10_14_1", "10_14_2", "10_14_3", "10_14_4", "10_14_5", "10_14_6", // Mojave
                "10_15", "10_15_0", "10_15_1", "10_15_2", "10_15_3", "10_15_4", "10_15_5", "10_15_6", "10_15_7", // Catalina
                "10_16", "10_16_0", // Big Sur 11.0 (some browser versions do this for early versions of Big Sur)
                "11_0", "11_0_0", // Big Sur 11.0
                "11_1", "11_1_0", // Big Sur 11.1
                "11_2", "11_2_0", "11_2_1", "11_2_2", "11_2_3", // Big Sur 11.2
                "11_3", "11_3_0", "11_3_1", // Big Sur 11.3
                "11_4", "11_4_0", // Big Sur 11.4
                "11_5", "11_5_0", "11_5_1", "11_5_2", // Big Sur 11.5
                "11_6", "11_6_0", "11_6_1", // Big Sur 11.6
                "12_0", "12_0_0", "12_0_1" // Monterey 12.0
            };
            int idx = rand.Next(versions.Length);

            return $"Macintosh; Intel Mac OS X {versions[idx]}";
        }

        /// <summary>
        ///  Generates a random Linux system information string.<br/>
        ///  Including the entire Linux platform list would be too time-consuming (Linux has even been ported to Motorola 68k), so only the 3 most common platforms are included: <c>i686</c> (32-bit x86), <c>x86_64</c> (64-bit x86, aka amd64), and <c>aarch64</c> (64-bit ARM).<br/>
        ///  As Wayland is not really popular yet, only X11 will be included.
        /// </summary>
        /// <returns>A string containing the system information string.</returns>
        private static string UASystemLinux()
        {
            Random rand = new Random();

            string[] platforms = { "i686", "x86_64", "aarch64" };
            int idx = rand.Next(platforms.Length);

            return $"X11; Linux {platforms[idx]}";
        }

        /// <summary>
        ///  Generates a random Mozilla Firefox platform information string.
        /// </summary>
        /// <returns>A string containing the platform information string.</returns>
        private static string UAPlatformFirefox()
        {
            Random rand = new Random();
            int ver = 60 + rand.Next(35); // Random Firefox version from 60 to 94 (the current one as of the time of writing)
            return $"; rv:{ver}.0) Gecko/20100101 Firefox/{ver}.0";
        }

        /// <summary>
        ///  Generates a random Google Chrome/Chromium platform information string.
        /// </summary>
        /// <returns>A string containing the platform information string.</returns>
        private static string UAPlatformChrome()
        {
            Random rand = new Random();

            /* List of stable Chrome versions from 80  */
            string[] versions =
            {
                "80.0.3987.100", "81.0.4044.92", "83.0.4103.97", "84.0.4147.89",
                "85.0.4183.83", "86.0.4240.80", "87.0.4280.67", "88.0.4324.192",
                "89.0.4389.114", "90.0.4430.212", "91.0.4422.77", "92.0.4515.107",
                "93.0.4577.82", "94.0.4606.54", "95.0.4638.54", "96.0.4664.45"
            };
            int idx = rand.Next(versions.Length);

            return $") AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{versions[idx]} Safari/537.36";
        }

        /// <summary>
        ///  Generates a random valid <c>User-Agent</c> string.
        /// </summary>
        /// <returns>A string containing the <c>User-Agent</c> string.</returns>
        public static string Next()
        {
            string ret = "Mozilla/5.0 ("; // Return string. The Mozilla/5.0 part is present for historical/backwards compatibility purposes. We also add a round bracket here to contain our randomized platform info.

            /* System information */
            Random rand = new Random(); // This will be used throughout this function
            switch (rand.Next(2)) // As sad as this looks, turns out this is how to call a random function in C#
            {
                case 0: ret += UASystemWindows(); break;
                case 1: ret += UASystemOSX(); break;
                // Removed Linux as of now due to issues with m.facebook.com and mbasic.facebook.com
            }

            /* Platform information */
            ret += UAPlatformFirefox(); // Removed Chrome as of now due to issues with m.facebook.com and mbasic.facebook.com

            return ret;
        }
    }
}
