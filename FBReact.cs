/*
 * FBReact.cs - Class for storing information on a person's reaction
 *              to a Facebook post.
 * Created on: 21:45 30-12-2021
 * Author    : itsmevjnk
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using HtmlAgilityPack;

namespace HRngBackend
{
    /// <summary>
    ///  Enumeration for storing reaction type.
    /// </summary>
    public enum ReactionEnum
    {
        None = -1,
        Like = 1,
        Love,
        Wow,
        Haha,
        Sad = 7,
        Angry,
        Thankful = 11,
        Pride,
        Care = 16
    }

    public class FBReact
    {
        /// <summary>
        ///  The user's ID.
        /// </summary>
        public long UserID = -1;

        /// <summary>
        ///  The user's name (optional).
        /// </summary>
        public string UserName = "";
        
        /// <summary>
        ///  The user's reaction.
        /// </summary>
        public ReactionEnum Reaction = ReactionEnum.None;
    }

    public static class FBReactUtil
    {
        /// <summary>
        ///  The reactions lookup table to be used by GetReaction(). Needs to be manually set by the main function during initialization (using GetLut()).
        /// </summary>
        public static dynamic? ReactionsLut { get; set; }

        /// <summary>
        ///  Retrieves the reactions lookup table.
        /// </summary>
        public static async Task GetLut()
        {
            var resp = await CommonHTTP.Client.GetAsync("https://raw.githubusercontent.com/itsmevjnk/HRngBackend/main/Reactions.json");
            resp.EnsureSuccessStatusCode();
            ReactionsLut = JsonConvert.DeserializeObject(await resp.Content.ReadAsStringAsync());
        }

        /// <summary>
        ///  Helper function to match a reaction type <c>&lt;i&gt;></c> element against a list of patterns.
        /// </summary>
        /// <param name="elem">The element to be matched against.</param>
        /// <param name="pattern">The list of "patterns" (class and style (optional)).</param>
        /// <returns></returns>
        private static bool GetReactionType(HtmlNode elem, dynamic pattern)
        {
            var e_class = elem.Attributes["class"].DeEntitizeValue.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (dynamic p in pattern)
            {
                var p_eclass = new List<string>(); foreach (string c in p.eclass) p_eclass.Add(c); // Very painful, but looks like this is the only way
                if (e_class.Count == p_eclass.Count && e_class.All(p_eclass.Contains))
                {
                    if (!p.ContainsKey("estyle")) return true; // No style to check
                    /* Check style */
                    if (elem.Attributes["style"] == null) return false;
                    var p_estyle = new Dictionary<string, string>(); foreach (var s in p.estyle) p_estyle.Add(s.Name, s.Value.Value);
                    var e_style = new Dictionary<string, string>();
                    foreach (var s in elem.Attributes["style"].DeEntitizeValue.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        var s_pair = s.Split(':', StringSplitOptions.TrimEntries);
                        e_style.Add(s_pair[0], s_pair[1]);
                    }
                    if (e_style.Count == p_estyle.Count && e_style.All(p_estyle.Contains)) return true;
                }
            }
            return false;
        }

        /// <summary>
        ///  Identifies and returns the reaction type from an <c>&lt;i&gt;</c> element.
        /// </summary>
        public static ReactionEnum GetReaction(HtmlNode elem)
        {
            ReactionEnum ret = ReactionEnum.None;
            if (GetReactionType(elem, ReactionsLut.like)) ret = ReactionEnum.Like;
            else if (GetReactionType(elem, ReactionsLut.care)) ret = ReactionEnum.Care;
            else if (GetReactionType(elem, ReactionsLut.love)) ret = ReactionEnum.Love;
            else if (GetReactionType(elem, ReactionsLut.haha)) ret = ReactionEnum.Haha;
            else if (GetReactionType(elem, ReactionsLut.wow)) ret = ReactionEnum.Wow;
            else if (GetReactionType(elem, ReactionsLut.sad)) ret = ReactionEnum.Sad;
            else if (GetReactionType(elem, ReactionsLut.angry)) ret = ReactionEnum.Angry;
            else if (GetReactionType(elem, ReactionsLut.pride)) ret = ReactionEnum.Pride;
            else if (GetReactionType(elem, ReactionsLut.thankful)) ret = ReactionEnum.Thankful;
            return ret;
        }
    }
}
