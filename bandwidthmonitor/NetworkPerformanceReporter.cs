using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;

namespace clawSoft.bandwidthmonitor
{
    public sealed class NetworkPerformanceReporter
    {
        public static bool mobile = false;
        private static bool _stopping;
        private static TraceEventSession etwSession;
        public static Dictionary<string, Counters> dicData = new Dictionary<string, Counters>();
        private readonly Counters _mCounters = new Counters();

        public static NetworkPerformanceReporter Create()
        {
            var networkPerformancePresenter = new NetworkPerformanceReporter();
            networkPerformancePresenter.Initialise();
            return networkPerformancePresenter;
        }

        private void Initialise()
        {
            Task.Run(() => StartEtwSession());
        }

        private void StartEtwSession()
        {
            var etwtask = Task.Run(() =>
            {
                using (etwSession = new TraceEventSession(KernelTraceEventParser.KernelSessionName))
                {
                    etwSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);
                    etwSession.Source.Kernel.TcpIpRecv += KernelOnTcpIpRecv;
                    etwSession.Source.Kernel.TcpIpRecvIPV6 += KernelOnTcpIpRecvIpv6;
                    etwSession.Source.Kernel.TcpIpSend += KernelOnTcpIpSend;
                    etwSession.Source.Kernel.TcpIpSendIPV6 += KernelOnTcpIpSendIpv6;
                    etwSession.Source.Process();
                }
            });
            Task.WaitAll(etwtask);
            Console.WriteLine("\nStop logging...");
            Environment.Exit(0);
        }

        private void KernelOnTcpIpSendIpv6(TcpIpV6SendTraceData data)
        {
            if (_stopping) return;
            if (Program.bmobile == "true" && mobile || Program.bmobile == "false")
                lock (_mCounters)
                {
                    if (dicData.ContainsKey(data.ProcessName))
                        dicData[data.ProcessName].Sent =
                            dicData[data.ProcessName].Sent + data.size / 1024f / 1024f;
                    else
                        dicData.Add(data.ProcessName,
                            new Counters {Received = 0, Sent = data.size / 1024f / 1024f});
                }
        }

        private void KernelOnTcpIpSend(TcpIpSendTraceData data)
        {
            if (_stopping) return;
            if (Program.bmobile == "true" && mobile || Program.bmobile == "false")
                lock (_mCounters)
                {
                    if (dicData.ContainsKey(data.ProcessName))
                        dicData[data.ProcessName].Sent =
                            dicData[data.ProcessName].Sent + data.size / 1024f / 1024f;
                    else
                        dicData.Add(data.ProcessName,
                            new Counters {Received = 0, Sent = data.size / 1024f / 1024f});
                }
        }

        private void KernelOnTcpIpRecvIpv6(TcpIpV6TraceData data)
        {
            if (_stopping) return;
            if (Program.bmobile == "true" && mobile || Program.bmobile == "false")
                lock (_mCounters)
                {
                    if (dicData.ContainsKey(data.ProcessName))
                        dicData[data.ProcessName].Received =
                            dicData[data.ProcessName].Received + data.size / 1024f / 1024f;
                    else
                        dicData.Add(data.ProcessName,
                            new Counters {Received = data.size / 1024f / 1024f, Sent = 0});
                }
        }

        private void KernelOnTcpIpRecv(TcpIpTraceData data)
        {
            if (_stopping) return;
            if (Program.bmobile == "true" && mobile || Program.bmobile == "false")
                lock (_mCounters)
                {
                    if (dicData.ContainsKey(data.ProcessName))
                        dicData[data.ProcessName].Received =
                            dicData[data.ProcessName].Received + data.size / 1024f / 1024f;
                    else
                        dicData.Add(data.ProcessName,
                            new Counters {Received = data.size / 1024f / 1024f, Sent = 0});
                }
        }

        public static void StopSessions()
        {
            _stopping = true;
            etwSession?.Dispose();
        }

        public class Counters
        {
            public double Received { get; set; }
            public double Sent { get; set; }
        }
    }
}