using System;

namespace Clouddns
{
    public enum LogType
    {
        None,
        Info,
        Success,
        Warning,
        Error
    }

    internal class Logs
    {
        public static void Write(string szMessage, LogType eType = LogType.Info, bool bShowTime = true, bool bShowName = true)
        {
            if (!string.IsNullOrWhiteSpace(szMessage))
            {
                if (bShowTime) WriteColorTag(DateTime.Now.ToString("HH:mm:ss"), ConsoleColor.Green);
                if (bShowName) WriteColorTag("PROGRAM", ConsoleColor.Blue);

                switch (eType)
                {
                    case LogType.Info:
                        WriteColorTag("INFO", ConsoleColor.DarkCyan);
                        break;
                    case LogType.Success:
                        WriteColorTag("SUCCESS", ConsoleColor.DarkGreen);
                        break;
                    case LogType.Warning:
                        WriteColorTag("WARNING", ConsoleColor.DarkYellow);
                        break;
                    case LogType.Error:
                        WriteColorTag("ERROR", ConsoleColor.DarkRed);
                        break;
                }
                
                if (eType != LogType.None) Console.Write("-> ");
                Console.WriteLine(szMessage);
            }
        }

        static void WriteColorTag(string szText, ConsoleColor ccColor)
        {
            Console.ResetColor();
            Console.Write("[");
            Console.ForegroundColor = ccColor;
            Console.Write(szText);
            Console.ResetColor();
            Console.Write("] ");
            Console.ResetColor();
        }
    }
}