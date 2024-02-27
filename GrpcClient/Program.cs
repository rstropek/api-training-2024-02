using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo;

AppContext.SetSwitch(
    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var httpClient = new HttpClient { DefaultRequestVersion = new Version(2, 0) };
var channel = GrpcChannel.ForAddress("http://localhost:5001", new GrpcChannelOptions { HttpClient = httpClient });

var client = new Greeter.GreeterClient(channel);
var reply = await client.SayHelloAsync(new HelloRequest { Name = "World" });
Console.WriteLine("Greeting: " + reply.Message);

var mathClient = new MathGuru.MathGuruClient(channel);
using var stream = mathClient.GetFibonacci(new FromTo { From = 0, To = 100 });
await foreach (var current in stream.ResponseStream.ReadAllAsync())
{
    Console.WriteLine(current.Result);
}

using var duplexStream = mathClient.GetFibonacciStepByStep();

// Note that client processes streamed results from server asynchronously
var receiverTask = Task.Run(async () =>
{
    // Note that this task will end when the server has closed the response stream
    await foreach (var fib in duplexStream.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine(fib.Result);

        if (fib.Result == 34)
        {
            await duplexStream.RequestStream.CompleteAsync();
            break;
        }
    }
});

// 1, 1, 2, 3, 5, 8, 13, 21, 34, 55
await duplexStream.RequestStream.WriteAsync(new FromTo() { From = 1, To = 10 });

await Task.Delay(5000);

// Request a meaningful result
await duplexStream.RequestStream.WriteAsync(new FromTo() { From = 20, To = 40 });

await receiverTask;
