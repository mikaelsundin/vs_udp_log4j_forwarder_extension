using Log4JForwardExtension.Data;
using Log4JForwardExtension.Parsers;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace Log4JForwardExtension;

/// <summary>
/// Handle sending Log4J UDP packets.
/// </summary>
internal class Log4JTransmitter
{

    string hostname = "127.0.0.1";
    int port = 7071;
    UdpClient udpClient = new();

    List<IParser> parsers = new();
    public static Log4JTransmitter Instance { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    private Log4JTransmitter()
    {

    }

    /// <summary>
    /// Send the Logmessage via UDP in Log4j format.
    /// </summary>
    /// <param name="log"></param>
    public void Send(LogMessage log)
    {
        string msg = string.Empty;
        long unixTimestamp = new DateTimeOffset(log.Timestamp).ToUnixTimeMilliseconds();
        var level = log.Level.ToString().ToUpper();

        //Build minimal Log4j message
        msg += $"<log4j:event logger=\"{log.LoggerName}\" level=\"{level}\" timestamp=\"{unixTimestamp}\" thread=\"{log.Thread}\">\n";
        msg += $"<log4j:message>{log.Message}</log4j:message>\n";
        msg += $"</log4j:event>";

        //Send UDP
        try
        {
            var payload = Encoding.UTF8.GetBytes(msg);
            udpClient.Send(payload, payload.Length, hostname, port);
        }catch (Exception ex)
        {
            Debug.WriteLine("Log4JTransmitter.Send {}", ex);
        }
    }

    /// <summary>
    /// Debug 
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private LogMessage CreateFallbackMessage(string line)
    {
        return new LogMessage()
        {
            Level = LogLevel.Debug,
            LoggerName = "VS",
            Thread = 0,
            Timestamp = DateTime.Now,
            Message = line
        };
    }

    /// <summary>
    /// Initilize the transmitter
    /// </summary>
    /// <param name="package"></param>
    public static void Initialize(AsyncPackage package)
    {
        Instance = new Log4JTransmitter();
    }

    /// <summary>
    /// Function to register a parser
    /// </summary>
    /// <param name="parser"></param>
    public void RegisterParser(IParser parser) => parsers.Add(parser);

    /// <summary>
    /// Process a DebugLine.
    /// </summary>
    /// <param name="line"></param>
    public void HandleDebugLine(string line)
    {
        //Go over all parsers
        foreach (var parser in parsers) {
            if(parser.TryParse(line, out var msg)){
                Send(msg ?? throw new NullReferenceException());
                return;
            }

        }

        //No parser found,
        var fallback  = CreateFallbackMessage(line);
        Send(fallback);
    }
}
