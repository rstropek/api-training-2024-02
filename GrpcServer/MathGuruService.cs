using Grpc.Core;
using GrpcDemo;
using GrpcServer;

namespace GrpServer;

public class MathGuruService(ILogger<MathGuruService> logger, MathAlgorithms math) : MathGuru.MathGuruBase
{
    public override async Task GetFibonacci(FromTo request, IServerStreamWriter<NumericResult> responseStream, ServerCallContext context)
    {
        if (request.From > request.To)
        {
            context.Status = new Status(StatusCode.InvalidArgument, "From must be less than To");
            return;
        }

        foreach (var current in math.GetFibonacci())
        {
            if (current < request.From)
            {
                // Fibonacci number is too low
                continue;
            }
            else if (current <= request.To)
            {
                await responseStream.WriteAsync(new NumericResult { Result = current });
                await Task.Delay(250);
            }
            else
            {
                // Fibonacci number is too high
                break;
            }
        }
    }

    public override async Task GetFibonacciStepByStep(IAsyncStreamReader<FromTo> requestStream, IServerStreamWriter<NumericResult> responseStream, ServerCallContext context)
    {
        var fib = math.GetFibonacci().GetEnumerator();

        // Read requests from client
        await foreach (var item in requestStream.ReadAllAsync())
        {
            // Calculate Fibi
            while (fib.Current < item.From)
            {
                fib.MoveNext();
            }

            if (fib.Current > item.To)
            {
                continue;
            }

            while (fib.Current <= item.To)
            {
                await responseStream.WriteAsync(new NumericResult { Result = fib.Current });
                fib.MoveNext();
                await Task.Delay(250);
            }
        }
    }
}