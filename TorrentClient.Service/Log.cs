using System;
using System.Diagnostics;
using System.Threading;

namespace TorrentClient.Service
{
    public static class Log
    {
        static readonly EventLog log = new EventLog(logName: "TorrentClient", machineName: ".", source: "TorrentClient");

        static Log()
        {
            if (!EventLog.Exists("TorrentClient")) {
                EventLog.CreateEventSource("TorrentClient", "TorrentClient");
            }
        }

        private static void Write(string output, EventLogEntryType type)
        {
            Console.Write(DateTime.UtcNow.ToString("hh:mm:ss.fff") + "|" + Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(5, '0') + ": " + output);
            log.WriteEntry(DateTime.UtcNow.ToString("hh:mm:ss.fff") + "|" + Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(5, '0') + ": " + output, type);
        }

        public static void WriteLine(object output, EventLogEntryType type = EventLogEntryType.Information)
        {
            Write(output + "\n", type);
        }

        public static void WriteLine(object obj, string output, EventLogEntryType type = EventLogEntryType.Information)
        {
            Write(obj + " " + output + "\n", type);
        }

        public static void LogException(Exception exception)
        {
            WriteLine(exception.ToString(), EventLogEntryType.Error);
        }
    }
}