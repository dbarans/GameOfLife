# RLE Parser for Game of Life

C# script for parsing RLE (Run-Length Encoded) files used in Conway's Game of Life.

## Setup

1. Create new console project:
   ```cmd
   dotnet new console -n RLEParser
   cd RLEParser
   ```

2. Add required package:
   ```cmd
   dotnet add package Newtonsoft.Json
   ```

3. Replace `Program.cs` with `RLEParser.cs`

4. Run the parser:
   ```cmd
   dotnet run -- <path_to_folder_with_RLE_files> [output_json_file]
   ```

## How to run

```cmd
dotnet run -- <path_to_folder_with_RLE_files> [output_json_file]
```

## Usage examples

```cmd
# Example 1 - default output file
dotnet run -- C:\Patterns\RLE

# Example 2 - custom output file
dotnet run -- "C:\My Game of Life Patterns" "my_patterns.json"

# Example 3 - current directory
dotnet run -- .
```

## What the script does

1. Scans the specified folder for `.rle` files
2. Parses each RLE file and converts to JSON with:
   - Pattern name
   - Author
   - Description
   - Dimensions (width x height)
   - Original RLE data (as string)
3. Handles large patterns gracefully - stores metadata even if parsing fails
4. Saves all data to a single JSON file incrementally (memory efficient)

## JSON Output Format

```json
{
  "SourceFolder": "C:\\Patterns\\RLE",
  "TotalPatterns": 1000,
  "Patterns": [
    {
      "Name": "Glider",
      "Author": "John Conway",
      "Description": "A simple glider pattern",
      "Width": 3,
      "Height": 3,
      "FileName": "glider.rle",
      "RleData": "bob$2bo$3o!"
    }
  ]
}
```

## RLE File Format

The script supports standard RLE format used in Conway's Game of Life:
- `b` or `.` - empty cell
- `o` or `A` - live cell
- `$` - end of line
- `!` - end of pattern
- Numbers before symbols indicate repetitions

## Features

- **Memory efficient** - processes files incrementally
- **Error handling** - creates metadata entries for failed patterns
- **Original RLE data** - preserves exact RLE strings (not converted to 2D arrays)
- **Progress tracking** - shows parsing progress in console
- **Large file support** - handles thousands of patterns without memory issues

## Requirements

- .NET 8.0 or newer
- RLE files in standard format
- Newtonsoft.Json package (added during setup)


