using System;
using System.IO;
using System.Text;

namespace Gaia.LogService
{
    public class LogRecorder : IDisposable
    {
        private readonly StreamWriter FileWriter;

        public readonly string LogName;
        
        /// <summary>
        /// When it is true, the log text will be automatically print to the console.
        /// </summary>
        public bool PrintToConsole { get; set; }
        
        /// <summary>
        /// Construct and open the corresponding log file.
        /// </summary>
        public LogRecorder(string path = "./")
        {
            PrintToConsole = false;
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            LogName = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + ".log";
            
            FileWriter = new StreamWriter(Path.Combine(path, LogName))
            {
                AutoFlush = true
            };
        }

        /// <summary>
        /// Will close the log file when destruct.
        /// </summary>
        public void Dispose()
        {
            FileWriter.Close();
        }

        /// <summary>
        /// The severity of the log text.
        /// </summary>
        public enum Severity
        {
            Message,
            Milestone,
            Warning,
            Error
        }

        /// <summary>
        /// Add a raw record line to the log file.
        /// </summary>
        /// <param name="raw_record">The text to record.</param>
        public void RecordRawText(string raw_record)
        {
            FileWriter.WriteLine(raw_record);
            if (PrintToConsole)
            {
                Console.WriteLine(raw_record);
            }
        }

        public static string GenerateLogText(string text, Severity severity, string author = "Anonymous")
        {
            var record_builder = new StringBuilder();
            
            record_builder.Append(DateTime.Now.ToLongTimeString());
            record_builder.Append("|");
            switch (severity)
            {
                case Severity.Message:
                    record_builder.Append("Message");
                    break;
                case Severity.Milestone:
                    record_builder.Append("Milestone");
                    break;
                case Severity.Warning:
                    record_builder.Append("Warning");
                    break;
                case Severity.Error:
                    record_builder.Append("Error");
                    break;
            }
            record_builder.Append("|");
            record_builder.Append(author);
            record_builder.Append("|");
            record_builder.Append(text);

            return record_builder.ToString();
        }
        
        /// <summary>
        /// Record a log.
        /// </summary>
        /// <param name="text">The text of the log.</param>
        /// <param name="severity">The severity of the log.</param>
        /// <param name="author">The author of the log.</param>
        public void Record(string text, Severity severity, string author = "Anonymous")
        {
            RecordRawText(GenerateLogText(text, severity, author));
        }
        
        /// <summary>
        /// Record a milestone log.
        /// Milestone log represents important time points of a program. 
        /// </summary>
        /// <param name="text">Text of the log.</param>
        /// <param name="author">Author of the log.</param>
        public void RecordMilestone(string text, string author = "Anonymous")
        {
            Record(text, Severity.Milestone, author);
        }

        /// <summary>
        /// Record a message log.
        /// Message log represents simple output of a program.
        /// </summary>
        /// <param name="text">Text of the log.</param>
        /// <param name="author">Author of the log.</param>
        public void RecordMessage(string text, string author = "Anonymous")
        {
            Record(text, Severity.Message, author);
        }

        /// <summary>
        /// Record a warning log.
        /// Warning log represents important messages that should pay attention to.
        /// </summary>
        /// <param name="text">Text of the log.</param>
        /// <param name="author">Author of the log.</param>
        public void RecordWarning(string text, string author = "Anonymous")
        {
            Record(text, Severity.Warning, author);
        }

        /// <summary>
        /// Record an error log.
        /// Error log represents the abnormal situation of a program.
        /// </summary>
        /// <param name="text">Text of the log.</param>
        /// <param name="author">Author of the log.</param>
        public void RecordError(string text, string author = "Anonymous")
        {
            Record(text, Severity.Error, author);
        }
    }
}