using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace perf
{
    internal class Program
    {
        private static int Round = 3;

        private static void Main(string[] args)
        {
            var redis = "127.0.0.1";
            if (args.Length > 0)
            {
                redis = args[0];
            }
            if (args.Length > 1)
            {
                Round = int.Parse(args[1]);
            }
            Trace.Listeners.Add(new ConsoleTraceListener());
            try
            {
                Test(redis).Wait();
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error: {ex}");
            }
        }

        private static async Task Test(string redis)
        {
            try
            {
                Trace.TraceInformation($"Connect to redis: {redis}");
                using var cm = await ConnectionMultiplexer.ConnectAsync(
                    $"{redis}:6379");
                var db = cm.GetDatabase(0);
                Trace.TraceInformation("Start test pub/sub.");
                await TestPubSub(cm.GetSubscriber(), db);
                Trace.TraceInformation("Start test stream.");
                await TestStream(db);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error: {ex}");
            }
        }

        private static async Task TestPubSub(ISubscriber sub, IDatabase db)
        {
            RedisChannel ch = "test-pubsub";
            const string prepareMsg = "prepare:123";
            string messageMsg = "message:" + new string('a', 1000);
            const string continueMsg = "continue:456";
            var semaphoreSlim = new SemaphoreSlim(1, 2);
            int count = -1000;
            var q = await sub.SubscribeAsync(ch);
            q.OnMessage(
                cm =>
                {
                    string s = cm.Message;
                    switch (s)
                    {
                        case prepareMsg:
                            count = 0;
                            break;
                        case continueMsg:
                            semaphoreSlim.Release();
                            break;
                        default:
                            count++;
                            break;
                    }
                });
            var sw = new Stopwatch();
            for (int i = 0; i < Round; i++)
            {
                Trace.TraceInformation($"Start pub/sub round {i}");
                await db.PublishAsync(ch, prepareMsg);
                while (Volatile.Read(ref count) != 0)
                {
                    await Task.Delay(1);
                }
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                cts.Token.Register(
                    () => Trace.TraceInformation($"Pub/sub {Volatile.Read(ref count) / sw.Elapsed.TotalSeconds} per second."));
                sw.Restart();
                while (!cts.IsCancellationRequested)
                {
                    var t = db.PublishAsync(ch, continueMsg);
                    for (int j = 0; j < 100; j++)
                    {
                        _ = db.PublishAsync(ch, messageMsg, CommandFlags.FireAndForget);
                    }
                    await t;
                    await semaphoreSlim.WaitAsync();
                }
            }
            await q.UnsubscribeAsync();
        }

        private static async Task TestStream(IDatabase db)
        {
            RedisKey key = "stream";
            const string prepareCmd = "prepare";
            const string messageCmd = "message";
            const string continueCmd = "continue";
            var prepareMsg = new[]
            {
                new NameValueEntry(prepareCmd, "123"),
            };
            var messageMsg = new[]
            {
                new NameValueEntry(messageCmd, new string('a', 1000)),
            };
            var continueMsg = new[]
            {
                new NameValueEntry(continueCmd, "456"),
            };
            var semaphoreSlim = new SemaphoreSlim(1, 2);
            int count = -1000;
            // create stream, or trim stream.
            await db.StreamAddAsync(key, messageMsg, maxLength: 1);
            bool completed = false;
            _ = Task.Run(async () =>
            {
                RedisValue pos = "0-0";
                while (!Volatile.Read(ref completed))
                {
                    var s = await db.StreamReadAsync(key, pos, 100);
                    // Block is not available in StackExchange.Redis 2.1.
                    // Since we are perf test, create a tight loop.
                    if (s.Length == 0)
                    {
                        continue;
                    }
                    foreach (var item in s)
                    {
                        string cmd = item.Values[0].Name;
                        switch (cmd)
                        {
                            case prepareCmd:
                                count = 0;
                                break;
                            case continueCmd:
                                semaphoreSlim.Release();
                                break;
                            default:
                                count++;
                                break;
                        }
                    }
                    pos = s[^1].Id;
                }
            });
            var sw = new Stopwatch();
            for (int i = 0; i < Round; i++)
            {
                Trace.TraceInformation($"Start stream round {i}");
                await db.StreamAddAsync(key, prepareMsg);
                while (Volatile.Read(ref count) != 0)
                {
                    await Task.Delay(1);
                }
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                cts.Token.Register(
                    () => Trace.TraceInformation($"Stream {Volatile.Read(ref count) / sw.Elapsed.TotalSeconds} per second."));
                sw.Restart();
                while (!cts.IsCancellationRequested)
                {
                    var task = db.StreamAddAsync(key, continueMsg, maxLength: 1000, useApproximateMaxLength: true);
                    for (int j = 0; j < 100; j++)
                    {
                        _ = db.StreamAddAsync(key, messageMsg, flags: CommandFlags.FireAndForget);
                    }
                    await task;
                    await semaphoreSlim.WaitAsync();
                }
            }
            completed = true;
        }
    }
}
