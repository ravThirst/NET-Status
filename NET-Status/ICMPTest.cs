using System.Net.NetworkInformation;


#nullable enable
namespace NET_Status
{
    internal class ICMPTest
    {
        private static List<string> TargetHosts = new List<string>();
        public static CancellationTokenSource cts = new CancellationTokenSource();
        public int TargetHostsCounnt { get => TargetHosts.Count(); }
        public static async Task Start(string targetHost)
        {
            TargetHosts = new List<string>()
      {
        "185.216.16.162",
        "d0bc0bb8f727.sn.mynetname.net"
      };
            TargetHosts.Add(targetHost);
            using (CancellationTokenSource cats = new CancellationTokenSource())
            {
                cts = cats;
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };
                await RunICMPTestsAsync(cts.Token);
            }
        }

        public static async Task Start(List<string> targetHosts)
        {
            TargetHosts = new List<string>()
      {
        "185.216.16.162",
        "d0bc0bb8f727.sn.mynetname.net"
      };
            TargetHosts.AddRange(targetHosts);
            using (CancellationTokenSource cats = new CancellationTokenSource())
            {
                cts = cats;
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };
                await RunICMPTestsAsync(cts.Token);
            }
        }

        private static async Task RunICMPTestsAsync(CancellationToken cancellationToken)
        {
            List<Task> taskList = new List<Task>();
            foreach (string targetHost in TargetHosts)
            {
                Task task = TestICMPConnectionAsync(targetHost, cancellationToken);
                taskList.Add(task);
            }
            await Task.WhenAll(taskList);
        }

        private static async Task TestICMPConnectionAsync(
          string host,
          CancellationToken cancellationToken)
        {
            using (Ping ping = new Ping())
            {
                int successCount = 0;
                int totalPings = 0;
                int consecutiveFailures = 0;
                int errorCount = 1;
                var result = "";
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        PingReply pingReply = await ping.SendPingAsync(host, 1000);
                        ++totalPings;
                        if (pingReply.Status == IPStatus.Success)
                        {
                            ++successCount;
                            consecutiveFailures = 0;
                        }
                        else
                            ++consecutiveFailures;
                        double lossPercentage = 100.0 - successCount * 100.0 / totalPings;
                        await Info.SetVariableAsync("Host " + host + " Success", successCount.ToString());
                        await Info.SetVariableAsync("Host " + host + " Total", totalPings.ToString());
                        await Info.SetVariableAsync("Host " + host + " Loss percentage", $"{Math.Round(lossPercentage, 2)} %");
                        if (consecutiveFailures >= 2)
                        {
                            result = $"Host {host}, DateTime {DateTime.Now}, ConnectivityIssue {errorCount++}";
                            await Info.SetVariableAsync("list", result);
                        }
                        await Task.Delay(TimeSpan.FromMilliseconds(400.0), cancellationToken);
                    }
                    catch (PingException ex)
                    {
                        result = $"Host {host}, DateTime {DateTime.Now}, Error {ex.Message}";
                        await Info.SetVariableAsync("list", result);
                        ++consecutiveFailures;
                        await Task.Delay(TimeSpan.FromMilliseconds(400.0), cancellationToken);
                    }
                    catch (OperationCanceledException ex)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        result = $"Host {host}, DateTime {DateTime.Now}, Error {ex.Message}";
                        await Info.SetVariableAsync("list", result);
                        ++consecutiveFailures;
                        await Task.Delay(TimeSpan.FromMilliseconds(400.0), cancellationToken);
                    }
                }
            }
        }
    }
}
