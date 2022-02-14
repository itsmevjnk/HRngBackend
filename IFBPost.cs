/*
 * IFBPost.cs - Abstraction for Facebook posts' information, as well as
 *              and their handling functions.
 * Created on: 17:44 14-02-2022
 * Author    : itsmevjnk
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRngBackend
{
    public interface IFBPost
    {
        /// <summary>
        ///  The Facebook post's ID. This is set by the initializer.
        /// </summary>
        public long PostID { get; }

        /// <summary>
        ///  The Facebook post author's user ID. This is set by the initializer.<br/>
        ///  This ID is needed for generating the comments URL (story.php) reliably. Without it, certain types of posts such as Facebook Watch videos will not load its comments.
        /// </summary>
        public long AuthorID { get; }

        /// <summary>
        ///  Whether this post is from a group.<br/>
        ///  Group posts require different handling in how the comments URL is constructed (i.e. no AuthorID, or else the page will bug out)
        /// </summary>
        public bool IsGroupPost { get; }

        /// <summary>
        ///  Attempt to get the IDs of the post and its author.
        /// </summary>
        /// <param name="id">The post's ID.</param>
        /// <returns>
        ///  0 if the initialization is successful, or one of these values on failure:
        ///  <list type="bullet">
        ///   <item><description>-1: Invalid URL</description></item>
        ///   <item><description>-2: Invalid webpage output (e.g. wrong URL, ratelimited by Facebook, or not logged in)</description></item>
        ///  </list>
        /// </returns>
        public Task<int> Initialize(long id);

        /// <summary>
        ///  Attempt to get the IDs of the post and its author.
        /// </summary>
        /// <param name="url">The post's URL.</param>
        /// <returns>
        ///  0 if the initialization is successful, or one of these values on failure:
        ///  <list type="bullet">
        ///   <item><description>-1: Invalid URL</description></item>
        ///   <item><description>-2: Invalid webpage output (e.g. wrong URL, ratelimited by Facebook, or not logged in)</description></item>
        ///  </list>
        /// </returns>
        public Task<int> Initialize(string url);

        /// <summary>
        ///  Scrape all comments from the Facebook post.
        /// </summary>
        /// <param name="cb">
        ///  Callback function to be called when each comment has been saved (optional).<br/>
        ///  This function takes the current percentage and returns <c>true</c> or <c>false</c>, depending on whether the user cancelled the operation.
        /// </param>
        /// <param name="muid">
        ///  Whether to retrieve UIDs of accounts mentioned in the comments (optional).<br/>
        ///  Enabled by default, however, this can be disabled for speed improvements if this data is unnecessary.
        /// </param>
        /// <param name="p1">
        ///  Whether to get all comments with an account logged in. Enabled by default.<br/>
        ///  Due to Facebook's algorithms, this pass does not guarantee obtainment of all comments.
        /// </param>
        /// <param name="p2">
        ///  Whether to get all comments WITHOUT logging in any account. Disabled by default.<br/>
        ///  This will allow for the maximum number of comments to be obtained (except for those from non-public accounts or those that Facebook's algorithms decide to hide). However, depending on the mentioned accounts' privacy settings, their UIDs/profile links may NOT be obtainable, in which case a decrementing number starting from -10 is used as the UID, and <c>&lt;name shown in the comment&gt; (&lt;UID&gt;)</c> is used as the profile link.
        /// </param>
        /// <returns>A comment ID =&gt; FBComment instance dictionary, or <c>null</c> if the function was cancelled.</returns>
        public Task<Dictionary<long, FBComment>> GetComments(Func<float, bool>? cb = null, bool muid = true, bool p1 = true, bool p2 = false);

        /// <summary>
        ///  Get all reactions to the post.
        /// </summary>
        /// <param name="cb">
        ///  Callback function to be called when each comment has been saved (optional).<br/>
        ///  This function takes the current percentage and returns <c>true</c> or <c>false</c>, depending on whether the user cancelled the operation.
        /// </param>
        /// <returns>A user ID => FBReact instance dictionary, or <c>null</c> if the function is cancelled.</returns>
        public Task<Dictionary<long, FBReact>> GetReactions(Func<float, bool>? cb = null);

        /// <summary>
        ///  Get the list of accounts that shared the post.
        /// </summary>
        /// <param name="cb">
        ///  Callback function to be called when each comment has been saved (optional).<br/>
        ///  This function takes the current percentage and returns <c>true</c> or <c>false</c>, depending on whether the user cancelled the operation.
        /// </param>
        /// <returns>A user ID => user name dictionary, or <c>null</c> if the function is cancelled.</returns>
        public Task<Dictionary<long, string>> GetShares(Func<float, bool>? cb = null);
    }
}
