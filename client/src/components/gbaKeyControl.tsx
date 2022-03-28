import * as React from "react";
import { GbaKey, GbaKeyAction } from "../models/actions";

export type GbaKeyHandler = (key: GbaKey, action: GbaKeyAction, repeat: boolean) => void;

export interface GbaKeyControlProps {
    readonly onKeyEvent: GbaKeyHandler;
}

export class GbaKeyControl extends React.PureComponent<GbaKeyControlProps> {

    private keyDownHandler?: (e: KeyboardEvent) => void = undefined;
    private keyUpHandler?: (e: KeyboardEvent) => void = undefined;

    /**
     * @overrides
     */
    public render(): React.ReactNode {
        return null;
    }

    /**
     * @overrides
     */
    public componentDidMount(): void {
        this.keyDownHandler = (e: KeyboardEvent) => {
            if (this.mapKeyAction(e.code, "down", e.repeat)) {
                e.preventDefault();
            }
        };
        window.addEventListener("keydown", this.keyDownHandler);

        this.keyUpHandler = (e: KeyboardEvent) => {
            if (this.mapKeyAction(e.code, "up", e.repeat)) {
                e.preventDefault();
            }
        };
        window.addEventListener("keyup", this.keyUpHandler);
    }

    /**
     * @overrides
     */
    public componentWillUnmount(): void {
        if (this.keyDownHandler) {
            window.removeEventListener("keydown", this.keyDownHandler);
        }

        if (this.keyUpHandler) {
            window.removeEventListener("keyup", this.keyUpHandler);
        }
    }

    /**
     * @returns Prevent default.
     */
    private mapKeyAction(key: string, action: GbaKeyAction, repeat: boolean): boolean {
        const gbaKey = this.mapGbaKey(key);
        if (gbaKey) {
            this.props.onKeyEvent(gbaKey, action, repeat);
            return true;
        }
        return false;
    }

    /**
     * @returns GBA button key.
     */
    private mapGbaKey(key: string): GbaKey | undefined {
        switch (key) {
            case "KeyZ":
                return "A";

            case "KeyX":
                return "B";

            case "KeyA":
                return "L";

            case "KeyS":
                return "R";

            case "Backspace":
                return "select";

            case "Enter":
                return "start";

            case "ArrowLeft":
                return "left";

            case "ArrowRight":
                return "right";

            case "ArrowUp":
                return "up";

            case "ArrowDown":
                return "down";

            default:
                return undefined;
        }
    }
}
