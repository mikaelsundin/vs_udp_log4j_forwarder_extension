using Log4JForwardExtension.Data;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Log4JForwardExtension.Parsers;


/// <summary>
/// This class handles Android messages when running application via HotReload.
/// </summary>
internal class HotReloadAndroidParser : IParser
{

    /// <summary>
    /// Try to parse Android Hot Reload message.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool TryParse(string line, out LogMessage? result)
    {
        //Regex pattern
        var pattern = @"^(?<timestamp>\d{2}:\d{2}:\d{2}:\d{3})\s*(?:\[(?<logger>[^\]]+)\]\s+)?(?<message>.+)$";

        //Check if we got Hot Reload format
        var match = Regex.Match(line, pattern);
        if (!match.Success)
        {
            result = null;
            return false;
        }

        //Get the timestamp.
        DateTime timestamp = DateTime.ParseExact(match.Groups["timestamp"].Value, "HH:mm:ss:fff", CultureInfo.InvariantCulture);

        //Get loggername or default to -
        var logger = match.Groups["logger"].Value.Trim();
        if (string.IsNullOrEmpty(logger))
        {
            logger = "default";
        }

        //Get message, abort if empry
        var message = match.Groups["message"].Value.Trim();
        if (string.IsNullOrEmpty(message))
        {
            result = null;
            return false;
        }

        //fill in the information
        result = new LogMessage()
        {
            Timestamp = timestamp,
            LoggerName = $"Android.{logger}",
            Message = message,
        };
        return true;
    }
}

