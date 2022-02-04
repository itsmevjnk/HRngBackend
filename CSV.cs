/*
 * CSV.cs - Functions for reading (parsing) and writing (generating)
 *          comma-separated values (CSV) files.
 * Created on: 18:32 01-01-2022
 * Author    : itsmevjnk
 */

using System;
using System.IO;
using System.Text;
using System.Collections;

namespace HRngBackend
{
    public static class CSV
    {
        /// <summary>
        ///  Parse a stream of CSV-formatted data.
        /// </summary>
        /// <param name="input">The CSV-formatted data stream.</param>
        /// <param name="encoding">The stream's encoding (optional). Defaults to UTF-8.</param>
        /// <param name="bom">Whether to detect encoding from byte order mark (BOM) (optional). Disabled by default.</param>
        /// <param name="delimiter">The delimiter between cells in a row (optional). Defaults to <c>,</c> (comma).</param>
        /// <param name="escape">The enclosing character used in cases such as multi-line cells. Defaults to <c>"</c> (double quote).</param>
        /// <param name="newline">The new line character(s) used in the CSV string. If not specified, any combination of <c>\r</c> and <c>\n</c> will be used.</param>
        /// <returns>A <c>Spreadsheet</c> instance generated from the input.</returns>
        public static Spreadsheet FromStream(Stream input, Encoding encoding = null, bool bom = false, char delimiter = ',', char escape = '"', string newline = "")
        {
            encoding = encoding ?? Encoding.UTF8;
            
            Spreadsheet output = new Spreadsheet();

            int row = 0, col = 0; // Processing cell's row and column index
            string val = ""; // Cell value
            bool escaped = false; // Set when the current cell is escaped
            Queue c_queue = new Queue(); // Character queue, used for seeking multiple bytes in advance
            using (StreamReader reader = new StreamReader(input, encoding, bom))
            {
                while (!reader.EndOfStream || c_queue.Count > 0)
                {
                    char c;
                    if (c_queue.Count > 0) c = (char)c_queue.Dequeue();
                    else c = (char)reader.Read();
                    if (c == -1) break; // End of stream
                    if ((newline.Length > 0 && c == newline[0]) ||
                        (newline.Length == 0 && (c == '\r' || c == '\n')))
                    {
                        /* Possible row end */
                        bool end = false; // Set if this is really row end
                        if (newline.Length == 0)
                        {
                            /* Any \r and \n combinations */
                            char c_next = (char)reader.Peek();
                            if ((c == '\r' && c_next == '\n') || (c == '\n' && c_next == '\r'))
                            {
                                /* \r\n or \n\r */
                                reader.Read();
                                end = true;
                            }
                            else end = true; // Single new line character, leave next character for next iteration
                        }
                        else
                        {
                            string seq = ""; // For temporarily storing the sequence if we need to return it to cache
                            end = true; // We'll use this to signal byte mismatch
                            for (int i = 1; i < newline.Length && end; i++)
                            {
                                char c_next = (char)reader.Read();
                                end = (c_next == newline[i]);
                                seq += $"{c_next}";
                            }
                            if (!end)
                            {
                                /* Byte mismatch, push all those characters we just read to queue */
                                foreach (char cs in seq) c_queue.Enqueue(cs);
                            }
                        }
                        if (end) c = '\n'; // Normalize new line to \n
                    }
                    if (c == escape)
                    {
                        escaped = !escaped; // Escape character, toggle escaped flag
                        if (!escaped && (char)reader.Peek() == escape)
                        {
                            /* Double escape character is used for putting escape character in cell value */
                            escaped = true; // Still escaping
                            val += $"{c}";
                            reader.Read(); // Advance to next character
                        }
                    }
                    else if (!escaped)
                    {
                        if (c == delimiter)
                        {
                            /* Cell end */
                            output.Update((row, col), val.Replace("\n", Environment.NewLine));
                            val = ""; col++;
                        }
                        else if (c == '\n')
                        {
                            output.Update((row, col), val.Replace("\n", Environment.NewLine));
                            val = ""; row++; col = 0;
                        }
                        else val += $"{c}";
                    }
                    else val += $"{c}";
                }
            }

            return output;
        }

        /// <summary>
        ///  Parse a string containing CSV-formatted data.
        /// </summary>
        /// <param name="input">The CSV-formatted data.</param>
        /// <param name="delimiter">The delimiter between cells in a row (optional). Defaults to <c>,</c> (comma).</param>
        /// <param name="escape">The enclosing character used in cases such as multi-line cells. Defaults to <c>"</c> (double quote).</param>
        /// <param name="newline">The new line character(s) used in the CSV string. If not specified, any combination of <c>\r</c> and <c>\n</c> will be used.</param>
        /// <returns>A <c>Spreadsheet</c> instance generated from the input.</returns>
        public static Spreadsheet FromString(string input, char delimiter = ',', char escape = '"', string newline = "")
        {
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                return FromStream(stream, delimiter: delimiter, escape: escape, newline: newline);
            }
        }

