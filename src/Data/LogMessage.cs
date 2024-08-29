using System;

namespace Log4JForwardExtension.Data;

internal record struct LogMessage
{
    public LogMessage()
    {

    }

    public DateTime Timestamp { get; set; } = DateTime.Now;
    public LogLevel Level { get; set; } = LogLevel.Info;
    public string LoggerName { get; set; } = "-";
    public int Thread { get; set; } = -1;
    public string Message { get; set; } = string.Empty;

}
