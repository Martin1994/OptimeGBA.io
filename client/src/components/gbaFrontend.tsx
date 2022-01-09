import * as React from "react";
import { ActionRequest, ActionResponse, GbaKey, GbaKeyAction, KeyActionRequest } from "../models/actions";
import { Decoder } from "./Decoder";
import { H264bsdCanvas } from "./WebGlCanvas";

interface GbaFrontendState {
    screenBuffer?: Blob;
    rtt: number;
    fps: number;
    worstFrameLatency: number;
    status: "shutdown" | "disconnected" | "connecting" | "connected";
}

// In milliseconds
const RETRY_TIMEOUT_MIN: number = 1000;
const RETRY_TIMEOUT_MAX: number = 60000;
const FPS_REFRESH_INTERVAL: number = 1000;

export class GbaFrontend extends React.Component<{}, GbaFrontendState> {

    private unloadHandler?: (e: BeforeUnloadEvent) => void = undefined;
    private keyDownHandler?: (e: KeyboardEvent) => void = undefined;
    private keyUpHandler?: (e: KeyboardEvent) => void = undefined;
    private ws?: WebSocket = undefined;
    private timerId: ReturnType<typeof setTimeout> | undefined = undefined;

    private retryTimeout: number = RETRY_TIMEOUT_MIN;

    private frameCounter: number = 0;

    private lastFrameArrived: number = NaN;
    private worstFrameLatencyInWindow: number = 0;

    private currentScreenBlobUrl?: string;

    private screenCanvasRef: React.RefObject<HTMLCanvasElement> = React.createRef();
    private screenCanvas?: H264bsdCanvas;
    private decoder: Decoder = new Decoder({ rgb: false });

    constructor(props: {}) {
        super(props);
        this.state = {
            screenBuffer: undefined,
            rtt: NaN,
            fps: 0,
            worstFrameLatency: NaN,
            status: "shutdown"
        };
    }

    public render(): React.ReactNode {
        if (this.currentScreenBlobUrl) {
            URL.revokeObjectURL(this.currentScreenBlobUrl);
        }
        let screenSource = "about:blank";
        if (this.state.screenBuffer) {
            screenSource = URL.createObjectURL(this.state.screenBuffer);
            this.currentScreenBlobUrl = screenSource;
        }

        return <div id="console-container">
            <img className="console-body" src="./images/consoleBody.png" />
            <img className="console-body" src="./images/innerLogo.png" />
            <img className="console-body" style={this.indicatorStyle} src="./images/consoleIndicator.png" />
            <canvas ref={this.screenCanvasRef} className="console-screen" />
            <div className="console-status">
                <span>{`RTT: ${this.renderedRtt.padStart(6, "\u00A0")} | FPS: ${this.renderedFps.padStart(2, "\u00A0")} | Worst Frame Gap: ${this.renderedWorstFrameLatency.padStart(14, "\u00A0")}`}</span>
            </div>
        </div>;
    }

    public componentDidMount(): void {
        this.ws = this.initiateInterfaceCommunication();

        this.unloadHandler = () => {
            this.ws?.close();
        };
        window.addEventListener("beforeunload", this.unloadHandler);

        this.keyDownHandler = (e: KeyboardEvent) => {
            if (this.mapKeyAction(e.code, "down", e.repeat)) {
                e.preventDefault();
            }
        };
        window.addEventListener("keydown", this.keyDownHandler)

        this.keyUpHandler = (e: KeyboardEvent) => {
            if (this.mapKeyAction(e.code, "up", e.repeat)) {
                e.preventDefault();
            }
        };
        window.addEventListener("keyup", this.keyUpHandler)

        this.timerId = setInterval(() => this.refreshStatus(), FPS_REFRESH_INTERVAL);

        if (this.screenCanvasRef.current == null) {
            throw new Error("Canvas is not initialized!");
        }
        this.screenCanvas = new H264bsdCanvas(this.screenCanvasRef.current, false, {});
        this.decoder.onPictureDecoded = (width, height, _crop, data) => {
            console.log("onDecoded");
            this.screenCanvas?.drawNextOutputPicture(width, height, null, data);
        };
    }

    public componentWillUnmount(): void {
        if (this.unloadHandler) {
            window.removeEventListener("beforeunload", this.unloadHandler);
        }

        if (this.keyDownHandler) {
            window.removeEventListener("keydown", this.keyDownHandler);
        }

        if (this.keyUpHandler) {
            window.removeEventListener("keyup", this.keyUpHandler);
        }

        if (this.timerId) {
            clearInterval(this.timerId);
        }
    }

