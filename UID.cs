﻿/*
 * UID.cs - Facebook user ID (UID) retrieval functions.
 * Created on: 00:09 01-12-2021
 * Author    : itsmevjnk
 */

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HRngBackend
{
    public static class UID
    {
        /// <summary>
        ///  User name to UID cache.<br/>
        ///  While it's possible to read and write directly from this dictionary, this is not recommended, and improper use may result in undefined behavior. Use <c>Add()</c> and <c>Get()</c> to insert and retrieve UIDs, and use <c>ClearCache()</c> to clear the cache.
        /// </summary>
        public static Dictionary<string, long> Cache = new Dictionary<string, long>();

        /// <summary>
        ///  Mutex object for the user name to UID cache above.<br/>
        ///  While it's also possible to use <c>ConcurrentDirectory</c>, it's overly complicated for what we're trying to accomplish, which is making sure that only one thread may have exclusive access to the cache.
        /// </summary>
        public static Mutex CacheMutex = new Mutex();

        /// <summary>
        ///  Process a Facebook profile link and retrieve the profile's handle.<br/>
        ///  A handle is our extension of the Facebook user name, and can be either:
        ///  <list type="bullet">
        ///   <item><description>the user name (if one exists in the link)</description></item>
        ///   <item><description><c>:&lt;UID&gt;</c> (if the UID exists instead of the user name, e.g. <c>https://www.facebook.com/profile.php?id=&lt;UID&gt;</c>)</description></item>
        ///  </list>
        /// </summary>
        /// <param name="link">A <c>string</c> variable containing the Facebook profile link.</param>
        /// <returns>The handle in <c>string</c> type, or an empty string if the passed URL is invalid.</returns>
        public static string GetHandle(string link)
        {
            if (link.StartsWith(':') && link.Substring(1).All(char.IsDigit)) return link; // UID handle provided

            link = Regex.Replace(link, "^.*://", ""); // Remove the schema (aka http(s)://) from the link

            if (link == "") return ""; // Totally invalid link

            /* Split the link up into elements */
            string[] link_elements = link.ToLower().Split("/").Where(x => !String.IsNullOrEmpty(x) && !x.StartsWith('?')).ToArray(); // Use LINQ to handle removal of empty elements
            if (link_elements.Length == 1)
            {
                /* We probably get a link in the form of [/]profile.php or [/](UID/user name), i.e. not even a link */
                string[] query = link_elements[0].Split('?');
                if (query.Length == 0) return ""; // Invalid URL
                if (query[0] == "profile.php")
                {
                    if (query.Length == 1) return ""; // No parameters present
                    foreach (string q in query[1].Split('&'))
                    {
                        if (q.StartsWith("id=") && q.Length > "id=".Length && q.Substring("id=".Length).All(char.IsDigit)) return ":" + q.Substring("id=".Length);
                    }
                    return ""; // Cannot find id= parameter
                }
                else
                {
                    if (query[0].All(char.IsDigit)) return ":" + query[0]; // UID
                    else return query[0];
                }
            }
            /// <summary>
            ///  determine if it's a valid link
            /// </summary>
            if (link_elements[0].EndsWith("m.me"))
            {
                /* m.me/(UID or user name) */
                if (link_elements[1].All(char.IsDigit)) return ":" + link_elements[1]; // UID
                else return link_elements[1];
            }
            else
            {
                string[] domain = link_elements[0].Split('.'); // We also split it here
                if (Array.IndexOf(domain, "facebook") > -1)
                {
                    /* facebook.com */
                    link_elements = link_elements.Where(x => x != "home.php").ToArray(); // Remove possible home.php element
                    string[] query = link_elements[1].Split('?'); // Split parameter from path
                    if (query.Length == 0) return ""; // There's nothing at all
                    if (query[0] == "profile.php")
                    {
                        /* profile.php with parameter id=(UID) */
                        if (query.Length == 1) return ""; // No parameters present
                        foreach (string q in query[1].Split('&'))
                        {
                            if (q.StartsWith("id=") && q.Length > "id=".Length && q.Substring("id=".Length).All(char.IsDigit)) return ":" + q.Substring("id=".Length);
                        }
                        return ""; // Cannot find id= parameter
                    }
                    else
                    {
                        /* User name */
                        return query[0];
                    }
                }
                else if (Array.IndexOf(domain, "messenger") > -1)
                {
                    /* messenger.com */
                    link_elements = link_elements.Where(x => x != "t").ToArray(); // Remove possible /t/ element
                    if (link_elements[1].All(char.IsDigit)) return ":" + link_elements[1]; // UID
                    else return link_elements[1];
                }
                else return ""; // Invalid URL
            }
        }

        /// <summary>
        ///  Processes the link to get the handle, then add an entry to the UID cache.<br/>
        ///  If the link already has the UID as part of it, this function will still return <c>true</c>, but no entries will be added.
        /// </summary>
        /// <param name="link">The Facebook profile link.</param>
        /// <param name="uid">The UID that is associated with it.</param>
        /// <returns><c>true</c> if the entry can be added, or <c>false</c> if the link or UID is invalid.</returns>
        public static bool Add(string link, long uid)
        {
            if (uid <= 0) return false; // Invalid UID

            string handle = GetHandle(link);
            if (handle == "") return false; // Cannot get handle due to invalid URL
            if (handle.StartsWith(':')) return true; // Handle is UID, we don't need to put it in cache

            AddHandle(handle, uid);
            return true;
        }

        /// <summary>
        ///  Adds an entry to the UID cache by handle.
        /// </summary>
        /// <param name="handle">The Facebook profile handle.</param>
        /// <param name="uid">The UID that is associated with it.</param>
        private static void AddHandle(string handle, long uid)
        {
            CacheMutex.WaitOne(); // Wait until mutex is released
            foreach (var item in Cache.Where(kvp => kvp.Value == uid).ToList()) Cache.Remove(item.Key); // Remove all existing cache entries with our UID since each UID can only be associated with an user name
            if (!Cache.ContainsKey(handle)) Cache.Add(handle, uid); // Add to cache
            CacheMutex.ReleaseMutex(); // Release mutex after our operation finishes
        }

        /// <summary>
        ///  Clears the UID cache.
        /// </summary>
        public static void ClearCache()
        {
            CacheMutex.WaitOne();
            Cache.Clear();
            CacheMutex.ReleaseMutex();
        }

        /// <summary>
        ///  Private helper function for looking up UID from services. This function sends a POST request with data specified in [data] to the service specified in [service_url], then retrieves the UID using the XPath specified in [xpath] and converts it to a [long] integer.
        /// </summary>
        /// <param name="service_url">URL of the lookup service used.</param>
        /// <param name="data">POST request data.</param>
        /// <param name="xpath">XPath pointing to the element containing the UID returned by the service.</param>
        /// <param name="ctoken">Cancellation token for cancelling the task (optional).</param>
        /// <returns>The retrieved UID, or -1 on failure. If <paramref name="ctoken"/> is specified, -2 will be returned if the task is cancelled.</returns>
        private async static Task<long> LookupUID(string service_url, IDictionary<string, string> data, string xpath, CancellationToken? ctoken = null)
        {
            var rq_content = new FormUrlEncodedContent(data); // POST request data, converted to work with HttpClient
            for (int i = 0; i < 3 && (ctoken == null || !((CancellationToken)ctoken).IsCancellationRequested); i++) // retry for 3 times at most
            {
                /* Send POST request to service */
                string response_data = "";
                try
                {
                    HttpRequestMessage request_msg = new HttpRequestMessage // For custom User-Agent
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(service_url),
                        Headers =
                        {
                            { HttpRequestHeader.UserAgent.ToString(), UserAgent.Next() }
                        },
                        Content = rq_content
                    };
                    if (ctoken == null)
                    {
                        var response = await CommonHTTP.Client.SendAsync(request_msg); // Perform POST request
                        response.EnsureSuccessStatusCode();
                        response_data = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        CancellationToken token = (CancellationToken)ctoken;
                        var response = await CommonHTTP.Client.SendAsync(request_msg, cancellationToken: token); // Perform POST request with cancellation token
                        response.EnsureSuccessStatusCode();
                        response_data = await response.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception exc)
                {
                    if (ctoken != null && exc.GetType().IsAssignableFrom(typeof(TaskCanceledException)))
                    {
                        if (((CancellationToken)ctoken).IsCancellationRequested) return -2; // Task canceled
                        else return -1; // Timeout
                    }
                    continue;
                }

                /* Load and parse output */
                var htmldoc = new HtmlDocument();
                htmldoc.LoadHtml(response_data);
#nullable enable // So that the compiler is happy with what we do
                HtmlNode? uid_node = htmldoc.DocumentNode.SelectSingleNode(xpath);
#nullable disable
                if (uid_node == null) continue;
                string uid = uid_node.InnerText;
                if (uid.All(char.IsDigit)) return Convert.ToInt64(uid);
            }
            if (ctoken != null && ((CancellationToken)ctoken).IsCancellationRequested) return -2; // Task cancelled
            return -1; // Cannot retrieve UID
        }

        /// <summary>
        ///  Attempt to retrieve the UID of a Facebook link using these methods:
        ///  <list type="bullet">
        ///   <item><description>Getting UID directly from link</description></item>
        ///   <item><description>Cache lookup</description></item>
        ///   <item><description>Lookup using existing UID lookup services</description></item>
        ///   <item><description>Scraping from profile page (if Facebook login information is provided)</description></item>
        ///  </list> 
        /// </summary>
        /// <param name="link">The Facebook profile link to try to get its UID.</param>
        /// <returns>
        ///  The profile's UID, or one of these values on failure:
        ///  <list type="bullet">
        ///   <item><description>-1: Invalid link</description></item>
        ///   <item><description>-2: Cannot retrieve UID using any of the available methods</description></item>
        ///   <item><description>-3: Cannot retrieve UID, but it might be possible to do so using the last method (i.e. we have no login information)</description></item>
        ///   <item><description>-4: Cannot retrieve UID because the provided Facebook account has been ratelimited</description></item>
        ///   <item><description>-5: Cannot retrieve UID because the provided Facebook cookies is invalid (i.e. signed out)</description></item>
        ///  </list>
        /// </returns>
        public static async Task<long> Get(string link)
        {
            string handle = GetHandle(link); // From now on we will work with this handle
            if (handle == "") return -1; // Invalid link

            if (handle.StartsWith(':')) return Convert.ToInt64(handle.Substring(1)); // Return the UID if it can be found in the link

            if (Cache.ContainsKey(handle)) return Cache[handle]; // Return the UID from cache if available

            /* Online retrieval */
            long uid = -1; // Retrieved UID, used at the end for adding into cache and returning

            /* Attempt retrieval using lookup services */
            link = $"https://www.facebook.com/{handle}";
            (string url, Dictionary<string, string> data, string xpath)[] services =
            {
                ("https://findidfb.com/#", new Dictionary<string, string>{ { "url", link } }, "//div[@class='alert alert-success alert-dismissable']/b"),
                ("https://lookup-id.com/#", new Dictionary<string, string>{ { "fburl", link }, { "check", "Lookup" } }, "//span[@id='code']")
                /* TODO: Add more services. The more services we have in here, the more chance we have at getting UIDs without ratelimiting the user. */
            };
            List<Task<long>> svc_tasks = new List<Task<long>> { }; // Lookup task pool
            List<CancellationTokenSource> svc_cts = new List<CancellationTokenSource> { }; // List of cancellation token sources corresponding to the lookup tasks
            foreach (var service in services)
            {
                svc_cts.Add(new CancellationTokenSource()); // Create cancellation token sources
                svc_tasks.Add(Task.Run(() => { return LookupUID(service.url, service.data, service.xpath, svc_cts.Last().Token); }, cancellationToken: svc_cts.Last().Token)); // Add each lookup task to the pool
            }
            while (svc_tasks.Count > 0)
            {
                Task<long> finished_task = await Task.WhenAny(svc_tasks); // Wait until any task finishes
                svc_tasks.Remove(finished_task); // Remove from thread pool
                if (finished_task.Result > 0)
                {
                    // Task finishes successfully, cancel all the other tasks and return
                    foreach (var cts in svc_cts) cts.Cancel(); // Send cancellation signal to all tasks (including the one that finished, but that's okay)

                    uid = finished_task.Result;
                    goto retrieved;
                }
            }

            /* Attempt retrieval by scraping Facebook (if possible) */
            string response_data;
            try
            {
                var response = await CommonHTTP.Client.GetAsync($"https://mbasic.facebook.com/{handle}");
                response.EnsureSuccessStatusCode();
                response_data = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return -2; // Cannot retrieve UID (due to network error)
            }
            var htmldoc = new HtmlDocument();
            htmldoc.LoadHtml(response_data);
            /* In some cases we can get the UID by checking the [Join] button while not logged in */
#nullable enable
            HtmlNode? e = htmldoc.DocumentNode.SelectSingleNode("//a[starts-with(@href, '/r.php')]");
#nullable disable
            if (e != null)
            {
                uid = Convert.ToInt64(Regex.Replace(e.Attributes["href"].Value, "(^.*\\&rid=)|(\\&.*$)", ""));
                goto retrieved;
            }
            if (CommonHTTP.ClientHandler.CookieContainer.Count == 0) return -3; // No cookies to perform logged-in scraping
            /* Check if we are still at the login page */
            if (htmldoc.DocumentNode.SelectSingleNode("//a[starts-with(@href, '/recover')]") != null) return -5; // Password recovery link, only present when we're logging in
            /* Check if we have been ratelimited */
            if (htmldoc.DocumentNode.SelectSingleNode("//a[@href='https://www.facebook.com/help/177066345680802']") != null) return -4;
            /* Retrieve from block link, works for normal profiles */
            e = htmldoc.DocumentNode.SelectSingleNode("//a[starts-with(@href, '/privacy/touch/block/confirm/?bid=')]");
            if (e != null)
            {
                uid = Convert.ToInt64(Regex.Replace(e.Attributes["href"].Value, "(^.*\\?bid=)|(&.*$)", ""));
                goto retrieved;
            }
            /* Retrieve from [More] button link, works for pages */
            e = htmldoc.DocumentNode.SelectSingleNode("//a[starts-with(@href, '/pages/more/')]");
            if (e != null)
            {
                uid = Convert.ToInt64(Regex.Replace(e.Attributes["href"].Value, "\\/.*$", ""));
                goto retrieved;
            }

            return -2; // Cannot retrieve UID

        retrieved:
            AddHandle(handle, uid);
            return uid;
        }
    }
}
