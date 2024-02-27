import * as WebSockets from 'ws';

const wss = new WebSockets.Server({ port: 3000 });

wss.on('connection', (ws) => {
    ws.on('message', (message) => {
        console.log('Received:', message);
        ws.send(`You sent: ${message}`);
    });
    ws.send('Welcome to the server!');
});

function broadcast(data: string) {
    wss.clients.forEach((client) => {
        if (client.readyState === WebSockets.OPEN) {
            client.send(data);
        }
    });
}

let i = 0;
setInterval(() => broadcast(`Message ${i++}`), 1000);