    private refreshStatus() {
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

        this.ping();
    }

    private handleMessage(e: MessageEvent<ArrayBuffer | string>) {
        if (e.data instanceof ArrayBuffer) {
            this.handleFrame(e.data);
            return;
        }

        const message: ActionResponse = JSON.parse(e.data);
        switch (message.action) {
            case "pong":
                this.setState({
                    rtt: performance.now() - message.pongAction.madeAt
                });
                break;

            default:
                // No op
                break;
        }
    }

    private handleFrame(frame: ArrayBuffer) {
        console.log("onFrame - size: " + frame.byteLength);
        this.decoder.decode(frame);

        this.frameCounter++;

        const frameArrived = performance.now();
        if (this.lastFrameArrived) {
            const frameLatency = frameArrived - this.lastFrameArrived;
            if (frameLatency > this.worstFrameLatencyInWindow) {
                this.worstFrameLatencyInWindow = frameLatency;
            }
        }
        this.lastFrameArrived = frameArrived;

        this.sendAction({
            action: "fillToken",
            fillTokenAction: {
                count: 1
            }
        });
    }

    private mapKeyAction(key: string, action: GbaKeyAction, repeat: boolean): boolean {
        switch (key) {
            case "KeyZ":
                this.sendKeyAction("A", action, repeat);
                break;

            case "KeyX":
                this.sendKeyAction("B", action, repeat);
                break;

            case "KeyA":
                this.sendKeyAction("L", action, repeat);
                break;

            case "KeyS":
                this.sendKeyAction("R", action, repeat);
                break;

            case "Backspace":
                this.sendKeyAction("select", action, repeat);
                break;

            case "Enter":
                this.sendKeyAction("start", action, repeat);
                break;

            case "ArrowLeft":
                this.sendKeyAction("left", action, repeat);
                break;

            case "ArrowRight":
                this.sendKeyAction("right", action, repeat);
                break;

            case "ArrowUp":
                this.sendKeyAction("up", action, repeat);
                break;

            case "ArrowDown":
                this.sendKeyAction("down", action, repeat);
                break;

            default:
                return false;
        }

        return true;
    }

    private sendKeyAction(key: GbaKey, action: GbaKeyAction, repeat: boolean) {
        if (repeat) {
            return;
        }
        const request: KeyActionRequest = {
            action: "key",
            keyAction: { key, action }
        };
        this.sendAction(request);
        this.ping();
    }

    private sendAction(request: ActionRequest) {
        if (!this.ws) {
            console.warn("No connection. Action will be discarded.");
            return;
        }

        this.ws.send(JSON.stringify(request));
    }

    private initiateInterfaceCommunication(): WebSocket {
        console.log("Initiating interface communication...");

        const ws = new WebSocket(`${location.href.substring(0, window.location.href.lastIndexOf("/") + 1).replace(/^http/, "ws")}consoleInterface.sock`);
        this.setState({
            status: "connecting"
        });

        ws.binaryType = "arraybuffer";

        ws.addEventListener("open", () => {
            this.setState({
                status: "connected"
            });
            this.retryTimeout = RETRY_TIMEOUT_MIN;
        });

        ws.addEventListener("message", (e: MessageEvent<ArrayBuffer | string>) => this.handleMessage(e));

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

        return ws;
    }

    private ping() {
        this.sendAction({
            action: "ping",
            pingAction: {
                madeAt: performance.now()
            }
        });
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

    private get renderedRtt(): string {
        if (isFinite(this.state.rtt)) {
            return `${this.state.rtt.toFixed(0)}ms`;
        } else {
            return "N/A"
        }
    }

    private get renderedFps(): string {
        return Math.min(this.state.fps, 99).toString(10).padStart(2, "0");
    }

    private get renderedWorstFrameLatency(): string {
        if (isNaN(this.state.worstFrameLatency)) {
            return "NO DATA";
        } else {
            const breachedPct = (this.state.worstFrameLatency / 1000 * this.state.fps - 1) * 100;
            return `${this.state.worstFrameLatency.toFixed(0)}ms / ${breachedPct > 0 ? "+" : "-"}${Math.min(Math.abs(breachedPct), 999).toFixed(0)}%`;
        }
    }
}
