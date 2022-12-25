import * as React from "react";
import { GbaKey, GbaKeyAction } from "../models/actions";
import { GbaStates } from "./gba";
import { GbaKeyHandler } from "./gbaKeyControl";

export type GbaMuteEvent = (mute: boolean) => void;

export interface GbaViewProps extends GbaStates {
    readonly onMute: GbaMuteEvent;
    readonly onKeyEvent: GbaKeyHandler;
}

export class GbaView extends React.PureComponent<GbaViewProps> {

    private readonly screenCanvasRef = React.createRef<HTMLCanvasElement>();
    private get screenDrawContext(): CanvasRenderingContext2D | null | undefined {
        return this.screenCanvasRef.current?.getContext("2d");
    }

    private readonly audioRef = React.createRef<HTMLAudioElement>();

    private decoder?: VideoDecoder;

    private audio?: AudioContext;
    private audioGenerator?: MediaStreamAudioTrackGenerator;
    private audioTimestamp: number = 0;

    constructor(props: GbaViewProps) {
        super(props);
    }

    /**
     * @override
     */
    public render(): React.ReactNode {
        if (!this.decoder && this.props.codec) {
            console.log(`Initializing decoder with codec ${this.props.codec}`);
            this.resetDecoder();
        }

        if (!this.props.mute && !this.audio) {
            void this.mountAudio();
        }

        const view = this;

        return (
            <div id="console-container">
                <img className="console-body" src="./images/consoleBody.png" />
                <img className="console-body" src="./images/innerLogo.png" />
                <img className="console-body" style={this.indicatorStyle} src="./images/consoleIndicator.png" />
                <canvas ref={this.screenCanvasRef} width={240} height={160} className="console-screen" />
                <audio ref={this.audioRef} />
                <div className="console-status">
                    <span>{`RTT: ${this.renderedRtt.padStart(6, "\u00A0")} | FPS: ${this.renderedFps.padStart(2, "\u00A0")} | Worst Frame Gap: ${this.renderedWorstFrameLatency.padStart(14, "\u00A0")}`}</span>
                </div>
                <MuteButton>{this.props.mute ? "\u{1F507}" : "\u{1F508}"}</MuteButton>
                <ControlButton gbaKey="A" binding="Z" />
                <ControlButton gbaKey="B" binding="X" />
                <ControlButton gbaKey="L" binding="A" />
                <ControlButton gbaKey="R" binding="S" />
                <ControlButton gbaKey="down" binding="DOWN" />
                <ControlButton gbaKey="up" binding="UP" />
                <ControlButton gbaKey="left" binding="LEFT" />
                <ControlButton gbaKey="right" binding="RIGHT" />
                <ControlButton gbaKey="select" binding="BACKSPACE" />
                <ControlButton gbaKey="start" binding="ENTER" />
            </div>
        );

        function ControlButton({ gbaKey, binding }: { gbaKey: GbaKey, binding: string }): React.ReactElement {
            function makeHandler(action: GbaKeyAction) {
                return (e: React.UIEvent) => {
                    view.props.onKeyEvent(gbaKey, action, false);
                }
            }

            const className = `console-control-button button-${gbaKey}`;
            const tooltip = `Key binding: ${binding}`;

            if (navigator.maxTouchPoints > 0) {
                return (
                    <div
                        className={className}
                        title={tooltip}
                        onContextMenu={e => e.preventDefault()}
                        onTouchStart={makeHandler("down")}
                        onTouchEnd={makeHandler("up")}
                    />
                );
            } else {
                return (
                    <div
                        className={className}
                        title={tooltip}
                        onMouseDown={makeHandler("down")}
                        onMouseUp={makeHandler("up")}
                    />
                );
            }
        };

        function MuteButton({ children }: { children: string }): React.ReactElement {
            return (
                <button
                    className="console-silent-button"
                    type="button"
                    onClick={() => setTimeout(() => view.props.onMute(!view.props.mute))}
                >
                    {children}
                </button>
            )
        }
    }

    private async mountAudio(): Promise<void> {
        this.audio = new AudioContext({
            latencyHint: "interactive"
        });

        this.audioGenerator = new MediaStreamTrackGenerator({
            kind: "audio"
        });

        this.audioTimestamp = 0;

        const audioStream = new MediaStream();
        audioStream.addTrack(this.audioGenerator);

        this.audio.createMediaStreamSource(audioStream).connect(this.audio.destination);
    }

    public renderVideoFrame(frame: ArrayBufferView): void {
        try {
            this.decoder?.decode(new EncodedVideoChunk({
                data: frame,
                type: "key",
                timestamp: 0
            }));
        } catch (e) {
            let skip: boolean = false;
            if (e instanceof DOMException) {
                console.warn("Decoder failed to decode a frame.", e);
                if (e.name === "DataError") {
                    skip = true;
                } else if (e.code === DOMException.INVALID_STATE_ERR) {
                    this.resetDecoder();
                    skip = true;
                }
            }
            if (!skip) {
                throw e;
            }
        }
    }

    public async flushAudioFrame(frame: ArrayBufferView): Promise<void> {
        const sampleRate = 32768;
        const channels = 2;
        const buffer = new Int16Array(frame.buffer, frame.byteOffset, frame.byteLength >> 1);

        if (!this.audioGenerator) {
            return;
        }

        const writer = this.audioGenerator.writable.getWriter();
        await writer.write(new AudioData({
            format: "s16",
            numberOfChannels: channels,
            numberOfFrames: buffer.length / channels,
            sampleRate,
            timestamp: this.audioTimestamp,
            data: buffer
        }));
        writer.releaseLock();

        this.audioTimestamp += buffer.length / channels / sampleRate * 1000000;
    }

    private resetDecoder(): void {
        this.decoder = new VideoDecoder({
            error: err => console.error("Decoder threw an error.", err),
            output: frame => {
                this.screenDrawContext?.drawImage(frame, 0, 0, 240, 160);
                frame.close();
            }
        });

        if (!this.props.codec) {
            return;
        }

        this.decoder.configure({
            codec: this.props.codec
        });
    }

    private get indicatorStyle(): React.CSSProperties | undefined
    {
        switch (this.props.status) {
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
        if (!isFinite(this.props.rtt)) {
            return "N/A";
        }

        return `${this.props.rtt.toFixed(0)}ms`;
    }

    private get renderedFps(): string {
        return Math.min(this.props.fps, 99).toString(10).padStart(2, "0");
    }

    private get renderedWorstFrameLatency(): string {
        if (!isFinite(this.props.worstFrameLatency)) {
            return "NO DATA";
        }

        const breachedPct = (this.props.worstFrameLatency / 1000 * this.props.fps - 1) * 100;
        return `${this.props.worstFrameLatency.toFixed(0)}ms / ${breachedPct > 0 ? "+" : "-"}${Math.min(Math.abs(breachedPct), 999).toFixed(0)}%`;
    }
}
