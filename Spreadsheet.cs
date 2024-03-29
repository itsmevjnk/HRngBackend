﻿/*
 * Spreadsheet.cs - Class for storing spreadsheets.
 * Created on: 20:35 01-01-2022
 * Author    : itsmevjnk
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace HRngBackend
{
    public class Spreadsheet
    {
        /// <summary>
        ///  A dictionary of cells stored as strings.<br/>
        ///  The key is a row-column index tuple starting from 0.
        /// </summary>
        public Dictionary<(int row, int col), string> Data = new Dictionary<(int row, int col), string>();

        /// <summary>
        ///  The number of rows in this spreadsheet.<br/>
        ///  The spreadsheet is growing only; this means that the number of rows and columns will always be increasing and will never decrease (unless Shrink() is called, which can be a time-intensive operation).
        /// </summary>
        public int Rows = 0;

        /// <summary>
        ///  The number of columns in this spreadsheet.
        /// </summary>
        public int Columns = 0;

        /// <summary>
        ///  Convert an Excel-type cell address (e.g. B3) to a row-column index tuple (e.g. (2, 1)).<br/>
        ///  If the row or column is not specified in the address, the respective value in the tuple will be set to -1.
        /// </summary>
        /// <param name="addr">The Excel-type cell address string to be converted.</param>
        /// <returns>A tuple of row-column indexes.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="addr"/> argument is misformed.</exception>
        public (int row, int col) Index(string addr)
        {
            addr = addr.ToUpper(); // Normalize address to uppercase
            int row = 0, col = -1; // Return values
            bool p_row = false; // Set when parsing row number
            foreach(char c in addr)
            {
                if (c >= '0' && c <= '9')
                {
                    /* Row */
                    if (!p_row) p_row = true;
                    row = row * 10 + c - '0';
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    /* Column */
                    if (p_row) throw new ArgumentException($"Column letter {c} appears in row number");
                    if (col == -1) col = 0;
                    col = col * 26 + c - 'A';
                }
                else throw new ArgumentException($"Invalid character {c}");
            }
            return (row - 1, col);
        }

        /// <summary>
        ///  Convert a row-column index tuple (e.g. (2, 1)) to an Excel-type cell address (e.g. B3).<br/>
        ///  If the row or column field is set to -1, the respective coordinate won't appear in the returning address.
        /// </summary>
        /// <param name="tup">The row-column index tuple to be converted.</param>
        /// <returns>An Excel-type cell address.</returns>
        public string Address((int row, int col) tup)
        {
            string addr = "";
            if (tup.col != -1)
            {
                do
                {
                    addr += $"{(char)((tup.col % 26) + 'A')}";
                    tup.col /= 26;
                } while (tup.col > 0);
            }
            if (tup.row != -1) addr += Convert.ToString(tup.row + 1);
            return addr;
        }

        /// <summary>
        ///  Add or update a cell's value.<br/>
        ///  If the given cell value is empty, the cell will be removed (if it exists) or not be added.
        /// </summary>
        /// <param name="idx">The row-column index tuple pointing to the cell.</param>
        /// <param name="val">The cell's value.</param>
        public void Update((int row, int col) idx, string val)
        {
            bool add = true; // Set only when a new cell is being added
            if (Data.ContainsKey(idx))
            {
                add = false;
                Data.Remove(idx);
            }
            if (val != "") Data.Add(idx, val);
            if (add)
            {
                /* Cell added, check if we have to update rows/columns count */
                if (Rows < idx.row + 1) Rows = idx.row + 1;
                if (Columns < idx.col + 1) Columns = idx.col + 1;
            }
        }

        /// <summary>
        ///  Shrink the spreadsheet to fit the data in it.<br/>
        ///  This function can be time-intensive for large spreadsheets; therefore, it should be called as infrequently as possible (e.g. before writing).
        /// </summary>
        public void Shrink()
        {
            if (Data.Count == 0) Columns = Rows = 0;
            else
            {
                Rows = Data.Keys.Max(x => x.row) + 1;
                Columns = Data.Keys.Max(x => x.col) + 1;
            }
        }
    }
}
