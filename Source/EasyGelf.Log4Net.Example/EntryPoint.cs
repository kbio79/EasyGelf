using System;
using System.IO;
using System.Threading;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace EasyGelf.Log4Net.Example
{
    public static class EntryPoint
    {
        private static readonly ILog Log = LogManager.GetLogger("ExampleLog");

        public static void Main()
        {
            ConfigureLoggingByCode();
            var cancelationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) => cancelationTokenSource.Cancel();
            while (!cancelationTokenSource.IsCancellationRequested)
            {
                Log.Info("I'm alive");
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
        }

        private static void ConfigureLogging()
        {
            var fileInfo = new FileInfo("log4net.config");
            if (!fileInfo.Exists)
                throw new Exception();
            XmlConfigurator.Configure(fileInfo);
        }

        private static void ConfigureLoggingByCode()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger [%.30location| Method = %method] - %message%newline";
            patternLayout.ActivateOptions();

            ColoredConsoleAppender c = new ColoredConsoleAppender();
            ColoredConsoleAppender.LevelColors col = new ColoredConsoleAppender.LevelColors();
            col.Level = log4net.Core.Level.Error;
            col.ForeColor = ColoredConsoleAppender.Colors.White;
            col.BackColor = ColoredConsoleAppender.Colors.Red;
            
            c.Layout = patternLayout;
            c.AddMapping(col);
            c.ActivateOptions();
            hierarchy.Root.AddAppender(c);
            
            GelfTcpAppender remoteAppender = new GelfTcpAppender();
            remoteAppender.Layout = patternLayout;
            remoteAppender.AdditionalFields = "app:AllInOne,version:0.0.6.6,environment:STAGE";
            remoteAppender.Facility = "AllInOne";
            remoteAppender.RemoteAddress = "logs.expertus.local";
            remoteAppender.RemotePort = 12201;
            remoteAppender.ActivateOptions();            
            hierarchy.Root.AddAppender(remoteAppender);

            hierarchy.Root.Level = log4net.Core.Level.All;
            hierarchy.Configured = true;
        }
    }
}
