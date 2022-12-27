import * as React from "react";
import { ActionResponse, GbaKey, GbaKeyAction } from "../models/actions";
import { GbaView } from "./gbaView";
import { GbaSocket } from "./gbaSocket";

const FPS_REFRESH_INTERVAL_MS: number = 1000;

export type GbaSocketStatus = "shutdown" | "disconnected" | "connecting" | "connected";

export interface GbaStates {
    readonly codec: string;
    readonly rtt: number;
    readonly fps: number;
    readonly worstFrameLatency: number;
    readonly status: GbaSocketStatus;
    readonly mute: boolean;
}

export interface GbaProps {
}

export class Gba extends React.PureComponent<GbaProps, GbaStates> {
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

    // Cache handlers to avoid rerendering
    private readonly keyActionHandler = this.sendKeyAction.bind(this);
    private readonly muteEventHandler = (mute: boolean) => setTimeout(() => this.handleMuteEvent(mute));
    private readonly statusHandler = (status: GbaSocketStatus) => this.setState({ status });
    private readonly videoFrameHandler = this.handleVideoFrame.bind(this);
    private readonly audioFrameHandler = this.handleAudioFrame.bind(this);
    private readonly messageHandler = this.handleMessage.bind(this);

    public constructor(props: GbaProps) {
        super(props);
        this.state = {
            codec: "",
            rtt: 0,
            fps: 0,
            worstFrameLatency: 0,
            status: "shutdown",
            mute: true
        };
    }

    /**
     * @overrides
     */
    public render(): React.ReactNode {
        return (
            <>
                <GbaView ref={this.viewRef} {...this.state}
                    onMute={this.muteEventHandler}
                    onKeyEvent={this.keyActionHandler}
                />
                <GbaSocket ref={this.socketRef}
                    onStatus={this.statusHandler}
                    onVideoFrame={this.videoFrameHandler}
                    onAudioFrame={this.audioFrameHandler}
                    onMessageEvent={this.messageHandler}
                />
            </>
        );
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

    private handleVideoFrame(frame: ArrayBufferView): void {
        this.view?.renderVideoFrame(frame);

        this.frameCounter++;

        const frameArrived = performance.now();
        if (this.lastFrameArrived) {
            const frameLatency = frameArrived - this.lastFrameArrived;
            if (frameLatency > this.worstFrameLatencyInWindow) {
                this.worstFrameLatencyInWindow = frameLatency;
            }
        }
        this.lastFrameArrived = frameArrived;

        this.socket?.sendAction("t");
    }

    private handleAudioFrame(frame: ArrayBufferView): void {
        void this.view?.flushAudioFrame(frame);
    }

    private handleMessage(message: ActionResponse): void {
        switch (message.action) {
            case "pong":
                this.setState({
                    rtt: performance.now() - message.madeAt
                });
                break;

            case "init":
                this.setState({
                    codec: message.codec
                });
                break;

            default:
                // No op
                break;
        }
    }

    private sendKeyAction(key: GbaKey, action: GbaKeyAction, repeat: boolean): void {
        if (repeat) {
            return;
        }
        this.socket?.sendAction("k", { key, action });
        this.ping();
    }

    private handleMuteEvent(mute: boolean): void {
        this.setState({ mute });

        this.socket?.sendAction("a", { mute });
    }

    public ping(): void {
        this.socket?.sendAction("p", { madeAt: performance.now() });
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
