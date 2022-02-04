/*
 * FBComment.cs - Class for storing information on a Facebook comment.
 * Created on: 16:40 29-12-2021
 * Author    : itsmevjnk
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace HRngBackend
{
    public class FBComment
    {
        /// <summary>
        ///  The comment's ID.
        /// </summary>
        public long ID = -1;

        /// <summary>
        ///  The ID of the comment's parent (optional).<br/>
        ///  The default value of -1 indicates that this comment has no parents.
        /// </summary>
        public long Parent = -1;

        /// <summary>
        ///  The user ID of the comment's author.
        /// </summary>
        public long AuthorID = -1;

        /// <summary>
        ///  The comment author's name (optional).
        /// </summary>
        public string AuthorName = "";

        /// <summary>
        ///  The comment's text, stripped of all HTML tags.<br/>
        ///  Can be left blank if there's no text in the comment.
        /// </summary>
        public string CommentText = "";

        /// <summary>
        ///  HTML code of the comment's text portion (optional).
        /// </summary>
        public string CommentText_HTML = "";

        /// <summary>
        ///  List containing handles of accounts mentioned in the comment.
        /// </summary>
        public HashSet<string> Mentions_Handle = new HashSet<string>();

        /// <summary>
        ///  List containing UIDs of accounts mentioned in the comment (optional).
        /// </summary>
        public HashSet<long> Mentions_UID = new HashSet<long>();

        /// <summary>
        ///  Title of the embed section underneath the comment's text (usually for external links).<br/>
        ///  Can be left blank if there's none.
        /// </summary>
        public string EmbedTitle = "";

        /// <summary>
        ///  URL of the embed section underneath the comment's text (usually for external links).<br/>
        ///  Can be left blank if there's none.
        /// </summary>
        public string EmbedURL = "";

        /// <summary>
        ///  URL of the attached image.<br/>
        ///  Can be left blank if there's none.
        /// </summary>
        public string ImageURL = "";

        /// <summary>
        ///  URL of the attached video.<br/>
        ///  Can be left blank if there's none.
        /// </summary>
        public string VideoURL = "";

        /// <summary>
        ///  URL of the attached sticker.<br/>
        ///  Can be left blank if there's none.
        /// </summary>
        public string StickerURL = "";
    }
}
