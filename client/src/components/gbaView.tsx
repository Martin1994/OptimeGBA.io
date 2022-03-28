import * as React from "react";
import { GbaStates } from "./gba";

export type GbaSilentEvent = (silent: boolean) => void;

export interface GbaViewProps extends GbaStates {
    onSilent: GbaSilentEvent;
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

        if (this.props.silent) {
            //this.audio?.suspend();
        } else {
            if (this.audio) {
                //this.audio.resume();
            } else {
                void this.mountAudio();
            }
        }

        return <div id="console-container">
            <img className="console-body" src="./images/consoleBody.png" />
            <img className="console-body" src="./images/innerLogo.png" />
            <img className="console-body" style={this.indicatorStyle} src="./images/consoleIndicator.png" />
            <canvas ref={this.screenCanvasRef} width={240} height={160} className="console-screen" />
            <audio ref={this.audioRef} />
            <div className="console-status">
                <span>{`RTT: ${this.renderedRtt.padStart(6, "\u00A0")} | FPS: ${this.renderedFps.padStart(2, "\u00A0")} | Worst Frame Gap: ${this.renderedWorstFrameLatency.padStart(14, "\u00A0")}`}</span>
            </div>
            <button className="console-silent-button" onClick={() => setTimeout(() => this.props.onSilent(!this.props.silent))}>{this.props.silent ? "\u{1F507}" : "\u{1F508}"}</button>
        </div>;
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

    public renderScreenFrame(frame: ArrayBuffer): void {
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

    public async flushSoundFrame(frame: ArrayBuffer): Promise<void> {
        const sampleRate = 32768;
        const channels = 2;
        const buffer = new Int16Array(frame);

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
            data: frame
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
