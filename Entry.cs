/*
 * Entry.cs - Class for containing information on entries
 *            (e.g. a person whose interactions with a Facebook
 *            post are to be checked)
 * Created on: 16:32 02-01-2022
 * Author    : itsmevjnk
 */

using System.Collections.Generic;

namespace HRngBackend
{
    public class Entry
    {
        /// <summary>
        ///  List of UIDs associated with the entry.
        /// </summary>
        public List<long> UID = new List<long>();

        /// <summary>
        ///  Other data (other than the UIDs) associated with the entry.<br/>
        ///  The key is the column number starting from 0 where the data was taken from and will be written to.
        /// </summary>
        public Dictionary<int, string> Data = new Dictionary<int, string>();

        /// <summary>
        ///  Integer copy of data entries above (if available).<br/>
        ///  Only used for calculation purposes.
        /// </summary>
        public Dictionary<int, int> IntData = new Dictionary<int, int>();
    }
}
