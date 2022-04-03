export type ActionRequest = KeyActionRequest | FillTokenActionRequest | PingActionRequest | SoundControlActionRequest;
export type ActionResponse = InitActionResponse | PongActionResponse;

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

export interface SoundControlActionRequest {
    action: "soundControl";
    soundControlAction: {
        mute: boolean;
    }
}

export interface InitActionResponse {
    action: "init";
    initAction: {
        codec: string;
    };
}

export interface PongActionResponse {
    action: "pong";
    pongAction: {
        madeAt: number;
    };
}
