import "./index.css";
import { HelloRequest } from "./greet_pb";
import { GreeterClient } from './GreetServiceClientPb';

(async() => {
    const client = new GreeterClient('http://localhost:5002');
    const request = new HelloRequest();
    request.setName('World');
    const message = await client.sayHello(request, {});
    console.log(`Greeting: ${message.getMessage()}`);
})();
