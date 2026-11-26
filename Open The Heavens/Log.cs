using BepInEx.Logging;
using System.Runtime.CompilerServices;
using System.IO;
using RoR2;

namespace Open_The_Heavens
{
    internal static class Log
    {
        private static ManualLogSource _logSource;

        internal enum LogTypes
        {
            Debug,
            Info,
            Warning,
            Error,
            Fatal
        }

        internal static string[] chatStyles =
        {
            "cEvent", "cIsUtility","cShrine","cIsHealth","cIsVoid"
        };

        internal static void Init(ManualLogSource logSource)
        {
            _logSource = logSource;
        }

        private static string Format(object data, string file, int line)
        {
            string fileName = System.IO.Path.GetFileName(file);
            return $"[{fileName}:{line}] {data}";
        }

        internal static void Debug(
            object data,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
            => _logSource.LogDebug(Format(data, file, line));

        internal static void Error(
            object data,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
            => _logSource.LogError(Format(data, file, line));

        internal static void Fatal(
            object data,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
            => _logSource.LogFatal(Format(data, file, line));

        internal static void Info(
            object data,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
            => _logSource.LogInfo(Format(data, file, line));

        internal static void Message(
            object data,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
            => _logSource.LogMessage(Format(data, file, line));

        internal static void Warning(
            object data,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
            => _logSource.LogWarning(Format(data, file, line));

        internal static void LogChat(Log.LogTypes type, string token)
        {
            string style = chatStyles[(int)type];
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = "<style=" + style + ">" + token + "</style>" });
        }
    }
}