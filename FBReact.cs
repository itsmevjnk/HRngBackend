/*
 * FBReact.cs - Class for storing information on a person's reaction
 *              to a Facebook post.
 * Created on: 21:45 30-12-2021
 * Author    : itsmevjnk
 */

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
}
