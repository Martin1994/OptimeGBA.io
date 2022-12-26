# OptimeGBA.io

This is a web server frontend of [OptimeGBA](https://github.com/Powerlated/OptimeGBA). Multiple players from browser may play on the same GBA simulator running on a server.

## How it works
OptimeGBA.io hosts an ASP.NET web server with a GBA simulator ([OptimeGBA](https://github.com/Powerlated/OptimeGBA)) running on it. Clients communicates with the simulator with WebSocket.

Video frames are transmitted in encoded video frames. There is a simple traffic control mechanism which caps 10 unacknowledged video frames per client. Each video
frames are flushed from emulator, encoded on server side, sent to each client, and rendered on client side as soon as possible. In other words, on client side video
frames are not rendered in a fixed rate like a normal video streaming, but dynamically based on when a frame arrives.

Audio frames are transmitted in raw 16 bit integer format with 2 channels. Audio buffers are transmitted every 256 samples (The sample rate of GBA is 32768 Hz).

## Native dependencies

OptimeGBA.io requires either libvpx or libopenh264 for video streaming.

### libvpx

The current native binding is written for libvpx v1.11.0 (or any version with compatible ABI). Usually you need to compile it from source: [https://chromium.googlesource.com/webm/libvpx/+/refs/tags/v1.11.0](https://chromium.googlesource.com/webm/libvpx/+/refs/tags/v1.11.0).

### libopenh264

The current native binding is written for libopenh264 v2.2.0 (or any version with compatible ABI). Because of the patent situation of H.264, it's recommended to use Cisco's precompiled binaries to avoid potential issues: [https://github.com/cisco/openh264/releases/tag/v2.2.0](https://github.com/cisco/openh264/releases/tag/v2.2.0).

### Configuration

Video encoder can be specified in app seetings, which can be configured either in `appsettings.json` (or `appsettings.Development.json` during development) under `.VideoEncoding`, or command line option `--VideoEncoding <encoding>`.

There are three supported options:
- `vp9`: VP9 encoding via libvpx.
- `h264highres`: H.264 encoding with enhanced image quality via libopenh264 (default)
- `h264`: H.264 encoding with standard image quality via libopenh264

## Development

Note: Currently OptimeGBA.io depends on a [forked branch of OptimeGBA](https://github.com/Martin1994/OptimeGBA/tree/corelib) which vends a library rather than an executable.

Run webpack dev server:

```shell
cd client
npm run dev
```

Run ASP.NET web server:

```shell
dotnet run -c Release --Optime:BiosHome=<dir-with-bios> --Optime:Rom=<path-to-rom> [--VideoEncoding="<encoding>"]
```

Local endpoint:
[http://127.0.0.1:5000](http://127.0.0.1:5000)


Publish:
```shell
dotnet publish -c Release
```
