using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Timers;

namespace loki_bms_common
{
    public class Logger : IDisposable
    {
        #region Enums
        public enum Severity
        {
            Log,
            Warning,
            Error,
            Exception,
            FATAL
        }

        public enum LogLevel
        {
            Normal,
            Debug,
            Verbose,
        }
        #endregion

        #region Fields and Properties
        /// <summary>
        /// The folder containing all log files; defaults to "%localappdata%\CASWebDir\Logs\"
        /// </summary>
        public string LogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CASWebDir", "Logs");
        public string OldLogsDir => Path.Combine(LogDirectory, "OldLogs");

        public string LogName = "NewLog";
        private bool disposedValue;

        public string Extension => "log";
        public string FullPath => Path.Combine(LogDirectory, $"{LogName}.{Extension}");
        public DateTime CreationTime = DateTime.Now;

        public LogLevel MaxLevel = LogLevel.Normal;
        /// <summary>
        /// Should the logger flush to file immediately upon logging a message? If false, waits to log and does it periodically.
        /// </summary>
        public bool FlushOnLog = false;
        /// <summary>
        /// Time, in seconds, that the logger will wait to flush to file before flushing again.
        /// </summary>
        public float FlushPeriodicity = 5;
        private System.Timers.Timer? FlushTimer;
        private DateTime LastFlushFinished;
        #endregion

        #region IO Components
        internal FileStream LogStream { get; private set; }
        internal TextWriter Writer { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the Logger class.
        /// </summary>
        /// <param name="LogName">The name of the log file, preceding its extension</param>
        /// <param name="KeepLogOpen">Whether to keep the log "open" when this logger is disposed. Defaults to FALSE.</param>
        /// <param name="UseExistingFile">Whether to use a still-open log file. Defaults to FALSE. Will automatically close any still-open logs if false.</param>
        public Logger(string LogName, bool FlushOnLog = false)
        {
            this.LogName = LogName;
            this.FlushOnLog = FlushOnLog;

            DirectoryInfo logdir = new DirectoryInfo(LogDirectory);
            if (!logdir.Exists)
            {
                logdir.Create();
                logdir.CreateSubdirectory("OldLogs");
            }

            LogStream = File.Open(FullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            
            Writer = new StreamWriter(LogStream, System.Text.Encoding.UTF8);
            Writer.WriteLine($"---- Log opened at {CreationTime:yyyy-MM-dd.HH:mm} ----");

            TryFlush();
            if(!FlushOnLog)
            {
                FlushTimer = new System.Timers.Timer(FlushPeriodicity * 1000);

                FlushTimer.BeginInit();
                FlushTimer.AutoReset = true;
                FlushTimer.Elapsed += FlushTimer_Elapsed;
                FlushTimer.EndInit();
                FlushTimer.Start();
            }
            
        }

        /// <summary>
        /// Creates an annotation in the log, marked by four hypens before and after the <paramref name="message">message</paramref>. Useful for marking new sections or major events in the log.
        /// </summary>
        /// <param name="message">The message to be printed within the annotation</param>
        /// <param name="includeTime">Should this annotation include a timestamp?</param>
        /// <example>Output: "---- message at 2023-10-13.12:32:30.123 ----"</example>
        public void WriteAnnotation (string message, bool includeTime)
        {
            Writer.WriteLine($"---- {message}{(includeTime ? " at " + string.Format("yyyy-MM-dd.HH:mm:ss.fff", DateTime.Now) : "")} ----");
            TryFlush();
        }

        /// <summary>
        /// Logs a single-line <paramref name="message"/> with specified <paramref name="severity"/>
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="severity">The severity of this message (default "Log" i.e. low severity)</param>
        public void LogMessage(string message, LogLevel depth, Severity severity = Severity.Log)
        {
            if (depth > MaxLevel) return;
            Writer.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{severity}]".PadRight(27) + $": {message}");

            TryFlush();
        }

        public void LogMessageMultiline(LogLevel depth, params string[] lines) => LogMessageMultiline(depth, Severity.Log, lines);

        /// <summary>
        /// Logs a multi-line message with specified severity and nice indents for readability
        /// </summary>
        /// <param name="severity">The severity of this message (default "Log" i.e. low severity)</param>
        /// <param name="lines">The lines to write; do not include newlines or tabs for formatting</param>
        public void LogMessageMultiline(LogLevel depth, Severity severity, params string[] lines)
        {
            if (lines.Length < 1 || depth > MaxLevel) return;

            bool AppendPrefix = true;

            foreach (string line in lines)
            {
                if (AppendPrefix)
                {
                    Writer.WriteLine($"[{DateTime.Now:HH:mm:ss:fff}] [{Severity.Exception}]".PadRight(27) + $": {line}");
                    AppendPrefix = false;
                }
                else Writer.WriteLine($"                    {line}");
            }

            TryFlush();
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message"></param>
        public void LogWarning(string message, LogLevel depth) => LogMessage(message, depth, Severity.Warning);
        /// <summary>
        /// Logs a generic error message
        /// </summary>
        /// <param name="message"></param>
        public void LogError(string message, LogLevel depth) => LogMessage(message, depth, Severity.Error);
        /// <summary>
        /// Logs an exception with nice formatting for the details of the exception.
        /// </summary>
        /// <param name="exception"></param>
        public void LogException(Exception exception, LogLevel depth)
        {
            string type = exception.GetType().Name;
            string message = exception.Message;
            string source = exception.Source ?? "null";
            string stacktrace = exception.StackTrace ?? "null";
            string inner = exception.InnerException?.ToString() ?? "no inner exceptions";

            LogMessageMultiline(
                depth, Severity.Exception,
                type,
                message,
                $"at {source}",
                stacktrace,
                inner);
        }
        /// <summary>
        /// Logs a fatal exception that may crash or have crashed the application. This will rarely get used, I imagine.
        /// </summary>
        /// <param name="exception"></param>
        public void LogFatal(Exception exception, LogLevel depth)
        {
            string type = exception.GetType().Name;
            string message = exception.Message;
            string source = exception.Source ?? "null";
            string stacktrace = exception.StackTrace ?? "null";
            string inner = exception.InnerException?.ToString() ?? "no inner exceptions";

            LogMessageMultiline(
                depth, Severity.FATAL,
                type,
                message,
                $"at {source}",
                stacktrace,
                inner,
                "This error was fatal! The application could not recover and is now closing.");
        }

        /// <summary>
        /// Closes an already-open log file and moves it to the Old Logs directory, changing its name to reflect open and close times.
        /// </summary>
        public void CloseLog()
        {
            DateTime CloseTime = DateTime.Now;

            Writer?.WriteLine($"---- Log closed at {CloseTime:yyyy-MM-dd.HH:mm} ----");
            Writer?.Dispose();

            LogStream?.Flush();
            LogStream?.Dispose();

            File.Move(FullPath, Path.Combine(OldLogsDir,
                $"{LogName}.{File.GetCreationTime(FullPath):yyyy-MM-dd-HHmm}_thru_{CloseTime:yyyy-MM-dd-HHmm}.{Extension}"));
        }

        protected void TryFlush()
        {
            if (FlushOnLog) FlushImmediate();
        }

        private void FlushTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            FlushImmediate();
        }

        private void FlushImmediate()
        {
            Writer.Flush();
            LastFlushFinished = DateTime.Now;
        }

        #region Disposing
        /// <summary>
        /// Disposes of this Logger object, closing the log file and disposing of any streams and writers.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CloseLog();
                    FlushTimer?.Stop();
                    FlushTimer?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
    #endregion
}