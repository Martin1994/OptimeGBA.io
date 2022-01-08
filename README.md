# OptimeGBA.io

This is a web server frontend of [OptimeGBA](https://github.com/Powerlated/OptimeGBA). Multiple players from browser may play on the same GBA simulator running on a server.

## How it works
OptimeGBA.io hosts an ASP.NET web server with a GBA simulator ([OptimeGBA](https://github.com/Powerlated/OptimeGBA)) running on it. Clients communicates with the simulator with WebSocket.

Screen frames are transmitted in image format frame by frame. Clients render a frame as soon as it arrives. There is a simple traffic control mechanism which limits the unacknowledged screen frames per client to be at most 10.

Audio is not supported at this moment.

## Development

Note: Currently OptimeGBA.io depends on a private branch of [OptimeGBA](https://github.com/Powerlated/OptimeGBA) which vends a library rather than an executable.

Run webpack dev server:

```shell
cd client
npm run dev
```

Run ASP.NET web server:

```shell
dotnet run -c Release
```

Local endpoint:
[http://127.0.0.1:5000](http://127.0.0.1:5000)


Publish:
```shell
dotnet publish -c Release
```
