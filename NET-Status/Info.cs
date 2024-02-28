// Decompiled with JetBrains decompiler
// Type: NETstatus_test.Info
// Assembly: NETstatus-test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2C21819A-A271-44DF-87FF-9A0712D306F0
// Assembly location: C:\Users\User\Desktop\scripts\тест соединения\win-x64\NETstatus-test.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


#nullable enable
namespace NETstatus_test
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
            if (Info.blackList.Any<string>((Func<string, bool>)(x => value.ToLower().Contains(x))))
                ;
            else
            {
                await Info.semaphore.WaitAsync();
                try
                {
                    if (key.ToLower().Contains("list") && !Info.list.Contains(value))
                        Info.list.Add(value);
                    else if (Info.variables.ContainsKey(key))
                        Info.variables[key] = value;
                    else
                        Info.variables.Add(key, value);
                }
                catch
                {
                    Info.semaphore.Release();
                }
                finally
                {
                    Info.semaphore.Release();
                }
            }
        }

        static Info() => Info.StartLoggingAsync();

        public static async Task StartLoggingAsync()
        {
            if (Info.logFilePath == null)
                throw new ArgumentNullException("logFilePath");
            Info.cts = new CancellationTokenSource();
            await Task.Run((Func<Task>)(async () =>
            {
                while (!Info.cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(1000);
                    await Info.LogVariablesAsync(Info.logFilePath);
                }
            }));
        }

        public static void StopLogging()
        {
            Info.cts?.Cancel();
            Info.cts?.Dispose();
        }

        public static async Task<Dictionary<string, string>> GetVariablesAsync()
        {
            await Info.semaphore.WaitAsync();
            try
            {
                int num;
                try
                {
                    return new Dictionary<string, string>((IDictionary<string, string>)Info.variables);
                }
                catch
                {
                    num = 1;
                }
                if (num == 1)
                {
                    await Task.Delay(500);
                    return Info.variables;
                }
            }
            finally
            {
                Info.semaphore.Release();
            }
            var variablesAsync = new Dictionary<string, string>();
            return variablesAsync;
        }

        private static async Task LogVariablesAsync(string logFilePath)
        {
            await Info.semaphore.WaitAsync();
            try
            {
                File.Delete(logFilePath);
                using (StreamWriter writer = new StreamWriter(logFilePath, false))
                {
                    foreach (KeyValuePair<string, string> variable in Info.variables)
                        await writer.WriteLineAsync(variable.Key + ": " + variable.Value);
                    foreach (string str in Info.list)
                        await writer.WriteLineAsync(str ?? "");
                }
            }
            catch
            {
                Info.semaphore.Release();
            }
            finally
            {
                Info.semaphore.Release();
            }
        }
    }
}
