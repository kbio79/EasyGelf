EasyGelf [![Build status](https://ci.appveyor.com/api/projects/status/o7ni0ymhjhvcsn8u/branch/master?svg=true)](https://ci.appveyor.com/project/Pliner/easygelf/branch/master)
========
Goals: to support up to date version of Gelf and provide reliable integration with popular .Net logging libraries.

Now log4net and NLog are supported. Also Udp, Tcp and Amqp protocols are supported.

##Branch Changes
* `Added support for custom Additional Fields as Gelf4Net definition "app:applicationName,version:1.0.2,environment:TEST"
* `Changed "version" field for "message_version"
* `Configure EasyGelf by code example added

## Usage(log4net)

###Configure by Code

Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

PatternLayout patternLayout = new PatternLayout();
patternLayout.ConversionPattern = "%date [%thread] %-5level %logger [%.30location| Method = %method] - %message%newline";
patternLayout.ActivateOptions();

GelfTcpAppender remoteAppender = new GelfTcpAppender();
remoteAppender.Layout = patternLayout;
remoteAppender.AdditionalFields = string.Format("app:applicationName,version:{0},environment:{1}","1.0.2","TEST");
remoteAppender.Facility = "Facility";
remoteAppender.RemoteAddress = "put your log server address here";
remoteAppender.RemotePort = 12201;
remoteAppender.ActivateOptions();            
hierarchy.Root.AddAppender(remoteAppender);

hierarchy.Root.Level = log4net.Core.Level.All;
hierarchy.Configured = true;
```                                


##Additional configuration
###Common

* `includeSource` (default: `true`)
  * Whether the source of the log message should be included

* `hostName` (default: the machine name)
  * The host name of the machine generating the logs

* `facility` (default: `gelf`)
  * The application specific name

* `useRetry` (default: `true`)
  * Allow to retry send log message
  * `retryCount` (default: 5) 
	* Count of retry attemps 
  * `retryDelay` (default: 50ms)
	* Pause between retry attempts


* `includeStackTrace` (default: `true`)
  * Will include exception message and exception stack trace
	

