# HRngBackend
The backend component of HRng.
Note that this does not contain any functions for Facebook operations; an operations module such as **HRngSelenium** is needed.

## Installation
Build this project using Visual Studio or `dotnet` (refer to the solution's `README.md`).

## Usage
The backend by itself is not executable; a frontend is needed. See the **LibTests** project for more details.

## Features
As of now, the HRng backend contains resources for:

**Internal (helper) features:**
* Common HTTP client (`CommonHTTP.cs`)
* User-Agent string generation (`UserAgent.cs`)
* Standardized common and platform-specific base directories (`BaseDir.cs`)
* Operating system - architecture combo string generation (`OSCombo.cs`)
* Interface for Facebook posts (`IFBPost.cs`)
* Structures for Facebook comments (`FBComment.cs`) and reactions (`FBReact.cs`)

**Frontend-facing features:**
* Parsing and loading cookies to HTTP client and Selenium (`Cookies.cs`)
* Reading and writing CSV files (`CSV.cs`)
* Reading Excel workbooks (`ExcelWorkbook.cs`)
* Spreadsheet manipulation (`Spreadsheet.cs`)
* Facebook user ID retrieval (`UID.cs`)
* Input/output person entry manipulation (`Entry.cs`, `EntryCollection.cs`)

These feature(s) are planned to be added:
* Facebook post poll scraping
* Reading/writing XLSX (and possibly XLS and other data container/spreadsheet formats)
* Facebook group (both posts groups and chat groups) members list retrieval

## Contributing
Pull requests are welcome.
For major changes, please open an issue first to discuss what you'd like to change.

Any feature change requires **LibTests** to be updated accordingly.