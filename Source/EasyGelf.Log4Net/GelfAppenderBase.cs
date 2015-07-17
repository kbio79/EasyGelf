using System;
using System.Collections.Generic;
using EasyGelf.Core;
using EasyGelf.Core.Transports;
using log4net.Appender;
using log4net.Core;

namespace EasyGelf.Log4Net
{
    public abstract class GelfAppenderBase : AppenderSkeleton
    {
        private ITransport transport;
        private IEasyGelfLogger logger;
        private Dictionary<string, string> customParams;

        public string Facility { get; set; }

        public bool IncludeSource { get; set; }

        public string HostName { get; set; }

        public bool UseRetry { get; set; }

        public int RetryCount { get; set; }

        public TimeSpan RetryDelay { get; set; }

        public bool IncludeStackTrace { get; set; }

        public bool Verbose { get; set; }

        public string AdditionalFields { get; set; }

        protected GelfAppenderBase()
        {
            Facility = "gelf";
            IncludeSource = true;
            Verbose = false;
            HostName = Environment.MachineName;
            UseRetry = true;
            RetryCount = 5;
            RetryDelay = TimeSpan.FromMilliseconds(50);
            IncludeStackTrace = true;
            AdditionalFields = null;            
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            try
            {
                logger = Verbose ? (IEasyGelfLogger)new VerboseLogger() : new SilentLogger();
                var mainTransport = InitializeTransport(logger);
                transport = new BufferedTransport(logger, UseRetry ? new RetryingTransport(logger, mainTransport, RetryCount, RetryDelay) : mainTransport);
                customParams = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(AdditionalFields))
                    ParseAdditionalFields(AdditionalFields);                
            }
            catch (Exception exception)
            {
                logger.Error("Failed to create Transport", exception);
            }
        }

        protected override bool RequiresLayout
        {
            get { return true; }
        }

        protected abstract ITransport InitializeTransport(IEasyGelfLogger logger);

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                var renderedEvent = RenderLoggingEvent(loggingEvent);
                var messageBuilder = new GelfMessageBuilder(renderedEvent, HostName, loggingEvent.TimeStamp, loggingEvent.Level.ToGelf());

                messageBuilder.SetAdditionalField(GelfAdditionalFields.Facility, Facility);                

                if (customParams != null && customParams.Count > 0)
                {
                    foreach (var item in customParams)
                    {
                        messageBuilder.SetAdditionalField(item.Key, item.Value);
                    }
                }
                else
                {
                    messageBuilder.SetAdditionalField(GelfAdditionalFields.LoggerName, loggingEvent.LoggerName)
                    .SetAdditionalField(GelfAdditionalFields.ThreadName, loggingEvent.ThreadName);
                }

                if (IncludeSource)
                {
                    var locationInformation = loggingEvent.LocationInformation;
                    if (locationInformation != null)
                    {
                        messageBuilder.SetAdditionalField(GelfAdditionalFields.SourceFileName, locationInformation.FileName)
                            .SetAdditionalField(GelfAdditionalFields.SourceClassName, locationInformation.ClassName)
                            .SetAdditionalField(GelfAdditionalFields.SourceMethodName, locationInformation.MethodName)
                            .SetAdditionalField(GelfAdditionalFields.SourceLineNumber, locationInformation.LineNumber);
                    }
                }
                if (IncludeStackTrace)
                {
                    var exception = loggingEvent.ExceptionObject;
                    if (exception != null)
                    {
                        messageBuilder.SetAdditionalField(GelfAdditionalFields.ExceptionMessage, exception.Message);
                        messageBuilder.SetAdditionalField(GelfAdditionalFields.ExceptionStackTrace, exception.StackTrace);
                    }
                }
                transport.Send(messageBuilder.ToMessage());
            }
            catch (Exception exception)
            {
                logger.Error("Unable to send logging event to remote host", exception);
            }
        }

        protected override void OnClose()
        {
            base.OnClose();
            if (transport == null)
                return;
            transport.Close();
            transport = null;
        }

        private void ParseAdditionalFields(string additionalParams)
        {
            //Gelf4Net additional fields format definition
            //app:RandomSentence,version:1.0,Level:%level

            string[] parsedParams = additionalParams.Split(',');

            foreach (var customField in parsedParams)
            {
                string[] param = customField.Split(':');

                if (param.Length == 2)
                    customParams.Add(param[0], param[1]);
            }
        }
    }
}