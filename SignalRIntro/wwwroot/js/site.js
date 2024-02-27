/* eslint-disable */

const result = document.getElementById("result");
const serverGreeter = document.getElementById("sayHelloToServer");
const fib = document.getElementById("fib");
const fibSse = document.getElementById("fib-sse");
const { fromEvent, Subject } = rxjs;

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5025/hub")
    .configureLogging(signalR.LogLevel.Debug)
    .build();

(async () => {
    // Handle sayHello message
    connection.on('sayHello', (message) => {
        result.innerHTML = result.innerHTML + `Server said hello with message "${message}"<br/>`;
    });

    // Start SignalR connection
    await connection.start();
    console.log('We are connected :-)');
    serverGreeter.hidden = false;
})();

function sayHelloToServer() {
    connection.invoke("SayHello", "Hello from Client");
}

function startStreamingNumbers() {
    connection.stream("GetFibonacci", 0, 100)
        .subscribe({
            next: (number) => {
                fib.innerHTML = fib.innerHTML + `${number}<br/>`;
            },
            complete: () => {
                fib.innerHTML = fib.innerHTML + "done.<br/>";
            },
            error: (err) => {
                fib.innerHTML = fib.innerHTML + `Ups, "${err}"<br/>`;
            },
        });
}

function startFibSse() {
    const session = Math.floor(Math.random() * 100000);
    const eventSource = new EventSource(`http://localhost:5025/fib-sse/${session}?from=0&to=200`);
    eventSource.onmessage = (e) => {
        fibSse.innerHTML = fibSse.innerHTML + `${e.data}<br/>`;
    };
}