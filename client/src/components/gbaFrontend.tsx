import * as React from "react";

interface GbaFrontendState {
    screenSource: string;
    fps: number;
    worstFrameLatency: number;
    status: "shutdown" | "disconnected" | "connecting" | "connected"
}

// In milliseconds
const RETRY_TIMEOUT_MIN: number = 1000;
const RETRY_TIMEOUT_MAX: number = 60000;
const FPS_REFRESH_INTERVAL: number = 1000;

export class GbaFrontend extends React.Component<{}, GbaFrontendState> {

    private unloadHandler?: (e: BeforeUnloadEvent) => void = undefined;
    private ws?: WebSocket = undefined;
    private timerId: ReturnType<typeof setTimeout> | undefined = undefined;

    private retryTimeout: number = RETRY_TIMEOUT_MIN;

    private frameCounter: number = 0;

    private lastFrameArrived: number = NaN;
    private worstFrameLatencyInWindow: number = 0;

    constructor({}) {
        super({});
        this.state = {
            screenSource: "./screen.png",
            fps: 0,
            worstFrameLatency: NaN,
            status: "shutdown"
        };
    }

    public render(): React.ReactNode {
        return <div id="console-container">
            <img className="console-body" src="./images/consoleBody.png" />
            <img className="console-body" src="./images/innerLogo.png" />
            <img className="console-body" style={this.indicatorStyle} src="./images/consoleIndicator.png" />
            <img className="console-screen" src={this.state.screenSource} />
            <div className="console-status">
                <span>{this.renderedFps} | {this.renderedWorstFrameLatency}</span>
            </div>
        </div>;
    }

    public componentDidMount(): void {
        this.ws = this.initiateInterfaceCommunication();

        this.unloadHandler = () => {
            this.ws?.close();
        };
        window.addEventListener("beforeunload", this.unloadHandler);

        this.timerId = setInterval(() => {
            let reliableWorstFrameLatency: number = this.worstFrameLatencyInWindow;
            if (isNaN(this.lastFrameArrived) || this.lastFrameArrived < performance.now() - 1000) {
                reliableWorstFrameLatency = NaN;
                this.lastFrameArrived = NaN; // To prevent polluting the next valuable window
            }
            this.setState({
                fps: this.frameCounter / FPS_REFRESH_INTERVAL * 1000,
                worstFrameLatency: reliableWorstFrameLatency
            });
            this.frameCounter = 0;
            this.worstFrameLatencyInWindow = 0;
        }, FPS_REFRESH_INTERVAL);
    }

    public componentWillUnmount(): void {
        if (this.unloadHandler) {
            window.removeEventListener("beforeunload", this.unloadHandler);
        }

        if (this.timerId) {
            clearInterval(this.timerId);
        }
    }

    private initiateInterfaceCommunication(): WebSocket {
        console.log("Initiating interface communication...");

        const ws = new WebSocket(`${location.href.replace(/^http/, "ws")}consoleInterface.sock`);
        this.setState({
            status: "connecting"
        });

        ws.binaryType = "blob";

        ws.addEventListener("open", () => {
            this.setState({
                status: "connected"
            });
            this.retryTimeout = RETRY_TIMEOUT_MIN;
        });

        ws.addEventListener("message", (e: MessageEvent<Blob>) => {
            this.setState({
                screenSource: URL.createObjectURL(e.data)
            });

            this.frameCounter++;

            const frameArrived = performance.now();
            if (this.lastFrameArrived) {
                const frameLatency = frameArrived - this.lastFrameArrived;
                if (frameLatency > this.worstFrameLatencyInWindow) {
                    this.worstFrameLatencyInWindow = frameLatency;
                }
            }
            this.lastFrameArrived = frameArrived;
        });

        const reconnect = () => {
            console.error("Reconnecting");

            this.setState({
                status: "disconnected"
            });

            setTimeout(() => {
                this.ws = this.initiateInterfaceCommunication();
            }, this.retryTimeout);

            this.retryTimeout = Math.min(this.retryTimeout * 2, RETRY_TIMEOUT_MAX);
        };

        ws.addEventListener("close", reconnect);
        ws.addEventListener("error", reconnect);

        return ws;
    }

    private get indicatorStyle(): React.CSSProperties | undefined
    {
        switch (this.state.status) {
            case "shutdown":
                return {
                    filter: "grayscale(100%)"
                };
            case "disconnected":
                return {
                    filter: "hue-rotate(270deg)"
                };
            case "connecting":
                return {
                    filter: "hue-rotate(315deg)"
                };
            case "connected":
                return undefined;
        }
    }

    private get renderedFps(): string {
        return `FPS: ${this.state.fps.toString(10).padStart(2, "0")}`;
    }

    private get renderedWorstFrameLatency(): string {
        return `Worst Frame Latency: ${this.renderedWorstFrameLatencyContent.padEnd(12)}`;
    }

    private get renderedWorstFrameLatencyContent(): string {
        if (isNaN(this.state.worstFrameLatency)) {
            return "NO DATA";
        } else {
            const breachedPct = (this.state.worstFrameLatency / 1000 * this.state.fps - 1) * 100;
            return `${this.state.worstFrameLatency.toFixed(0)}ms / ${breachedPct > 0 ? "+" : "-"}${Math.abs(breachedPct).toFixed(0)}%`;
        }
    }
}
