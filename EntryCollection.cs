﻿/*
 * EntryCollection.cs - Class for containing Entry objects (declared
 *                      in Entry.cs) and spreadsheet parsing/writing
 *                      information.
 * Created on: 17:46 02-01-2022
 * Author    : itsmevjnk
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace HRngBackend
{
    public class EntryCollection
    {
        /// <summary>
        ///  List of Entry objects.
        /// </summary>
        public List<Entry> Entries = new List<Entry>();

        /// <summary>
        ///  Set of all UIDs of all entries.
        /// </summary>
        public HashSet<long> UID = new HashSet<long>();

        /// <summary>
        ///  List of header names of each column in the input/output spreadsheet.
        /// </summary>
        public Dictionary<int, string> Headers = new Dictionary<int, string>();

        /// <summary>
        ///  The column number (starting from 0) where the UIDs will be stored.
        /// </summary>
        public int UIDColumn = -1;

        /// <summary>
        ///  Populate the <c>EntryCollection</c> object with information from a spreadsheet.
        /// </summary>
        /// <param name="sheet">The input spreadsheet.</param>
        /// <param name="start_row">The starting row number (starting from 0) of the input spreadsheet (i.e. the row where headers are put in) (optional). Defaults to 0.</param>
        /// <param name="uid_name">The name of the UID column (optional). Defaults to <c>UID</c>.</param>
        /// <param name="uid_col">
        ///  The column number (starting from 0) where the UIDs will be stored (optional).<br/>
        ///  If specified, this will override <paramref name="uid_name"/> unless the specified column number is out of the spreadsheet's range.
        /// </param>
        /// <param name="uid_delim">The delimiter character(s) for separating UIDs (optional). Defaults to <c>Environment.NewLine</c>.</param>
        /// <exception cref="FormatException">Thrown if the UID column cannot be found.</exception>
        public void FromSpreadsheet(Spreadsheet sheet, int start_row = 0, string uid_name = "UID", int uid_col = -1, string uid_delim = null)
        {
            uid_delim = uid_delim ?? Environment.NewLine; // Set the UID delimiter

            /* Retrieve the header for each column. Columns without the header will be ignored. */
            for (int i = 0; i < sheet.Columns; i++)
            {
                if (sheet.Data.ContainsKey((start_row, i))) Headers.Add(i, sheet.Data[(start_row, i)]);
            }

            /* Find the UID column */
            if (uid_col >= 0 && Headers.ContainsKey(uid_col)) UIDColumn = uid_col;
            else
            {
                foreach (var header in Headers)
                {
                    if(header.Value == uid_name)
                    {
                        UIDColumn = header.Key;
                        break;
                    }
                }
            }
            if (UIDColumn < 0) throw new FormatException("Cannot find UID column");

            /* Read entries */
            for (int i = start_row + 1; i < sheet.Rows; i++)
            {
                Entry entry = new Entry();

                /* UID cell */
                if (sheet.Data.ContainsKey((i, UIDColumn)))
                {
                    foreach (string uid in sheet.Data[(i, UIDColumn)].Split(uid_delim, StringSplitOptions.RemoveEmptyEntries))
                    {
                        entry.UID.Add(Convert.ToInt64(uid));
                        UID.Add(Convert.ToInt64(uid));
                    }
                }

                /* Other data cells */
                foreach (int col in Headers.Keys)
                {
                    if (col != UIDColumn)
                    {
                        entry.Data.Add(col, (sheet.Data.ContainsKey((i, col))) ? sheet.Data[(i, col)] : "");
                    }
                }

                Entries.Add(entry);
            }
        }

        /// <summary>
        ///  Outputs a Spreadsheet with information from the EntryCollection.
        /// </summary>
        /// <param name="start_row">The starting row number (starting from 0) for the resulting spreadsheet (which is where the headers will go) (optional). Defaults to 0.</param>
        /// <param name="uid_delim">The delimiter character(s) for separating UIDs (optional). Defaults to <c>Environment.NewLine</c>.</param>
        /// <returns>A <c>Spreadsheet</c> object containing the generated spreadsheet.</returns>
        public Spreadsheet ToSpreadsheet(int start_row = 0, string uid_delim = null)
        {
            uid_delim = uid_delim ?? Environment.NewLine;

            Spreadsheet sheet = new Spreadsheet();

            foreach (var header in Headers) sheet.Update((start_row, header.Key), header.Value); // Output the headers

            int row = start_row + 1; // The entry's row
            foreach (var entry in Entries)
            {
                foreach (var col in Headers.Keys)
                {
                    if (col != UIDColumn) sheet.Update((row, col), entry.Data[col]);
                }
                sheet.Update((row, UIDColumn), String.Join(uid_delim, entry.UID));
                row++;
            }

            return sheet;
        }

        /// <summary>
        ///  Append a column to the EntryCollection. This column will be placed at any empty space in between existing columns, or the end of the existing columns if there's no empty space.
        /// </summary>
        /// <param name="header">The header name for the new column.</param>
        /// <returns>The column number of the new column.</returns>
        public int AddColumn(string header)
        {
            int col, maxcol = Headers.Keys.Max();
            for(col = 0; col <= maxcol; col++)
            {
                if (!Headers.Keys.Contains(col)) break;
            }
            Headers.Add(col, header);
            foreach (var entry in Entries) entry.Data.Add(col, "");
            return col;
        }

        /// <summary>
        ///  Remove a column from the EntryCollection.
        /// </summary>
        /// <param name="col">The column number of the column to be removed.</param>
        /// <exception cref="ArgumentException">Thrown if the column specified in <paramref name="col"/> does not exist.</exception>
        public void RemoveColumn(int col)
        {
            if (!Headers.Keys.Contains(col)) throw new ArgumentException($"Attempting to remove nonexistant column {col}");
            Headers.Remove(col);
            foreach (var entry in Entries)
            {
                entry.Data.Remove(col);
                if (entry.IntData.ContainsKey(col)) entry.IntData.Remove(col);
            }
        }

        /// <summary>
        ///  Count the number of reactions associated with each entry.
        /// </summary>
        /// <param name="reactions">A list of <c>FBReact</c> objects storing reactions to be counted.</param>
        /// <param name="col">The column number to store reaction count. Must be set to store the reaction count.</param>
        /// <param name="col_log">The column number to store information on which reactions were used with which UID. Must be set to store this data.</param>
        /// <param name="log_sep">The string to separate the UID from the reaction type in a line (optional). Defaults to <c>: </c>.</param>
        /// <param name="log_delim">The delimiter to use in the log column if there are multiple UIDs reacting to the post (optional). Defaults to <c>Environment.NewLine</c>.</param>
        /// <param name="include">Array of reaction types to include (optional). If not specified, all types will be included.</param>
        /// <param name="exclude">Array of reaction types to exclude (optional). If not specified, no types will be excluded.</param>
        /// <exception cref="ArgumentException">Thrown if a specified column number does not exist.</exception>
        public void CountReactions(List<FBReact> reactions, int col = -1, int col_log = -1, string log_sep = ": ", string log_delim = null, ReactionEnum[] include = null, ReactionEnum[] exclude = null)
        {
            if (col == -1 && col_log == -1) return; // Nothing to do
            if (col != -1 && !Headers.Keys.Contains(col)) throw new ArgumentException($"Invalid column number {col}");
            if (col_log != -1 && !Headers.Keys.Contains(col_log)) throw new ArgumentException($"Invalid column number {col_log}");
            log_delim = log_delim ?? Environment.NewLine;
            foreach (var entry in Entries)
            {
                Dictionary<long, ReactionEnum> entry_reacts = new Dictionary<long, ReactionEnum>();
                foreach (var react in reactions)
                {
                    if (entry.UID.Contains(react.UserID) &&
                        (include == null || include.Contains(react.Reaction)) &&
                        (exclude == null || !exclude.Contains(react.Reaction)))
                        entry_reacts.Add(react.UserID, react.Reaction);
                }
                if (col != -1)
                {
                    entry.Data.Remove(col); 
                    entry.Data.Add(col, Convert.ToString(entry_reacts.Count));
                    if (entry.IntData.ContainsKey(col)) entry.IntData.Remove(col);
                    entry.IntData.Add(col, entry_reacts.Count);
                }
                if (col_log != -1)
                {
                    entry.Data.Remove(col_log);
                    List<string> logs = new List<string>();
                    foreach (var p in entry_reacts) logs.Add($"{p.Key}{log_sep}{Convert.ToString(p.Value)}");
                    entry.Data.Add(col_log, String.Join(log_delim, logs));
                }
            }
        }

        /// <summary>
        ///  Count the number of post shares associated with each entry.
        /// </summary>
        /// <param name="shares">A list of UIDs of accounts that shared the post.</param>
        /// <param name="col">The column number to store share count. Must be set to store the share count.</param>
        /// <param name="col_log">The column number to store information on which account shared the post. Must be set to store this data.</param>
        /// <param name="log_delim">The delimiter to use in the log column if there are multiple UIDs sharing the post (optional). Defaults to <c>Environment.NewLine</c>.</param>
        /// <exception cref="ArgumentException">Thrown if a specified column number does not exist.</exception>
        public void CountShares(List<long> shares, int col = -1, int col_log = -1, string log_delim = null)
        {
            if (col == -1 && col_log == -1) return; // Nothing to do
            if (col != -1 && !Headers.Keys.Contains(col)) throw new ArgumentException($"Invalid column number {col}");
            if (col_log != -1 && !Headers.Keys.Contains(col_log)) throw new ArgumentException($"Invalid column number {col_log}");
            log_delim = log_delim ?? Environment.NewLine;
            foreach (var entry in Entries)
            {
                List<long> entry_shares = new List<long>();
                foreach (var uid in entry.UID)
                {
                    if (shares.Contains(uid)) entry_shares.Add(uid);
                }
                if (col != -1)
                {
                    entry.Data.Remove(col);
                    entry.Data.Add(col, Convert.ToString(entry_shares.Count));
                    if (entry.IntData.ContainsKey(col)) entry.IntData.Remove(col);
                    entry.IntData.Add(col, entry_shares.Count);
                }
                if (col_log != -1)
                {
                    entry.Data.Remove(col_log);
                    entry.Data.Add(col_log, String.Join(log_delim, entry_shares));
                }
            }
        }

        /// <summary>
        ///  Count the number of comments made by each entry.
        /// </summary>
        /// <param name="comments">A list of <c>FBComment</c> objects for each comment to be checked.</param>
        /// <param name="col">The column number to store comments count. Must be set to store this data.</param>
        /// <param name="col_cmts">The column number to store comment text. Must be set to store this data.</param>
        /// <param name="cmts_sep">The separator character(s) to separate the UID from the comment text (optional). Defaults to <c>: </c>.</param>
        /// <param name="cmts_delim">The delimiter character(s) to separate comments (optional). Defaults to <c>Environment.NewLine</c>.</param>
        /// <param name="col_ment">The column number to store mentions count. Must be set to store this data.</param>
        /// <param name="col_mdet">The column number to store detailed information on mentions (i.e. which accounts were mentioned). Must be set to store this data.</param>
        /// <param name="mdet_sep">The separator character(s) to separate the mentioned UIDs (optional). Defaults to <c>, </c>.</param>
        /// <param name="replies">Whether to count replies. Disabled by default.</param>
        /// <param name="ment_exc">Whether to not count mentioned accounts that are in the EntryCollection. Enabled by default.</param>
        /// <exception cref="ArgumentException">Thrown if a specified column number does not exist.</exception>
        /// <exception cref="FormatException">Thrown if <paramref name="comments"/> does not contain any UIDs for mentioned accounts.</exception>
        public void CountComments(List<FBComment> comments, int col = -1, int col_cmts = -1, string cmts_sep = ": ", string cmts_delim = null, int col_ment = -1, int col_mdet = -1, string mdet_sep = ", ", bool replies = false, bool ment_exc = true)
        {
            if (col == -1 && col_cmts == -1 && col_ment == -1 && col_mdet == -1) return; // Nothing to do
            if (col != -1 && !Headers.Keys.Contains(col)) throw new ArgumentException($"Invalid column number {col}");
            if (col_cmts != -1 && !Headers.Keys.Contains(col_cmts)) throw new ArgumentException($"Invalid column number {col_cmts}");
            if (col_ment != -1 && !Headers.Keys.Contains(col_ment)) throw new ArgumentException($"Invalid column number {col_ment}");
            if (col_mdet != -1 && !Headers.Keys.Contains(col_mdet)) throw new ArgumentException($"Invalid column number {col_mdet}");
            cmts_delim = cmts_delim ?? Environment.NewLine;
            foreach (var entry in Entries)
            {
                List<string> cmt_text = new List<string>();
                HashSet<long> cmt_mentions = new HashSet<long>();
                foreach (var comment in comments)
                {
                    if (entry.UID.Contains(comment.AuthorID) && (replies || comment.Parent == -1))
                    {
                        if (col != -1 || col_cmts != -1) cmt_text.Add($"{comment.AuthorID}{cmts_sep}{comment.CommentText}");
                        if (col_ment != -1 || col_mdet != -1)
                        {
                            if (comment.Mentions_UID.Count == 0 && comment.Mentions_Handle.Count != 0) throw new FormatException("muid must be true in IFBPost.GetComments() for mentions counting to work");
                            foreach (var uid in comment.Mentions_UID)
                            {
                                if (!ment_exc || !UID.Contains(uid)) cmt_mentions.Add(uid);
                            }
                        }
                    }
                }
                if (col != -1)
                {
                    entry.Data.Remove(col);
                    entry.Data.Add(col, Convert.ToString(cmt_text.Count));
                    if (entry.IntData.ContainsKey(col)) entry.IntData.Remove(col);
                    entry.IntData.Add(col, cmt_text.Count);
                }
                if (col_cmts != -1)
                {
                    entry.Data.Remove(col_cmts);
                    entry.Data.Add(col_cmts, String.Join(cmts_delim, cmt_text));
                }
                if (col_ment != -1)
                {
                    entry.Data.Remove(col_ment);
                    entry.Data.Add(col_ment, Convert.ToString(cmt_mentions.Count));
                    if (entry.IntData.ContainsKey(col_ment)) entry.IntData.Remove(col_ment);
                    entry.IntData.Add(col_ment, cmt_mentions.Count);
                }
                if (col_mdet != -1)
                {
                    entry.Data.Remove(col_mdet);
                    entry.Data.Add(col_mdet, String.Join(mdet_sep, cmt_mentions));
                }
            }
        }
    }
}
