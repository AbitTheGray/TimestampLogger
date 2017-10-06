using System;
using System.IO;

namespace TimestampLogger
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            Settings settings = Settings.Default;
            if (ParseArguments(settings, args))
            {// Display Help
                using (var help_stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("TimestampLogger.help.txt"))
                {
                    using (var help_reader = new StreamReader(help_stream))
                    {
                        while (!help_reader.EndOfStream)
                            Console.WriteLine(help_reader.ReadLine());
                    }
                }

                return;
            }
            if (settings.PrefixDistance < settings.DateFormat.Length)
                settings.PrefixDistance = settings.DateFormat.Length > 255 ? (byte)255 : (byte)settings.DateFormat.Length;
            if (!Console.IsInputRedirected)
            {
                settings.PrintToConsole = false;
                Console.Error.WriteLine("Console input is not redirected.");
                Console.Error.WriteLine("Printing to Console is disabled to prevent double-typed chars.");
            }

            // TextWriter array init
            TextWriter[] writers = new TextWriter[settings.OutputCount];
            if (writers.Length == 0)
            {
                Console.Error.WriteLine("Nowhere to output.");
                return;
            }

            // Output to Console
            if (settings.PrintToConsole)
                AddWriter(writers, Console.Out);

            // Output File
            if (!string.IsNullOrEmpty(settings.OutputFile))
            {
                var writer = new StreamWriter(File.Open(settings.OutputFile, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    AutoFlush = true
                };
                AddWriter(writers, writer);
            }

            // Output Directory
            if (!string.IsNullOrEmpty(settings.OutputDirectory))
            {
                var now = settings.DateUTC ? DateTime.UtcNow : DateTime.Now;
                string path = string.Format(settings.OutputDirectory, now.Day, now.Month, now.DayOfYear, now.Year.ToString("D4"), (now.Year % 100).ToString("D2"));

                var writer = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    AutoFlush = true
                };
                AddWriter(writers, writer);
            }

            // Processing
            bool prevCharNewLine = true;
            int c;
            while ((c = Console.Read()) != -1)
            {
                if (settings.LineBreak != Settings.LineBreakType.Original) // New Line Tricks
                {
                    if (prevCharNewLine)
                    {
                        prevCharNewLine = false;
                        switch (settings.LineBreak)
                        {
                            case Settings.LineBreakType.r:
                                Write(writers, '\r');
                                break;
                            case Settings.LineBreakType.n:
                                Write(writers, '\n');
                                break;
                            case Settings.LineBreakType.rn:
                                Write(writers, '\r');
                                Write(writers, '\n');
                                break;
                            case Settings.LineBreakType.nr:
                                Write(writers, '\n');
                                Write(writers, '\r');
                                break;
                        }
                        WritePrefix(writers, settings);

                        if (c == '\n' || c == '\r')
                            continue;
                    }
                    else
                    {
                        if (c == '\n' || c == '\r')
                        {
                            prevCharNewLine = true;
                            continue;
                        }
                    }
                }
                else
                {
                    if (c == '\n' || c == '\r')
                        prevCharNewLine = true;
                    else if (prevCharNewLine)
                    {
                        WritePrefix(writers, settings);
                        prevCharNewLine = false;
                    }
                }

                Write(writers, (char)c);
            }

            // Dispose StreamWriters (files) but not other (Console) TextWriters
            for (int writer_i = 0; writer_i < writers.Length; writer_i++)
                if (writers[writer_i] is StreamWriter)
                    writers[writer_i].Dispose();
        }

        /// <summary>
        /// Add writer to first empty (<see cref="null"/>) item in array.
        /// </summary>
        /// <param name="writers">Array of writers into which we will be adding.</param>
        /// <param name="writer">Writer to add.</param>
        public static void AddWriter(TextWriter[] writers, TextWriter writer)
        {
            for (int i = 0; i < writers.Length; i++)
            {
                if (writers[i] == null)
                {
                    writers[i] = writer;
                    return;
                }
            }
        }

        public static void WritePrefix(TextWriter[] writers, Settings settings)
        {
            DateTime now = settings.DateUTC ? DateTime.UtcNow : DateTime.Now;
            string prefix = now.ToString(settings.DateFormat);
            switch (settings.Brackets)
            {
                default:
                case Settings.BracketsType.None:
                    break;
                case Settings.BracketsType.Round:
                    prefix = "(" + prefix + ")";
                    break;
                case Settings.BracketsType.Square:
                    prefix = "[" + prefix + "]";
                    break;
                case Settings.BracketsType.Curly:
                    prefix = "{" + prefix + "}";
                    break;
                case Settings.BracketsType.Angle:
                    prefix = "<" + prefix + ">";
                    break;
            }
            for (int char_i = 0; char_i < prefix.Length; char_i++)
                for (int i = 0; i < writers.Length; i++)
                    writers[i].Write(prefix[char_i]);

            for (int index_from_left = prefix.Length; index_from_left < settings.PrefixDistance; index_from_left++)
                for (int i = 0; i < writers.Length; i++)
                    writers[i].Write(' ');
        }

        public static void Write(TextWriter[] writers, char c)
        {
            for (int i = 0; i < writers.Length; i++)
                writers[i].Write(c);
        }

        public static bool ParseArguments(Settings settings, string[] arguments)
        {
            for (int arg_i = 0; arg_i < arguments.Length; arg_i++)
            {
                string arg = arguments[arg_i];
                if (string.IsNullOrEmpty(arg))
                    continue;
                if (arg[0] == '-')
                {
                    if (arg.Length < 2)
                    {
                        Console.Error.WriteLine("Unknown argument '{0}'. Too short.", arg);
                        continue;
                    }

                    if (arg[1] == '-') // --
                    {
                        if (arg.Length < 2)
                        {
                            Console.Error.WriteLine("Unknown argument '{0}'. Too short.", arg);
                            continue;
                        }

                        int equal_index = arg.IndexOf('=', 2);
                        string key = equal_index == -1 ? arg.Substring(2) : arg.Substring(2, equal_index - 2);
                        string val = equal_index == -1 || equal_index == arg.Length - 1 ? null : arg.Substring(equal_index + 1);

                        switch (key)
                        {
                            case "help":
                                return true;
                            case "output":
                            case "output-file":
                                settings.OutputFile = val;
                                break;
                            case "output-dir":
                            case "output-directory":
                                settings.OutputDirectory = val;
                                break;
                            case "format":
                                settings.DateFormat = val;
                                break;
                            case "utc":
                            case "utc-date":
                                settings.DateUTC = true;
                                break;
                            case "brackets":
                                {
                                    switch (val.ToLowerInvariant())
                                    {
                                        case "none":
                                        case "empty":
                                            settings.Brackets = Settings.BracketsType.None;
                                            break;
                                        case "round":
                                            settings.Brackets = Settings.BracketsType.Round;
                                            break;
                                        case "square":
                                            settings.Brackets = Settings.BracketsType.Square;
                                            break;
                                        case "curly":
                                            settings.Brackets = Settings.BracketsType.Curly;
                                            break;
                                        case "angle":
                                            settings.Brackets = Settings.BracketsType.Angle;
                                            break;
                                        default:
                                            Console.Error.WriteLine("Unknown brackets type '{0}'.", val);
                                            break;
                                    }
                                }
                                break;
                            case "prefix":
                                {
                                    if (byte.TryParse(val, out byte prefix_b))
                                        settings.PrefixDistance = prefix_b;
                                    else
                                        Console.Error.WriteLine("Unsupported prefix distance '{0}'. Cannot be >255.", val);
                                }
                                break;
                            case "line-break":
                                {
                                    switch (val.ToLowerInvariant())
                                    {
                                        case "r":
                                            settings.LineBreak = Settings.LineBreakType.r;
                                            break;
                                        case "n":
                                            settings.LineBreak = Settings.LineBreakType.n;
                                            break;
                                        case "rn":
                                            settings.LineBreak = Settings.LineBreakType.rn;
                                            break;
                                        case "nr":
                                            settings.LineBreak = Settings.LineBreakType.nr;
                                            break;
                                        default:
                                            Console.Error.WriteLine("Unknown line-break type '{0}'.", val);
                                            break;
                                    }
                                }
                                break;
                            case "no-print":
                                settings.PrintToConsole = false;
                                break;
                            default:
                                Console.Error.WriteLine("Unknown argument key '{0}'.", key);
                                break;
                        }
                    }
                    else // -
                    {
                        for (int arg_char_i = 1; arg_char_i < arg.Length; arg_char_i++)
                        {
                            char arg_char = arg[arg_char_i];
                            switch (arg_char)
                            {
                                case 'u':// utc-date
                                    settings.DateUTC = true;
                                    break;
                                case 'n':// no-print
                                    settings.PrintToConsole = false;
                                    break;
                                default:
                                    Console.Error.WriteLine("Unknown argument char '{0}'.", arg_char);
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    Console.Error.WriteLine("Unknown argument '{0}'. Missing minus char.", arg);
                    continue;
                }
            }
            return false;
        }
    }
}