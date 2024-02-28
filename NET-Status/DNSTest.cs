using System.Net;


#nullable enable
namespace NET_Status
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
            cancellationTokenSource = new CancellationTokenSource();
            hostToIPDictionary = new Dictionary<string, List<IPAddress>>();
            popularWebsites.Add(hostName);
            try
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    foreach (string website in popularWebsites)
                    {
                        try
                        {
                            List<IPAddress> newIPAddresses = await ResolveDNSAsync(website);
                            List<IPAddress> old;
                            if (hostToIPDictionary.TryGetValue(website, out old))
                            {
                                List<string> addedItems = GetAddedItems(old, newIPAddresses);
                                List<string> removedItems = GetRemovedItems(old, newIPAddresses);
                                if (EqualLists(old, newIPAddresses))
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
                            hostToIPDictionary[website] = newIPAddresses;
                            newIPAddresses = null;
                        }
                        catch (Exception ex)
                        {
                            var result = $"Host: {website}, Error: {ex.Message}" +
                                        $", Time: {DateTime.Now}";
                            await Info.SetVariableAsync("list", result);
                        }
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1.0), cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException ex)
            {
            }
        }

        public static void StopTest() => cancellationTokenSource?.Cancel();

        private static bool EqualLists(List<IPAddress> old, List<IPAddress> _new)
        {
            List<string> addedItems = GetAddedItems(old, _new);
            List<string> removedItems = GetRemovedItems(old, _new);
            return addedItems.Count > 0 || removedItems.Count > 0;
        }

        private static List<string> GetAddedItems(List<IPAddress> old, List<IPAddress> _new)
        {
            List<string> list = old.Select(x => x.ToString()).ToList();
            return _new.Select(x => x.ToString()).ToList().Except(list).ToList();
        }

        private static List<string> GetRemovedItems(List<IPAddress> old, List<IPAddress> _new) => old.Select(x => x.ToString()).ToList().Except(_new.Select(x => x.ToString()).ToList()).ToList();

        private static void AddValue(string key, IPAddress iPAddress)
        {
            if (hostToIPDictionary.ContainsKey(key))
                hostToIPDictionary[key].Add(iPAddress);
            else
                hostToIPDictionary.Add(key, new List<IPAddress>()
        {
          iPAddress
        });
        }

        private static void RemoveValue(string key, IPAddress iPAddress)
        {
            if (!hostToIPDictionary.ContainsKey(key) || !hostToIPDictionary[key].Any(x => x.ToString() == iPAddress.ToString()))
                return;
            hostToIPDictionary[key].Add(iPAddress);
        }

        private static async Task<List<IPAddress>> ResolveDNSAsync(string hostname) => (await Dns.GetHostEntryAsync(hostname)).AddressList.ToList();
    }
}
