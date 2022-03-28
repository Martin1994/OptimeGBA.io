export type ActionRequest = KeyActionRequest | FillTokenActionRequest | PingActionRequest;
export type ActionResponse = InitActionResponse | FrameActionResponse | PongActionResponse;

export type GbaKey = "A" | "B" | "L" | "R" | "select" | "start" | "left" | "right" | "up" | "down";
export type GbaKeyAction = "up" | "down";

export interface KeyActionRequest {
    action: "key";
    keyAction: {
        key: GbaKey;
        action: GbaKeyAction;
    }
}

export interface FillTokenActionRequest {
    action: "fillToken";
    fillTokenAction: {
        count: number;
    }
}

export interface PingActionRequest {
    action: "ping";
    pingAction: {
        madeAt: number;
    }
}

export interface InitActionResponse {
    action: "init";
    initAction: {
        codec: string;
    };
}

export type FrameType = "screen" | "sound";

export interface FrameActionResponse {
    action: "frame";
    frameAction: {
        type: FrameType;
    };
}

export interface PongActionResponse {
    action: "pong";
    pongAction: {
        madeAt: number;
    };
}
