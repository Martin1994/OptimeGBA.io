import * as React from "react";
import { ActionResponse, GbaKey, GbaKeyAction, KeyActionRequest } from "../models/actions";
import { GbaView } from "./gbaView";
import { GbaSocket } from "./gbaSocket";
import { GbaKeyControl } from "./gbaKeyControl";

const FPS_REFRESH_INTERVAL_MS: number = 1000;

export interface GbaStates {
    readonly rtt: number;
    readonly fps: number;
    readonly worstFrameLatency: number;
    readonly status: "shutdown" | "disconnected" | "connecting" | "connected";
}

export interface GbaProps {
}

export class Gba extends React.Component<GbaProps, GbaStates> {
    private timerId: ReturnType<typeof setTimeout> | undefined = undefined;

    private frameCounter: number = 0;

    private lastFrameArrived: number = NaN;
    private worstFrameLatencyInWindow: number = 0;

    private readonly viewRef = React.createRef<GbaView>();
    private get view(): GbaView | null {
        return this.viewRef.current;
    }

    private readonly socketRef = React.createRef<GbaSocket>();
    private get socket(): GbaSocket | null {
        return this.socketRef.current;
    }

    /**
     * @overrides
     */
    public render(): React.ReactNode {
        return <React.Fragment>
            <GbaView ref={this.viewRef} {...this.state} />
            <GbaKeyControl gba={this} />
            <GbaSocket ref={this.socketRef} gba={this} />
        </React.Fragment>;
    }

    /**
     * @override
     */
    public componentDidMount(): void {
        this.timerId = setInterval(() => this.refreshStatus(), FPS_REFRESH_INTERVAL_MS);
    }

    /**
     * @override
     */
    public componentWillUnmount(): void {
        if (this.timerId) {
            clearInterval(this.timerId);
        }
    }

    public handleFrame(frame: ArrayBuffer): void {
        this.view?.renderScreenFrame(frame);

        this.frameCounter++;

        const frameArrived = performance.now();
        if (this.lastFrameArrived) {
            const frameLatency = frameArrived - this.lastFrameArrived;
            if (frameLatency > this.worstFrameLatencyInWindow) {
                this.worstFrameLatencyInWindow = frameLatency;
            }
        }
        this.lastFrameArrived = frameArrived;

        this.socket?.sendAction({
            action: "fillToken",
            fillTokenAction: {
                count: 1
            }
        });
    }

    public handleMessage(message: ActionResponse): void {
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

    public sendKeyAction(key: GbaKey, action: GbaKeyAction, repeat: boolean): void {
        if (repeat) {
            return;
        }
        const request: KeyActionRequest = {
            action: "key",
            keyAction: { key, action }
        };
        this.socket?.sendAction(request);
        this.ping();
    }

    public ping(): void {
        this.socket?.sendAction({
            action: "ping",
            pingAction: {
                madeAt: performance.now()
            }
        });
    }

    private refreshStatus(): void {
        let reliableWorstFrameLatency: number = this.worstFrameLatencyInWindow;
        if (isNaN(this.lastFrameArrived) || this.lastFrameArrived < performance.now() - 1000) {
            reliableWorstFrameLatency = NaN;
            this.lastFrameArrived = NaN; // To prevent polluting the next valuable window
        }
        this.setState({
            fps: this.frameCounter / FPS_REFRESH_INTERVAL_MS * 1000,
            worstFrameLatency: reliableWorstFrameLatency
        });
        this.frameCounter = 0;
        this.worstFrameLatencyInWindow = 0;

        this.ping();
    }
}
