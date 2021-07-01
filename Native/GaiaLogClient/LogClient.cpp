#include "LogClient.hpp"

#include <iostream>

namespace Gaia::LogService
{
    /// Try to connect to the Redis server, and it will use a local file instead if failed.
    LogClient::LogClient(unsigned int port, const std::string &ip)
    {
        try
        {
            Connection = std::make_unique<sw::redis::Redis>("tcp://" + ip + ":" + std::to_string(port));
            if (Connection->publish("logs/record",
                                    LogRecorder::GenerateLogText("Log service client connected.",
                                    LogRecorder::Severity::Message, Author)) <1)
            {
                throw std::runtime_error("No log server detected on " + ip + ":" + std::to_string(port));
            }
        }catch (std::exception& error)
        {
            Connection.reset();
            Logger = std::make_unique<LogRecorder>();
            Logger->RecordError(error.what(), Author);
            Logger->RecordError("Failed to connect the Redis server on " + ip + ":" + std::to_string(port));
        }
    }

    /// Record a raw text into the log.
    void LogClient::RecordRawText(const std::string& text)
    {
        if (Connection)
        {
            Connection->publish("logs/record", text);
        }
        else if (Logger)
        {
            Logger->RecordRawText(text);
        }

        if (PrintToConsole)
        {
            std::cout << text << std::endl;
        }
    }

    /// Record a message log.
    void LogClient::RecordMessage(const std::string& text)
    {
        RecordRawText(LogRecorder::GenerateLogText(text, LogRecorder::Severity::Message, Author));
    }

    /// Record a milestone log.
    void LogClient::RecordMilestone(const std::string& text)
    {
        RecordRawText(LogRecorder::GenerateLogText(text, LogRecorder::Severity::Milestone, Author));
    }

    /// Record a warning log.
    void LogClient::RecordWarning(const std::string& text)
    {
        RecordRawText(LogRecorder::GenerateLogText(text, LogRecorder::Severity::Warning, Author));
    }

    /// Record an error log.
    void LogClient::RecordError(const std::string& text)
    {
        RecordRawText(LogRecorder::GenerateLogText(text, LogRecorder::Severity::Error, Author));
    }
}