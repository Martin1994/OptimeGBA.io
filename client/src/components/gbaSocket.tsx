import * as React from "react";
import { ActionRequest, ActionResponse, FrameType } from "../models/actions";
import { GbaSocketStatus } from "./gba";

const RETRY_TIMEOUT_MIN_MS: number = 1000;
const RETRY_TIMEOUT_MAX_MS: number = 60000;

export type GbaSocketStatusEvent = (state: GbaSocketStatus) => void;
export type GbaFrameEvent = (frame: ArrayBuffer) => void;
export type GbaResponseEvent = (message: ActionResponse) => void;

export interface GbaSocketProps {
    readonly onStatus: GbaSocketStatusEvent;
    readonly onScreenFrame: GbaFrameEvent;
    readonly onSoundFrame: GbaFrameEvent;
    readonly onMessageEvent: GbaResponseEvent;
}

export class GbaSocket extends React.PureComponent<GbaSocketProps> {
    private retryTimeout: number = RETRY_TIMEOUT_MIN_MS;

    private unloadHandler?: (e: BeforeUnloadEvent) => void = undefined;
    private ws?: WebSocket = undefined;
    private nextFrameType: FrameType = "screen";

    /**
     * @overrides
     */
    public render(): React.ReactNode {
        return null;
    }

    /**
     * @override
     */
    public componentDidMount(): void {
        this.ws = this.initiateInterfaceCommunication();

        this.unloadHandler = () => {
            this.ws?.close();
        };
        window.addEventListener("beforeunload", this.unloadHandler);
    }

    /**
     * @override
     */
    public componentWillUnmount(): void {
        if (this.unloadHandler) {
            window.removeEventListener("beforeunload", this.unloadHandler);
        }
    }

    private initiateInterfaceCommunication(): WebSocket {
        console.log("Initiating interface connection...");

        const ws = new WebSocket(`${location.href.substring(0, window.location.href.lastIndexOf("/") + 1).replace(/^http/, "ws")}consoleInterface.sock`);
        this.props.onStatus("connecting");

        ws.binaryType = "arraybuffer";

        ws.addEventListener("open", () => {
            console.log("Interface connection established.");
            this.props.onStatus("connected");
            this.retryTimeout = RETRY_TIMEOUT_MIN_MS;
        });

        ws.addEventListener("message", (e: MessageEvent<ArrayBuffer | string>) => this.handleMessage(e));

        const reconnect = (): void => {
            console.error("Reconnecting");

            this.props.onStatus("disconnected");

            setTimeout((): void => {
                this.ws = this.initiateInterfaceCommunication();
            }, this.retryTimeout);

            this.retryTimeout = Math.min(this.retryTimeout * 2, RETRY_TIMEOUT_MAX_MS);
        };

        ws.addEventListener("close", reconnect);

        return ws;
    }

    public sendAction(request: ActionRequest): void {
        if (!this.ws) {
            console.warn("No connection. Action will be discarded.");
            return;
        }

        this.ws.send(JSON.stringify(request));
    }

    private handleMessage(e: MessageEvent<ArrayBuffer | string>): void {
        if (e.data instanceof ArrayBuffer) {
            if (this.nextFrameType === "screen") {
                this.props.onScreenFrame(e.data);
            } else {
                this.props.onSoundFrame(e.data);
            }
            return;
        }

        const message: ActionResponse = JSON.parse(e.data) as ActionResponse;

        if (message.action === "frame") {
            this.nextFrameType = message.frameAction.type;
            return;
        }

        this.props.onMessageEvent(message);
    }
}
