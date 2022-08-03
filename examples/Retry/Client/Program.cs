// Copyright (c) ZeroC, Inc. All rights reserved.

using Demo;
using IceRpc;
using IceRpc.Retry;
using IceRpc.Slice;
using Microsoft.Extensions.Logging;

if (args.Length < 1)
{
    Console.WriteLine("Missing server instances argument");
    return;
}

int serverInstances;
if (!int.TryParse(args[0], out serverInstances))
{
    Console.WriteLine($"Invalid server instances argument '{args[0]}', expected a number");
    return;
}

using var cancellationSource = new CancellationTokenSource();
Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationSource.Cancel();
};

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddFilter("IceRpc", LogLevel.Information);
        builder.AddSimpleConsole(configure => configure.IncludeScopes = true);
    });

await using var connectionCache = new ConnectionCache(
    new ConnectionCacheOptions { PreferExistingConnection = true },
    loggerFactory: loggerFactory);

// Create an invocation pipeline with the retry and logger interceptors.
var pipeline = new Pipeline()
    .UseRetry(
        // Make up to 5 attempts before giving up
        new RetryOptions { MaxAttempts = 5 },
        loggerFactory)
    .UseLogger(loggerFactory)
    .Into(connectionCache);

string helloServiceAddress = "icerpc://127.0.0.1:10000/hello?alt-server=127.0.0.1:10001";
for (int i = 2; i < serverInstances; i++)
{
    helloServiceAddress += $"&alt-server=127.0.0.1:{10000 + i}";
}
var hello = new HelloProxy(pipeline, new Uri(helloServiceAddress));

Console.Write("To say hello to the server, type your name: ");

CancellationToken cancel = cancellationSource.Token;
if (Console.ReadLine() is string name)
{
    try
    {
        while (true)
        {
            string helloResponse = await hello.SayHelloAsync(name, cancel: cancel);
            Console.WriteLine($"Server says: {helloResponse}");
            Console.WriteLine("Looping in 1 second, press Ctrl+C to exit");
            await Task.Delay(TimeSpan.FromSeconds(1), cancel);
        }
    }
    catch (DispatchException ex)
    {
        // The request failed because we reached the allowed max attempts or because all server addresses were excluded due
        // to the failure retry policy.
        Console.WriteLine(ex);
    }
    catch (OperationCanceledException)
    {
    }
}
