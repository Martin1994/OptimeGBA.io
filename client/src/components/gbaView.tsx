import * as React from "react";

export interface GbaViewProps {
    readonly rtt: number;
    readonly fps: number;
    readonly worstFrameLatency: number;
    readonly status: "shutdown" | "disconnected" | "connecting" | "connected";
}

export class GbaView extends React.Component<GbaViewProps> {

    private readonly screenCanvasRef = React.createRef<HTMLCanvasElement>();
    private get screenDrawContext(): CanvasRenderingContext2D | null | undefined {
        return this.screenCanvasRef.current?.getContext("2d");
    }

    private decoder: VideoDecoder;

    constructor(props: GbaViewProps) {
        super(props);
        this.state = {
            rtt: NaN,
            fps: 0,
            worstFrameLatency: NaN,
            status: "shutdown"
        };
        this.decoder = this.createDecoder();
    }

    /**
     * @override
     */
    public render(): React.ReactNode {
        return <div id="console-container">
            <img className="console-body" src="./images/consoleBody.png" />
            <img className="console-body" src="./images/innerLogo.png" />
            <img className="console-body" style={this.indicatorStyle} src="./images/consoleIndicator.png" />
            <canvas ref={this.screenCanvasRef} width={240} height={160} className="console-screen" />
            <div className="console-status">
                <span>{`RTT: ${this.renderedRtt.padStart(6, "\u00A0")} | FPS: ${this.renderedFps.padStart(2, "\u00A0")} | Worst Frame Gap: ${this.renderedWorstFrameLatency.padStart(14, "\u00A0")}`}</span>
            </div>
        </div>;
    }

    public renderScreenFrame(frame: ArrayBuffer): void {
        try {
            this.decoder.decode(new EncodedVideoChunk({
                data: frame,
                type: "key",
                timestamp: 0
            }));
        } catch (e) {
            let skip: boolean = false;
            if (e instanceof DOMException) {
                if (e.name === "DataError") {
                    skip = true;
                } else if (e.code === DOMException.INVALID_STATE_ERR) {
                    this.decoder = this.createDecoder();
                    skip = true;
                }
            }
            if (!skip) {
                throw e;
            }
        }
    }

    private createDecoder(): VideoDecoder {
        const decoder = new VideoDecoder({
            error: err => console.error(err),
            output: frame => {
                this.screenDrawContext?.drawImage(frame, 0, 0);
                frame.close();
            }
        });

        decoder.configure({
            codec: "vp09.01.10.08.03"
        });

        return decoder;
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
        if (isNaN(this.props.worstFrameLatency)) {
            return "NO DATA";
        }

        const breachedPct = (this.props.worstFrameLatency / 1000 * this.props.fps - 1) * 100;
        return `${this.props.worstFrameLatency.toFixed(0)}ms / ${breachedPct > 0 ? "+" : "-"}${Math.min(Math.abs(breachedPct), 999).toFixed(0)}%`;
    }
}
