using System.Collections.Concurrent;
using System.Net.Http.Headers;
using SignalRDrawingServer.Hubs;
using SignalRIntroduction;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<MathAlgorithms>();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapHub<GreetingHub>("/hub");

var sessions = new ConcurrentDictionary<string, bool>();

app.MapGet("/fib-sse/{session}", async (string session, HttpContext ctx, CancellationToken cancellationToken, MathAlgorithms math, int? from, int? to) =>
{
    if (!sessions.TryAdd(session, true))
    {
        ctx.Response.StatusCode = StatusCodes.Status204NoContent;
        return;
    }

    ctx.Response.Headers.Append("Content-Type", "text/event-stream");
    foreach (var current in math.GetFibonacci())
    {
        if (current < (from ?? 0))
        {
            // Fibonacci number is too low
            continue;
        }
        else if (current <= (to ?? 100))
        {
            await ctx.Response.WriteAsync($"data: {current}\n\n", cancellationToken);
            await ctx.Response.Body.FlushAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await Task.Delay(250);
        }
        else
        {
            // Fibonacci number is too high
            break;
        }
    }
});

app.Run();
