
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;


#nullable enable
namespace NETstatus_test
{
    public static class DNSTest
    {
        private static List<string> popularWebsites = new List<string>()
    {
      "google.com",
      "youtube.com"
    };
        private static Dictionary<string, List<IPAddress>> hostToIPDictionary = new Dictionary<string, List<IPAddress>>();
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public static async Task StartTestAsync(string hostName = "yandex.ru")
        {
            DNSTest.cancellationTokenSource = new CancellationTokenSource();
            DNSTest.hostToIPDictionary = new Dictionary<string, List<IPAddress>>();
            DNSTest.popularWebsites.Add(hostName);
            try
            {
                while (!DNSTest.cancellationTokenSource.Token.IsCancellationRequested)
                {
                    foreach (string website in DNSTest.popularWebsites)
                    {
                        try
                        {
                            List<IPAddress> newIPAddresses = await DNSTest.ResolveDNSAsync(website);
                            List<IPAddress> old;
                            if (DNSTest.hostToIPDictionary.TryGetValue(website, out old))
                            {
                                List<string> addedItems = DNSTest.GetAddedItems(old, newIPAddresses);
                                List<string> removedItems = DNSTest.GetRemovedItems(old, newIPAddresses);
                                if (DNSTest.EqualLists(old, newIPAddresses))
                                {
                                    var result = $"Host: {website}, New IPs: {string.Join(", ", addedItems.Select(x => x.ToString()))}" +
                                        $", Old IPs: {string.Join(", ", removedItems.Select(x => x.ToString()))}, Time: {DateTime.Now}";
                                    await Info.SetVariableAsync("list", result);
                                }
                            }
                            else
                            {
                                var result = $"Host: {website}, IP: {string.Join(", ", newIPAddresses.Select(x => x.ToString()))}" +
                                        $", Time: {DateTime.Now}";
                                await Info.SetVariableAsync("list", result);
                            }
                            DNSTest.hostToIPDictionary[website] = newIPAddresses;
                            newIPAddresses = (List<IPAddress>)null;
                        }
                        catch (Exception ex)
                        {
                            var result = $"Host: {website}, Error: {ex.Message}" +
                                        $", Time: {DateTime.Now}";
                            await Info.SetVariableAsync("list", result);
                        }
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1.0), DNSTest.cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException ex)
            {
            }
        }

        public static void StopTest() => DNSTest.cancellationTokenSource?.Cancel();

        private static bool EqualLists(List<IPAddress> old, List<IPAddress> _new)
        {
            List<string> addedItems = DNSTest.GetAddedItems(old, _new);
            List<string> removedItems = DNSTest.GetRemovedItems(old, _new);
            return addedItems.Count > 0 || removedItems.Count > 0;
        }

        private static List<string> GetAddedItems(List<IPAddress> old, List<IPAddress> _new)
        {
            List<string> list = old.Select<IPAddress, string>((Func<IPAddress, string>)(x => x.ToString())).ToList<string>();
            return _new.Select<IPAddress, string>((Func<IPAddress, string>)(x => x.ToString())).ToList<string>().Except<string>((IEnumerable<string>)list).ToList<string>();
        }

        private static List<string> GetRemovedItems(List<IPAddress> old, List<IPAddress> _new) => old.Select<IPAddress, string>((Func<IPAddress, string>)(x => x.ToString())).ToList<string>().Except<string>((IEnumerable<string>)_new.Select<IPAddress, string>((Func<IPAddress, string>)(x => x.ToString())).ToList<string>()).ToList<string>();

        private static void AddValue(string key, IPAddress iPAddress)
        {
            if (DNSTest.hostToIPDictionary.ContainsKey(key))
                DNSTest.hostToIPDictionary[key].Add(iPAddress);
            else
                DNSTest.hostToIPDictionary.Add(key, new List<IPAddress>()
        {
          iPAddress
        });
        }

        private static void RemoveValue(string key, IPAddress iPAddress)
        {
            if (!DNSTest.hostToIPDictionary.ContainsKey(key) || !DNSTest.hostToIPDictionary[key].Any<IPAddress>((Func<IPAddress, bool>)(x => x.ToString() == iPAddress.ToString())))
                return;
            DNSTest.hostToIPDictionary[key].Add(iPAddress);
        }

        private static async Task<List<IPAddress>> ResolveDNSAsync(string hostname) => ((IEnumerable<IPAddress>)(await Dns.GetHostEntryAsync(hostname)).AddressList).ToList<IPAddress>();
    }
}
