using System.Net;
using GrpcServer;
using GrpServer;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => 
{
    options.Listen(IPAddress.Any, 5001, o => o.Protocols = HttpProtocols.Http2);
    options.Listen(IPAddress.Any, 5002, o => o.Protocols = HttpProtocols.Http1);
});
builder.Services.AddSingleton<MathAlgorithms>();
builder.Services.AddGrpc();
builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));
var app = builder.Build();

app.UseCors();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.MapGrpcService<GreeterService>().RequireCors("AllowAll")/*.EnableGrpcWeb()*/;
app.MapGrpcService<MathGuruService>().RequireCors("AllowAll");

app.Run();
