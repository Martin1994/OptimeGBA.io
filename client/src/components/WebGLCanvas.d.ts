export declare class H264bsdCanvas {
    constructor(canvas: HTMLCanvasElement, forceNoGL: boolean, contextOptions: WebGLContextAttributes);

    drawNextOutputPicture(width: number, height: number, croppingParams: null, data: ArrayBufferLike): void;
}
