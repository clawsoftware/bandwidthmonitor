using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;

namespace clawSoft.bandwidthmonitor
{
    internal class Program
    {
        private static readonly Random Rnd = new Random();
        private static string _logtext = "";
        private static string _logtextcsv = "";

        private static readonly string Logfile = @"\bandwidthmonitor_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" +
                                                 Environment.MachineName + "_" + Rnd.Next(10000, 99999) + ".log";

        private static readonly string Logfilecsv = @"\bandwidthmonitor_" + DateTime.Now.ToString("yyyyMMddHHmmss") +
                                                    "_" + Environment.MachineName + "_" + Rnd.Next(10000, 99999) +
                                                    ".csv";

        private static string brun = "true";
        public static string bmobile = "false";
        private static string bsort = "d";
        private static string bcsv = "false";
        private static string _logpath;


        private static int help()
        {
            Console.WriteLine(" ");
            Console.WriteLine("{0,-30}{1,20}", "Copyright © 2019 Andrew Hess // clawSoft", "");
            Console.WriteLine("{0,-30}{1,20}", "Licensed under MIT License", "");
            Console.WriteLine(" ");
            Console.WriteLine("{0,-30}{1,20}", " /m=false|true",
                "Log only when mobile connection is active. Default: false");
            Console.WriteLine("{0,-30}{1,20}", " /csv=false|true", "Log as CSV-File. Default: false");
            Console.WriteLine("{0,-30}{1,20}", " /s=u|d", "Which Column to sort. Upload or download. Default: d");
            Console.WriteLine("{0,-30}{1,20}", " /l=\"logfolder\"",
                "Logfile destination. Default: %EXEFOLDER%\\bandwidthmonitor_%date%_%computer%_%randomnum%.log");
            return 1;
        }

        private static void Main(string[] args)
        {
            NetworkChange.NetworkAddressChanged += AvailabilityChanged;
            mobile();
            Console.CancelKeyPress += (sender, cancelArgs) =>
            {
                NetworkPerformanceReporter.StopSessions();
                cancelArgs.Cancel = true;
            };
            arg(args);
        }

        private static void mobile()
        {
            if (IsMobileConnectionActive())
                NetworkPerformanceReporter.mobile = true;
            else
                NetworkPerformanceReporter.mobile = false;
        }

        private static void AvailabilityChanged(object sender, EventArgs e)
        {
            mobile();
        }

        private static bool IsMobileConnectionActive()
        {
            if (!NetworkInterface.GetIsNetworkAvailable()) return false;

            var broadbandTypes = new[]
            {
                NetworkInterfaceType.Ppp,
                NetworkInterfaceType.Wwanpp,
                NetworkInterfaceType.Wwanpp2,
                NetworkInterfaceType.Wman
            };

            var mobileInterfaces = from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                where broadbandTypes.Contains(nic.NetworkInterfaceType)
                select nic;
            return mobileInterfaces.Any();
        }

