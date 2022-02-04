/*
 * Cookies.cs - Functions for handling web cookies.
 * Created on: 12:25 27-12-2021
 * Author    : itsmevjnk
 */

using OpenQA.Selenium;
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

        /// <summary>
        ///  Load a single cookie to a Selenium browser session.
        /// </summary>
        /// <param name="driver">The driver instance for the Selenium browser session.</param>
        /// <param name="key">The cookie's key.</param>
        /// <param name="value">The cookie's value.</param>
        /// <param name="url">URL to the domain for which the cookie will be stored for (optional). If specified, this function will load the web page before setting the cookie.</param>
        public static void Se_LoadCookie(IWebDriver driver, string key, string value, string url = "")
        {
            if (url != "" && driver.Url != url) driver.Navigate().GoToUrl(url);
            driver.Manage().Cookies.AddCookie(new Cookie(key, value));
        }

        /// <summary>
        ///  Load a string =&gt; string dictionary containing cookies to a Selenium browser session.
        /// </summary>
        /// <param name="driver">The driver instance for the Selenium browser session.</param>
        /// <param name="cookies">The string =&gt; string dictionary containing the cookies.</param>
        /// <param name="url">
        ///  URL to the domain for which the cookies will be stored for (optional).<br/>
        ///  If specified, this function will load the web page before setting the cookies.
        /// </param>
        public static void Se_LoadCookies(IWebDriver driver, IDictionary<string, string> cookies, string url = "")
        {
            if (url != "" && driver.Url != url) driver.Navigate().GoToUrl(url);
            foreach (var item in cookies) driver.Manage().Cookies.AddCookie(new Cookie(item.Key, item.Value));
        }

        /// <summary>
        ///  Clear all cookies for a domain in a Selenium browser session.
        /// </summary>
        /// <param name="driver">The driver instance for the Selenium browser session.</param>
        /// <param name="url">
        ///  URL to the domain of which the cookies will be deleted (optional).<br/>
        ///  If specified, this function will load the web page before clearing the cookies.
        /// </param>
        public static void Se_ClearCookies(IWebDriver driver, string url = "")
        {
            if (url != "" && driver.Url != url) driver.Navigate().GoToUrl(url);
            driver.Manage().Cookies.DeleteAllCookies();
        }

        /// <summary>
        ///  Get all cookies associated with a domain in a Selenium browser session.
        /// </summary>
        /// <param name="driver">The driver instance for the Selenium browser session.</param>
        /// <param name="url">
        ///  The URL to retrieve cookies for (optional).<br/>
        ///  If specified, this function will load the page before getting its cookies.
        /// </param>
        /// <returns>A string =&gt; string dictionary containing the cookies.</returns>
        public static Dictionary<string, string> Se_SaveCookies(IWebDriver driver, string url = "")
        {
            if (url != "" && driver.Url != url) driver.Navigate().GoToUrl(url);
            Dictionary<string, string> cookies = new Dictionary<string, string>();
            foreach (var cookie in driver.Manage().Cookies.AllCookies)
            {
                if (!cookies.ContainsKey(cookie.Name)) cookies.Add(cookie.Name, cookie.Value);
            }
            return cookies;
        }
    }
}
#nullable disable
