export declare class Decoder {
    constructor(options: { rgb: boolean });

    decode(data: ArrayBufferLike, info: void): void;

    onPictureDecoded(width: number, height: number, croppingParams: null, data: ArrayBufferLike): void;
}