        private static void run()
        {
            NetworkPerformanceReporter.Create();


            while (true)
            {
                try
                {
                    _logtext = "";
                    _logtextcsv = "";
                    Console.Clear();
                    Console.Title = "clawSoft bandwithmonitor v1.0 | Logfile: " + _logpath;
                    if (NetworkPerformanceReporter.mobile)
                        Console.WriteLine("!!! Mobile connection active !!!" + Environment.NewLine);
                    else
                        Console.WriteLine("!!! Mobile connection not active !!!" + Environment.NewLine);
                    if (bmobile == "true" && NetworkPerformanceReporter.mobile)
                    {
                        Console.WriteLine("!!! Logging active !!!" + Environment.NewLine + Environment.NewLine);
                    }
                    else
                    {
                        if (bmobile == "true")
                            Console.WriteLine("!!! Logging paused !!!" + Environment.NewLine + Environment.NewLine);
                    }

                    var header = string.Format("{0,-30}{1,20}{2,25}", "Processes", "Received data (MB)",
                        "Transmitted data (MB)");
                    _logtext = _logtext + header + Environment.NewLine;
                    Console.WriteLine(header);
                    Console.WriteLine("---------------------------------------------------------------------------");
                    _logtext = _logtext +
                               "---------------------------------------------------------------------------" +
                               Environment.NewLine;

                    if (bcsv == "true")
                    {
                        var headercsv = "\"Processes\";" + "\"Received data (MB)\";" + "\"Transmitted data (MB)\"";
                        _logtextcsv = _logtextcsv + headercsv + Environment.NewLine;
                        Console.Title = "clawSoft bandwithmonitor v1.0 | Logfile: " + _logpath;
                    }

                    if (bsort == "u")
                        foreach (var item in NetworkPerformanceReporter.dicData.OrderByDescending(key => key.Value.Sent)
                        )
                        {
                            var output = string.Format("{0,-30}{1,20}{2,25}", item.Key,
                                string.Format("{0:0.00}", item.Value.Received),
                                string.Format("{0:0.00}", item.Value.Sent));
                            Console.WriteLine(output);
                            _logtext = _logtext + output + Environment.NewLine;

                            if (bcsv == "true")
                            {
                                var outputcsv = "\"" + item.Key + "\";\"" +
                                                string.Format("{0:0.00}", item.Value.Received) + "\";\"" +
                                                string.Format("{0:0.00}", item.Value.Sent) + "\"";
                                _logtextcsv = _logtextcsv + outputcsv + Environment.NewLine;
                            }
                        }
                    else
                        foreach (var item in NetworkPerformanceReporter.dicData.OrderByDescending(key =>
                            key.Value.Received))
                        {
                            var output = string.Format("{0,-30}{1,20}{2,25}", item.Key,
                                string.Format("{0:0.00}", item.Value.Received),
                                string.Format("{0:0.00}", item.Value.Sent));
                            Console.WriteLine(output);
                            _logtext = _logtext + output + Environment.NewLine;
                            if (bcsv == "true")
                            {
                                var outputcsv = "\"" + item.Key + "\";\"" +
                                                string.Format("{0:0.00}", item.Value.Received) + "\";\"" +
                                                string.Format("{0:0.00}", item.Value.Sent) + "\"";
                                _logtextcsv = _logtextcsv + outputcsv + Environment.NewLine;
                            }
                        }

                    if (bcsv == "true")
                        File.WriteAllText(_logpath, _logtextcsv);
                    else
                        File.WriteAllText(_logpath, _logtext);
                }
                catch
                {
                }

                Thread.Sleep(5000);
            }
        }

        private static void arg(string[] args)
        {
            if (args.Length == 0)
            {
                _logpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Logfile;
                run();
            }
            else
            {
                for (var i = 0; i < args.Length; i++)
                {
                    var argument = args[i];
                    if (argument.Contains("?") || argument.Contains("-h") || argument.Contains("help"))
                    {
                        brun = "false";
                        help();
                        break;
                    }

                    if (argument.Contains("/m"))
                    {
                        if (argument.Contains("="))
                        {
                            bmobile = argument.Split('=')[1];
                        }
                        else
                        {
                            brun = "false";
                            help();
                        }
                    }

                    if (argument.Contains("/s"))
                    {
                        if (argument.Contains("="))
                        {
                            bsort = argument.Split('=')[1];
                        }
                        else
                        {
                            brun = "false";
                            help();
                        }
                    }

                    if (argument.Contains("/csv"))
                    {
                        if (argument.Contains("="))
                        {
                            bcsv = argument.Split('=')[1];
                        }
                        else
                        {
                            brun = "false";
                            help();
                        }
                    }

                    if (argument.Contains("/l"))
                    {
                        if (argument.Contains("="))
                        {
                            if (bcsv == "true")
                            {
                                _logpath = argument.Split('=')[1] + Logfilecsv;
                                _logpath = _logpath.Replace(@"\\", @"\").Replace("\"", "");
                            }
                            else
                            {
                                _logpath = argument.Split('=')[1] + Logfile;
                                _logpath = _logpath.Replace(@"\\", @"\").Replace("\"", "");
                                ;
                            }
                        }
                        else
                        {
                            brun = "false";
                            help();
                        }
                    }
                    else
                    {
                        if (bcsv == "true")
                            _logpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Logfilecsv;
                        else
                            _logpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Logfile;
                    }
                }

                if (brun == "true") run();
            }
        }
    }
}