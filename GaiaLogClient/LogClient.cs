using System;
using System.Linq;
using System.Threading;
using StackExchange.Redis;

namespace Gaia.LogService
{
    /// <summary>
    /// Client for log service, providing log recording functions.
    /// If this client can not connect to the Redis server,
    /// it will use a local log file instead.
    /// </summary>
    public class LogClient
    {
        /// <summary>
        /// The subscriber to the Redis server to use the log service.
        /// </summary>
        private readonly ISubscriber Subscriber = null;
        /// <summary>
        /// If subscriber failed to connect to the Redis server, this local file log will be used instead.
        /// </summary>
        private readonly LogRecorder Logger = null;

        /// <summary>
        /// Whether print logs to the console or not.
        /// If it is true, logs will be sent to the log server and then printed to the local console.
        /// </summary>
        public bool PrintToConsole = false;

        /// <summary>
        /// The author of the logs sent from this client.
        /// </summary>
        public string Author = "Anonymous";
        
        /// <summary>
        /// Construct and try to connect to the Redis server.
        /// </summary>
        /// <param name="port">Port of the Redis server.</param>
        /// <param name="ip">IP address of the Redis server.</param>
        public LogClient(uint port = 6379, string ip = "127.0.0.1") : 
            this(ConnectionMultiplexer.Connect($"{ip}:{port.ToString()}"))
        { }

        /// <summary>
        /// Reuse the connection to a Redis server.
        /// </summary>
        /// <param name="connection">Connection to the Redis server.</param>
        public LogClient(IConnectionMultiplexer connection)
        {
            Subscriber = connection.GetSubscriber();
            if (Subscriber.Publish(
                "logs/record",
                LogRecorder.GenerateLogText(
                    "Log service client connected.",
                    LogRecorder.Severity.Message, Author)) < 1)
            {
                Logger = new LogRecorder {PrintToConsole = false};
                Subscriber = null;
            }
        }

        /// <summary>
        /// Use a local log file.
        /// </summary>
        public LogClient()
        {
            Logger = new LogRecorder {PrintToConsole = false};
        }
        
        /// <summary>
        /// Record a log.
        /// </summary>
        private void RecordRawText(string text)
        {
            if (Subscriber != null)
            {
                Subscriber.Publish("logs/record", text);
            }
            else if (Logger != null)
            {
                Logger.RecordRawText(text);
            }

            if (PrintToConsole)
            {
                Console.WriteLine(text);
            }
        }
        
        /// <summary>
        /// Record a milestone log.
        /// Milestone log represents important time points of a program. 
        /// </summary>
        /// <param name="text">Text of the log.</param>
        public void RecordMilestone(string text)
        {
            RecordRawText(LogRecorder.GenerateLogText(text, LogRecorder.Severity.Milestone, Author));
        }

        /// <summary>
        /// Record a message log.
        /// Message log represents simple output of a program.
        /// </summary>
        /// <param name="text">Text of the log.</param>
        public void RecordMessage(string text)
        {
            RecordRawText(LogRecorder.GenerateLogText(text, LogRecorder.Severity.Message, Author));
        }

        /// <summary>
        /// Record a warning log.
        /// Warning log represents important messages that should pay attention to.
        /// </summary>
        /// <param name="text">Text of the log.</param>
        public void RecordWarning(string text)
        {
            RecordRawText(LogRecorder.GenerateLogText(text, LogRecorder.Severity.Warning, Author));
        }

        /// <summary>
        /// Record an error log.
        /// Error log represents the abnormal situation of a program.
        /// </summary>
        /// <param name="text">Text of the log.</param>
        public void RecordError(string text)
        {
            RecordRawText(LogRecorder.GenerateLogText(text, LogRecorder.Severity.Error, Author));
        }
    }
}