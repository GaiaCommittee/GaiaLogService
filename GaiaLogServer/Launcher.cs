using System;
using System.Threading;
using StackExchange.Redis;
using Microsoft.Extensions.CommandLineUtils;

namespace Gaia.LogService
{
    /// <summary>
    /// Launcher will startup the log service, and manage its life cycle.
    /// </summary>
    public class Launcher
    {
        /// <summary>
        /// Entrance function which will parse the command line arguments and manage the service instance.
        /// </summary>
        /// <param name="arguments">Arguments from command line.</param>
        public static void Main(string[] arguments)
        {
            // Prepare command line arguments parser.
            var application = new CommandLineApplication();
            var option_host = application.Option("-h | --host <address>", 
                "set the ip address of the Redis server to connect.",
                CommandOptionType.SingleValue);
            var option_port = application.Option("-p | --port <number>", 
                "set the port number of the Redis server to connect.",
                CommandOptionType.SingleValue);
            var option_path = application.Option("-d | --directory <path>",
                "path of the directory to storage log files.",
                CommandOptionType.SingleValue);
            application.HelpOption("-? | --help");
            
            application.OnExecute(() =>
            {
                bool crashed = false;
                // Loop until launcher normally exited.
                do
                {
                    try
                    {
                        var launcher = new Launcher();
                        crashed = false;
                        Console.WriteLine("Launching log service...");
                        launcher.Launch(
                            option_path.HasValue() ? option_path.Value() : "./Logs",
                            option_port.HasValue() ? Convert.ToUInt32(option_port.Value()) : 6379,
                            option_host.HasValue() ? option_host.Value() : "127.0.0.1"
                        );
                    }
                    catch (Exception error)
                    {
                        crashed = true;
                        Console.WriteLine(error.ToString());
                        Console.WriteLine("Log service crashed. Restart in 1 seconds.");
                        
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                } while (crashed);

                return 0;
            });

            // Parse command line arguments and then perform the action.
            application.Execute(arguments);
        }

        /// <summary>
        /// Instance for log recording.
        /// </summary>
        private LogRecorder Log;
        /// <summary>
        /// When it is false, the service main loop will exit.
        /// </summary>
        private bool LifeFlag = true;
        
        /// <summary>
        /// Startup the log service and subscribe channels.
        /// </summary>
        /// <param name="path">Path to storage the log files.</param>
        /// <param name="port">Port to the Redis server.</param>
        /// <param name="ip">IP address to the Redis server.</param>
        private void Launch(string path = "./Logs", uint port = 6379, string ip = "127.0.0.1")
        {
            Log = new LogRecorder(path);
            
            Console.WriteLine($"Log file {Log.LogName} created.");
            
            
            var connection = ConnectionMultiplexer.Connect($"{ip}:{port.ToString()}");
            var subscriber = connection.GetSubscriber();
            subscriber.Subscribe("logs/record", (channel, value) =>
            {
                Log.RecordRawText(value);
            });
            subscriber.Subscribe("logs/command", (channel, value) =>
            {
                HandleCommand(value);
            });

            Console.WriteLine($"Log service online. Redis server on {ip}:{port.ToString()} connected.");
            
            while (LifeFlag)
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
            
            Console.WriteLine($"Log service stopped.");
        }

        /// <summary>
        /// Parse and execute the command.
        /// </summary>
        /// <param name="command">Command text.</param>
        private void HandleCommand(string command)
        {
            switch (command)
            {
                case "shutdown":
                    LifeFlag = false;
                    break;
            }
        }
    }
}