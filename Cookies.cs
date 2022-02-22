/*
 * Cookies.cs - Functions for handling web cookies.
 * Created on: 12:25 27-12-2021
 * Author    : itsmevjnk
 */

using System;
using System.Collections.Generic;
using System.IO;

#nullable enable
namespace HRngBackend
{
    public static class Cookies
    {
        /// <summary>
        ///  Parse a key-value pair string of &lt;key&gt;=&lt;value&gt;; form into a string => string dictionary.
        /// </summary>
        /// <param name="kvpstr">Key-value pair string.</param>
        /// <param name="kvsep">Key-value separator character (optional). Defaults to <c>=</c> (equal).</param>
        /// <param name="psep">Pair separator character (optional). Defaults to <c>;</c> (semicolon).</param>
        /// <returns>A string => string dictionary containing the parsed cookies, or null if parsing fails.</returns>
        public static Dictionary<string, string>? FromKVPString(string kvpstr, char kvsep = '=', char psep = ';')
        {
            kvpstr = kvpstr.Replace(" ", ""); // Remove all whitespaces
            Dictionary<string, string> cookies = new Dictionary<string, string> { };
            foreach (string kvp in kvpstr.Split(psep, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] pair = kvp.Split(kvsep);
                if (pair.Length != 2) return null; // Invalid pair
                cookies.Add(pair[0], pair[1]);
            }
            return cookies;
        }

        /// <summary>
        ///  Parse a string containing Netscape formatted cookies (also known as <c>cookies.txt</c>) into a string =&gt; string dictionary.
        /// </summary>
        /// <param name="txtstr">The input string containing data from the <c>cookies.txt</c>-formatted file.</param>
        /// <returns>A string => string dictionary containing the parsed cookies, or null if parsing fails.</returns>
        public static Dictionary<string, string>? FromTxt_String(string txtstr)
        {
            Dictionary<string, string> cookies = new Dictionary<string, string> { };
            foreach (string line in txtstr.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)) // We don't need \r\n and \n\r since those will be split into an empty string, and deleted by the RemoveEmptyEntries option
            {
                if (line.StartsWith('#')) continue; // Skip comment lines
                string[] components = line.Split('\t'); // Tab delmited
                if (components.Length >= 7) cookies.Add(components[5], components[6]); // We only need the key and value
                else return null; // Parsing failed
            }
            return cookies;
        }

        /// <summary>
        ///  Parse a Netscape formatted cookies file (aka <c>cookies.txt</c>) into a string =&gt; string dictionary.
        /// </summary>
        /// <param name="path">Path to the <c>cookies.txt</c>-formatted file.</param>
        /// <returns>A string =&gt; string dictionary containing the parsed cookies, or null if parsing fails.</returns>
        public static Dictionary<string, string>? FromTxt_File(string path)
        {
            return FromTxt_String(File.ReadAllText(path));
        }
    }
}
#nullable disable