        /// <summary>
        ///  Parse a CSV file.
        /// </summary>
        /// <param name="fname">The path (absolute or relative to the backend) to the CSV file.</param>
        /// <param name="encoding">The file's encoding (optional). Defaults to UTF-8.</param>
        /// <param name="bom">Whether the encoding is detected using the file's byte order mark (BOM) (optional). Defaults to enabled.</param>
        /// <param name="delimiter">The delimiter between cells in a row (optional). Defaults to <c>,</c> (comma).</param>
        /// <param name="escape">The enclosing character used in cases such as multi-line cells. Defaults to <c>"</c> (double quote).</param>
        /// <param name="newline">The new line character(s) used in the CSV string. If not specified, any combination of <c>\r</c> and <c>\n</c> will be used.</param>
        /// <returns>A <c>Spreadsheet</c> instance generated from the input.</returns>
        public static Spreadsheet FromFile(string fname, Encoding encoding = null, bool bom = true, char delimiter = ',', char escape = '"', string newline = "")
        {
            encoding = encoding ?? Encoding.UTF8;
            using (FileStream stream = File.Open(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return FromStream(stream, encoding, bom, delimiter, escape, newline);
            }
        }

        /// <summary>
        ///  Writes the spreadsheet to a CSV-formatted stream.
        /// </summary>
        /// <param name="sheet">The Spreadsheet object to be written.</param>
        /// <param name="stream">The destination stream.</param>
        /// <param name="encoding">The stream's encoding. Set to UTF-8 by default.</param>
        /// <param name="delimiter">The delimiter between cells in a row (optional). Defaults to <c>,</c> (comma).</param>
        /// <param name="escape">The enclosing character used in cases such as multi-line cells. Defaults to <c>"</c> (double quote).</param>
        /// <param name="sheet_nl">The new line character(s) used in the spreadsheet (optional). Defaults to <c>Environment.NewLine</c>.</param>
        /// <param name="newline">The new line character(s) used in the CSV string (optional). Defaults to <c>\r\n</c>.</param>
        /// <returns>A string of CSV-formatted data.</returns>
        public static void ToStream(Spreadsheet sheet, Stream stream, Encoding encoding = null, char delimiter = ',', char escape = '"', string sheet_nl = null, string newline = "\r\n")
        {
            using(StreamWriter writer = new StreamWriter(stream, encoding ?? Encoding.UTF8))
            {
                sheet_nl = sheet_nl ?? Environment.NewLine;
                bool nl_replace = (sheet_nl != newline); // Set if new line character replacement is needed
                string cell = ""; // Cell value

                /* Preprocessed for optimized memory usage (i.e. less GC) */
                string escape_str = $"{escape}";
                string escape_escape_str = $"{escape}{escape}";

                for (int row = 0; row < sheet.Rows; row++)
                {
                    for (int col = 0; col < sheet.Columns; col++)
                    {
                        if (sheet.Data.ContainsKey((row, col)))
                        {
                            cell = sheet.Data[(row, col)];
                            if (nl_replace) cell = cell.Replace(sheet_nl, newline);
                            if (cell.Contains(escape) || cell.Contains(delimiter) || cell.Contains(newline))
                            {
                                cell = escape_str + cell.Replace(escape_str, escape_escape_str) + escape_str; // Escape cell value
                            }
                        }
                        else cell = "";
                        writer.Write(cell);
                        if (col != sheet.Columns - 1) writer.Write(delimiter);
                    }
                    writer.Write(newline);
                }
            }
        }

        /// <summary>
        ///  Writes the spreadsheet to a CSV-formatted string.
        /// </summary>
        /// <param name="sheet">The Spreadsheet object to be written.</param>
        /// <param name="delimiter">The delimiter between cells in a row (optional). Defaults to <c>,</c> (comma).</param>
        /// <param name="escape">The enclosing character used in cases such as multi-line cells. Defaults to <c>"</c> (double quote).</param>
        /// <param name="sheet_nl">The new line character(s) used in the spreadsheet (optional). Defaults to <c>Environment.NewLine</c>.</param>
        /// <param name="newline">The new line character(s) used in the CSV string (optional). Defaults to <c>\r\n</c>.</param>
        /// <returns>A string of CSV-formatted data.</returns>
        public static string ToString(Spreadsheet sheet, char delimiter = ',', char escape = '"', string sheet_nl = null, string newline = "\r\n")
        {
            using(MemoryStream stream = new MemoryStream())
            {
                ToStream(sheet, stream, delimiter: delimiter, escape: escape, sheet_nl: sheet_nl, newline: newline);
                return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }

        /// <summary>
        ///  Writes the spreadsheet to a CSV file.
        /// </summary>
        /// <param name="fname">The path (absolute or relative to the backend) to the CSV file.</param>
        /// <param name="encoding">The encoding to be used in the file. Defaults to UTF-8.</param>
        /// <param name="delimiter">The delimiter between cells in a row (optional). Defaults to <c>,</c> (comma).</param>
        /// <param name="escape">The enclosing character used in cases such as multi-line cells. Defaults to <c>"</c> (double quote).</param>
        /// <param name="sheet_nl">The new line character(s) used in the spreadsheet (optional). Defaults to <c>Environment.NewLine</c>.</param>
        /// <param name="newline">The new line character(s) used in the CSV string (optional). Defaults to <c>\r\n</c>.</param>
        public static void ToFile(Spreadsheet sheet, string fname, Encoding encoding = null, char delimiter = ',', char escape = '"', string sheet_nl = null, string newline = "\r\n")
        {
            using(FileStream stream = File.Create(fname))
            {
                ToStream(sheet, stream, encoding, delimiter, escape, sheet_nl, newline);
            }
        }
    }
}
