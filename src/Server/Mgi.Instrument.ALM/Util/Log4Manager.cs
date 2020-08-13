using System;
using System.Reflection;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Log4Net.Async;

namespace Mgi.Instrument.ALM.Util
{
    public static class Log4Manager
    {
        private static Level logLevel = Level.Info;
        public static ILog GetLogger(Type type)
        {
            return GetLogger(type.Name);
        }
        public static ILog GetLogger(string name)
        {
            if (LogManager.Exists(name) == null)
            {
                var appname = Assembly.GetEntryAssembly().GetName().Name;
                // Pattern Layout defined
                var patternLayout = new PatternLayout
                {
                    ConversionPattern = "%date\t[%thread]\t%-5level\t%logger - %message%newline"
                };
                patternLayout.ActivateOptions();


                // configurating the RollingFileAppender object
                var fileAppender = new RollingFileAppender
                {
                    Name = name,
                    AppendToFile = true,
                    File = $"Log\\",
                    StaticLogFileName = false,

                    DatePattern = $"\"{appname}-{name}-\"yyyyMMdd\".log\"",
                    LockingModel = new FileAppender.MinimalLock(),
                    Layout = patternLayout,
                    MaxSizeRollBackups = 512,
                    MaximumFileSize = "16MB",
                    RollingStyle = RollingFileAppender.RollingMode.Composite,
                };
                fileAppender.ActivateOptions();

                //var consoleAppender = new ManagedColoredConsoleAppender()
                //{
                //    Name = name,
                //    Layout = patternLayout,
                //    Threshold = Level.All,
                //};
                //consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors() { Level = Level.Error, ForeColor = System.ConsoleColor.Red });
                //consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors() { Level = Level.Warn, ForeColor = System.ConsoleColor.Yellow });
                //consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors() { Level = Level.Info, ForeColor = System.ConsoleColor.White });
                //consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors() { Level = Level.Debug, ForeColor = System.ConsoleColor.Green });
                //consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors() { Level = Level.Fatal, ForeColor = System.ConsoleColor.White, BackColor = System.ConsoleColor.Red });
                //consoleAppender.ActivateOptions();
                //var debugAppender = new DebugAppender()
                //{
                //    Category = patternLayout,
                //    Name = name,
                //    Layout = patternLayout,
                //    Threshold = Level.All
                //};
                //debugAppender.ActivateOptions();

                var p = new AsyncForwardingAppender();
                p.AddAppender(fileAppender);
                //p.AddAppender(debugAppender);
                //p.AddAppender(consoleAppender);
                p.ActivateOptions();
                var hierarchy = (Hierarchy)LogManager.GetRepository();

                var loger = hierarchy.GetLogger(name, hierarchy.LoggerFactory);
                loger.Hierarchy = hierarchy;
                loger.AddAppender(p);
                //loger.AddAppender(consoleAppender);
                //loger.AddAppender(debugAppender);
                loger.Level = logLevel;
                BasicConfigurator.Configure();
            }
            var log = LogManager.GetLogger(name);
            return log;
        }
        public static void SetMinLogLevel(Level level, string name = "")
        {
            if (string.IsNullOrEmpty(name))
            {
                var hierarchy = (Hierarchy)LogManager.GetRepository();
                hierarchy.Threshold = level;
            }
            else
            {
                var hierarchy = (Hierarchy)LogManager.GetRepository();
                if (hierarchy.Exists(name) is log4net.Repository.Hierarchy.Logger loger)
                {
                    loger.Level = level;
                }
            }
        }

        public static void SetLogLevelAll(string level)
        {
            switch (level.ToUpper())
            {
                case "DEBUG":
                    logLevel = Level.Debug;
                    break;
                case "INFO":
                    logLevel = Level.Info;
                    break;
                case "WARN":
                    logLevel = Level.Warn;
                    break;
                case "ERROR":
                    logLevel = Level.Error;
                    break;
                case "FATAL":
                    logLevel = Level.Fatal;
                    break;
                default:
                    break;
            }
        }
    }
}
