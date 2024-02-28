// Decompiled with JetBrains decompiler
// Type: NETstatus_test.Info
// Assembly: NETstatus-test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2C21819A-A271-44DF-87FF-9A0712D306F0
// Assembly location: C:\Users\User\Desktop\scripts\тест соединения\win-x64\NETstatus-test.dll


#nullable enable
namespace NET_Status
{
    public static class Info
    {
        private static readonly Dictionary<string, string> variables = new Dictionary<string, string>();
        private static readonly List<string> list = new List<string>();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private static string logFilePath = "log.txt";
        private static List<string> blackList = new List<string>()
    {
      "аренд"
    };

        public static async Task SetVariableAsync(string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (blackList.Any(x => value.ToLower().Contains(x)))
            {

            }
            else
            {
                await semaphore.WaitAsync();
                try
                {
                    if (key.ToLower().Contains("list") && !list.Contains(value))
                        list.Add(value);
                    else if (variables.ContainsKey(key))
                        variables[key] = value;
                    else
                        variables.Add(key, value);
                }
                catch
                {
                    semaphore.Release();
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        static Info() => StartLoggingAsync();

        public static async Task StartLoggingAsync()
        {
            if (logFilePath == null)
                throw new ArgumentNullException("logFilePath");
            cts = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(1000);
                    await LogVariablesAsync(logFilePath);
                }
            });
        }

        public static void StopLogging()
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        public static async Task<Dictionary<string, string>> GetVariablesAsync()
        {
            await semaphore.WaitAsync();
            try
            {
                int num;
                try
                {
                    return new Dictionary<string, string>(variables);
                }
                catch
                {
                    num = 1;
                }
                if (num == 1)
                {
                    await Task.Delay(500);
                    return variables;
                }
            }
            finally
            {
                semaphore.Release();
            }
            var variablesAsync = new Dictionary<string, string>();
            return variablesAsync;
        }

        private static async Task LogVariablesAsync(string logFilePath)
        {
            await semaphore.WaitAsync();
            try
            {
                File.Delete(logFilePath);
                using (StreamWriter writer = new StreamWriter(logFilePath, false))
                {
                    foreach (KeyValuePair<string, string> variable in variables)
                        await writer.WriteLineAsync(variable.Key + ": " + variable.Value);
                    foreach (string str in list)
                        await writer.WriteLineAsync(str ?? "");
                }
            }
            catch
            {
                semaphore.Release();
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
