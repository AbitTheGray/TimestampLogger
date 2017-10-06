# Timestamp Logger
**Timestamp Logger** is C# .NET/Mono Console Utility to prefix each line by timestamp.

## Arguments
Arguments are used from console to 

### Output File
**--output=...**
**--output-file=...**
Use this argument to save output to specified file. The file is overwritten if it exists.
Best for *last.log* file.
//TODO Support "" around file path to allow spaces

### Output Directory
**--output-dir=...**
**--output-directory=...**
Use for saving output to specified directory.
You can use date values:
    {0} = day (1 or 2 chars)
    {1} = month (1 or 2 chars)
    {2} = day of year (1 to 3 chars)
    {3} = year (4 chars)
    {4} = year (last 2 chars)
For formatting, look at C#'s [string.Format(...)](https://msdn.microsoft.com/en-us/library/system.string.format).
//TODO Support "" around directory path to allow spaces

### Format
**--format=...**
Format of prefix date/time. For values, look at [DateTime.ToString()](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).
Default: *hh:MM:ss*

### UTC Date
**-u**
**--utc**
**--utc-date**
To have all time in [UTC](https://en.wikipedia.org/wiki/Coordinated_Universal_Time).

### Brackets
**--brackets=...**
Defines type of brackets surrounding time
Supported values:
    round
    square (default)
    curly
    angle
    none

### Prefix Distance
**--prefix=...**
Define distance (offset) of inputted text from left.

### Line Break
**--line-break=...**
Allows you to change char(s) for end of line.
Supported values:
    r
    n
    rn
    nr

### No Print
**-n**
**--no-print**
Disables output to console.
Automatically disabled when input is not redirected.

## TODO List
[ ] Add support for "" to **--output-...**
[ ] Move **--brackets** to **--format** (will it work?)
[ ] Change reading from console to not print typed characters
