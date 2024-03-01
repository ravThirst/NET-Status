// Decompiled with JetBrains decompiler
// Type: IPConfigParser
// Assembly: NETstatus-test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2C21819A-A271-44DF-87FF-9A0712D306F0
// Assembly location: C:\Users\User\Desktop\scripts\тест соединения\win-x64\NETstatus-test.dll

using NET_Status;
using System.Diagnostics;
using System.Text;


#nullable enable
public static class DeviceManagerParser
{
    private static Dictionary<string, string> previousConfig = new Dictionary<string, string>();
    private static CancellationTokenSource cts = new CancellationTokenSource();
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(1.0);

    public static async Task StartParsingAsync()
    {
        cts = new CancellationTokenSource();
        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                Dictionary<string, string> currentConfig = await GetIPConfigAsync();
                await CheckForChanges(previousConfig, currentConfig);
                previousConfig = currentConfig;
                await Task.Delay(UpdateInterval);
                currentConfig = null;
            }
        }
        catch (OperationCanceledException ex)
        {
        }
    }

    public static void StopParsing() => cts?.Cancel();

    private static async Task<Dictionary<string, string>> GetIPConfigAsync()
    {
        Dictionary<string, string> config = new Dictionary<string, string>();
        using (Process process = new Process())
        {
            process.StartInfo.FileName = "pnputil";
            process.StartInfo.Arguments = "/enum-devices /connected";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(866);
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string endAsync = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();
            using (StringReader stringReader = new StringReader(endAsync))
            {
                string str;
                while ((str = stringReader.ReadLine()) != null)
                {
                    Debug.WriteLine(str);
                    if (str.Contains(':'))
                    {
                        int length = str.IndexOf(':');
                        if (length > 0)
                            config[str.Substring(0, length).Trim()] = str.Substring(length + 1).Trim();
                    }
                }
            }
        }
        Dictionary<string, string> ipConfigAsync = config;
        config = null;
        return ipConfigAsync;
    }

    private static async Task CheckForChanges(
      Dictionary<string, string> previous,
      Dictionary<string, string> current)
    {
        bool changed = false;
        foreach (KeyValuePair<string, string> keyValuePair in current)
        {
            string str;
            if (previous.TryGetValue(keyValuePair.Key, out str))
            {
                if (str != keyValuePair.Value)
                {
                    changed = true;
                    var result = $"Change detected in {keyValuePair.Key}, previous value: {str}, current value: {keyValuePair.Value}";
                    await Info.SetVariableAsync("list", result);
                }
            }
            else
            {
                changed = true;
                await Info.SetVariableAsync("list", "New property detected: " + keyValuePair.Key + ", value: " + keyValuePair.Value);
            }
        }
        int num = changed ? 1 : 0;
    }
}
