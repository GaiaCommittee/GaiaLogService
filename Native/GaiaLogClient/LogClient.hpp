#pragma once

#include <string>
#include <memory>
#include <fstream>
#include <sw/redis++/redis++.h>

#include "LogRecorder.hpp"

namespace Gaia::LogService
{
    /**
     * @brief Client for log service, provides log recording service.
     * @details
     *  If it failed to connect to the given Redis server,
     *  it will create a local log file and use it instead.
     */
    class LogClient
    {
    private:
        /// Local file log recorder.
        std::unique_ptr<LogRecorder> Logger;
        /// Remote log service connection.
        std::shared_ptr<sw::redis::Redis> Connection;

        /// Record a raw text into the log.
        void RecordRawText(const std::string& text);

    public:
        /// The author of the logs.
        std::string Author {"Anonymous"};
        /// Whether logs will be printed to the console or not.
        bool PrintToConsole {false};

        /**
         * @brief Try to connect to the Redis server, and it will use a local file instead if failed.
         * @param port Port of the Redis server.
         * @param ip IP address of the Redis server.
         */
        explicit LogClient(unsigned int port = 6379, const std::string& ip = "127.0.0.1");
        /// Reuse the connection to a Redis server.
        explicit LogClient(std::shared_ptr<sw::redis::Redis> connection);

        /**
         * @brief Record a message log.
         * @param text Text of the log.
         * @details Message log represents simple output of a program.
         */
        void RecordMessage(const std::string& text);
        /**
         * @brief Record a milestone log.
         * @param text Text of the log.
         * @details Milestone log represents important time points of a program.
         */
        void RecordMilestone(const std::string& text);
        /**
         * @brief Record a warning log.
         * @param text Text of the log.
         * @details Warning log represents important messages that should pay attention to.
         */
        void RecordWarning(const std::string& text);
        /**
         * @brief Record an error log.
         * @param text Text of the log.
         * @details Error log represents the abnormal situation of a program.
         */
        void RecordError(const std::string& text);
    };
}