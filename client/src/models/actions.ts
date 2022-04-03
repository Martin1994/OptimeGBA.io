export type ActionRequestArgs =
    ["k", KeyRequest] |
    ["t"] |
    ["p", PingRequest] |
    ["a", AudioControlRequest];
export type ActionResponse = InitResponse | PongResponse;

export type GbaKey = "A" | "B" | "L" | "R" | "select" | "start" | "left" | "right" | "up" | "down";
export type GbaKeyAction = "up" | "down";

export interface KeyRequest {
    key: GbaKey;
    action: GbaKeyAction;
}

export interface PingRequest {
    madeAt: number;
}

export interface AudioControlRequest {
    mute: boolean;
}

export interface InitResponse {
    action: "init";
    codec: string;
}

export interface PongResponse {
    action: "pong";
    madeAt: number;
}
