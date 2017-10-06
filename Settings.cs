using System;
namespace TimestampLogger
{
    public class Settings
    {
        public Settings()
        {
        }

        public static Settings Default
        {
            get
            {
                return new Settings();
            }
        }

        #region Values

        public string OutputFile
        {
            get;
            set;
        } = null;

        public string OutputDirectory
        {
            get;
            set;
        } = null;

        public string DateFormat
        {
            get;
            set;
        } = "HH:mm:ss.f";

        public bool DateUTC
        {
            get;
            set;
        } = false;

        public enum BracketsType
        {
            None = 0,
            Round,
            Square,
            Curly,
            Angle
        }
        public BracketsType Brackets
        {
            get;
            set;
        } = BracketsType.Square;
        public byte PrefixDistance
        {
            get;
            set;
        } = 13;

        public enum LineBreakType
        {
            Original = 0,
            r,
            n,
            rn,
            nr
        }
        public LineBreakType LineBreak
        {
            get;
            set;
        } = LineBreakType.Original;

        public bool PrintToConsole
        {
            get;
            set;
        } = true;

        #endregion

        public int OutputCount
        {
            get
            {
                int count = 0;
                if (PrintToConsole)
                    count++;
                if (!string.IsNullOrEmpty(OutputFile))
                    count++;
                if (!string.IsNullOrEmpty(OutputDirectory))
                    count++;
                return count;
            }
        }

    }
}