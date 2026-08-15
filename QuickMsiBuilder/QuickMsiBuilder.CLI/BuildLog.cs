using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace QuickMsiBuilder.CLI
{
    /// <summary>
    /// NLog set up in code rather than through NLog.config, so the logger keeps working wherever the
    /// executable is copied to (it is deployed next to the add-in, not installed).
    /// </summary>
    public static class BuildLog
    {
        private static readonly object Gate = new object();
        private static Logger _logger;

        public static string LogDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "RevitAddinManager", "QuickMsiBuilder", "logs");
            }
        }

        public static string LogFilePath
        {
            get { return Path.Combine(LogDirectory, "quickmsibuilder.log"); }
        }

        public static Logger Logger
        {
            get
            {
                if (_logger != null) return _logger;
                lock (Gate)
                {
                    if (_logger == null) _logger = Configure();
                }

                return _logger;
            }
        }

        private static Logger Configure()
        {
            try
            {
                var config = new LoggingConfiguration();

                var file = new FileTarget("file")
                {
                    FileName = LogFilePath,
                    Layout = "${longdate}|${level:uppercase=true}|${message}${onexception:|${exception:format=tostring}}",
                    ArchiveAboveSize = 1024 * 1024,
                    MaxArchiveFiles = 5,
                    KeepFileOpen = false,
                    Encoding = System.Text.Encoding.UTF8
                };

                // The UI reads stdout to show the build result, so console output stays plain.
                var console = new ConsoleTarget("console") { Layout = "${message}" };

                config.AddRule(LogLevel.Debug, LogLevel.Fatal, file);
                config.AddRule(LogLevel.Info, LogLevel.Fatal, console);
                LogManager.Configuration = config;
            }
            catch
            {
                // Logging must never be the reason a build fails.
            }

            return LogManager.GetLogger("QuickMsiBuilder");
        }

        public static void Shutdown()
        {
            try
            {
                LogManager.Shutdown();
            }
            catch
            {
                // ignored
            }
        }
    }
}
