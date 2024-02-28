// Decompiled with JetBrains decompiler
// Type: NETstatus_test.ICMPTest
// Assembly: NETstatus-test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2C21819A-A271-44DF-87FF-9A0712D306F0
// Assembly location: C:\Users\User\Desktop\scripts\тест соединения\win-x64\NETstatus-test.dll

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;


#nullable enable
namespace NETstatus_test
{
    internal class ICMPTest
    {
        private static List<string> TargetHosts = new List<string>();
        public static CancellationTokenSource cts = new CancellationTokenSource();

        public static async Task Start(string targetHost)
        {
            ICMPTest.TargetHosts = new List<string>()
      {
        "185.216.16.162",
        "d0bc0bb8f727.sn.mynetname.net"
      };
            ICMPTest.TargetHosts.Add(targetHost);
            using (CancellationTokenSource cats = new CancellationTokenSource())
            {
                ICMPTest.cts = cats;
                Console.CancelKeyPress += (ConsoleCancelEventHandler)((s, e) =>
                {
                    e.Cancel = true;
                    ICMPTest.cts.Cancel();
                });
                await ICMPTest.RunICMPTestsAsync(ICMPTest.cts.Token);
            }
        }

        public static async Task Start(List<string> targetHosts)
        {
            ICMPTest.TargetHosts = new List<string>()
      {
        "185.216.16.162",
        "d0bc0bb8f727.sn.mynetname.net"
      };
            ICMPTest.TargetHosts.AddRange((IEnumerable<string>)targetHosts);
            using (CancellationTokenSource cats = new CancellationTokenSource())
            {
                ICMPTest.cts = cats;
                Console.CancelKeyPress += (ConsoleCancelEventHandler)((s, e) =>
                {
                    e.Cancel = true;
                    ICMPTest.cts.Cancel();
                });
                await ICMPTest.RunICMPTestsAsync(ICMPTest.cts.Token);
            }
        }

        private static async Task RunICMPTestsAsync(CancellationToken cancellationToken)
        {
            List<Task> taskList = new List<Task>();
            foreach (string targetHost in ICMPTest.TargetHosts)
            {
                Task task = ICMPTest.TestICMPConnectionAsync(targetHost, cancellationToken);
                taskList.Add(task);
            }
            await Task.WhenAll((IEnumerable<Task>)taskList);
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
                        PingReply pingReply = await ping.SendPingAsync(host);
                        ++totalPings;
                        if (pingReply.Status == IPStatus.Success)
                        {
                            ++successCount;
                            consecutiveFailures = 0;
                        }
                        else
                            ++consecutiveFailures;
                        double lossPercentage = 100.0 - (double)successCount * 100.0 / (double)totalPings;
                        await Info.SetVariableAsync("Host " + host + " Success", successCount.ToString());
                        await Info.SetVariableAsync("Host " + host + " Total", totalPings.ToString());
                        string key = "Host " + host + " LossPercentage";
                        result = $"lossPercentage"
                        interpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 1);
                        interpolatedStringHandler.AppendFormatted<double>(lossPercentage);
                        interpolatedStringHandler.AppendLiteral("%");
                        string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                        await Info.SetVariableAsync(key, stringAndClear);
                        if (consecutiveFailures >= 2)
                        {
                            interpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 3);
                            interpolatedStringHandler.AppendLiteral("Host ");
                            interpolatedStringHandler.AppendFormatted(host);
                            interpolatedStringHandler.AppendLiteral(" DateTime ");
                            interpolatedStringHandler.AppendFormatted<DateTime>(DateTime.Now);
                            interpolatedStringHandler.AppendLiteral(" ConnectivityIssue ");
                            interpolatedStringHandler.AppendFormatted<int>(errorCount++);
                            await Info.SetVariableAsync("list", interpolatedStringHandler.ToStringAndClear());
                        }
                        await Task.Delay(TimeSpan.FromMilliseconds(400.0), cancellationToken);
                    }
                    catch (PingException ex)
                    {
                        interpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 3);
                        interpolatedStringHandler.AppendLiteral("Host ");
                        interpolatedStringHandler.AppendFormatted(host);
                        interpolatedStringHandler.AppendLiteral(" DateTime ");
                        interpolatedStringHandler.AppendFormatted<DateTime>(DateTime.Now);
                        interpolatedStringHandler.AppendLiteral(" Error ");
                        interpolatedStringHandler.AppendFormatted(ex.Message);
                        await Info.SetVariableAsync("list", interpolatedStringHandler.ToStringAndClear());
                        ++consecutiveFailures;
                        await Task.Delay(TimeSpan.FromMilliseconds(400.0), cancellationToken);
                    }
                    catch (OperationCanceledException ex)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        interpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 3);
                        interpolatedStringHandler.AppendLiteral("Host ");
                        interpolatedStringHandler.AppendFormatted(host);
                        interpolatedStringHandler.AppendLiteral(" DateTime ");
                        interpolatedStringHandler.AppendFormatted<DateTime>(DateTime.Now);
                        interpolatedStringHandler.AppendLiteral(" Error ");
                        interpolatedStringHandler.AppendFormatted(ex.Message);
                        await Info.SetVariableAsync("list", interpolatedStringHandler.ToStringAndClear());
                        ++consecutiveFailures;
                        await Task.Delay(TimeSpan.FromMilliseconds(400.0), cancellationToken);
                    }
                }
            }
        }
    }
}
