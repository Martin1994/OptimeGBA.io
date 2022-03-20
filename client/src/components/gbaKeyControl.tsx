import * as React from "react";
import { GbaKeyAction } from "../models/actions";
import { Gba } from "./gba";

export interface GbaKeyControlProps {
    readonly gba: Gba;
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

    public componentWillUnmount(): void {
        if (this.keyDownHandler) {
            window.removeEventListener("keydown", this.keyDownHandler);
        }

        if (this.keyUpHandler) {
            window.removeEventListener("keyup", this.keyUpHandler);
        }
    }

    private mapKeyAction(key: string, action: GbaKeyAction, repeat: boolean): boolean {
        switch (key) {
            case "KeyZ":
                this.props.gba.sendKeyAction("A", action, repeat);
                break;

            case "KeyX":
                this.props.gba.sendKeyAction("B", action, repeat);
                break;

            case "KeyA":
                this.props.gba.sendKeyAction("L", action, repeat);
                break;

            case "KeyS":
                this.props.gba.sendKeyAction("R", action, repeat);
                break;

            case "Backspace":
                this.props.gba.sendKeyAction("select", action, repeat);
                break;

            case "Enter":
                this.props.gba.sendKeyAction("start", action, repeat);
                break;

            case "ArrowLeft":
                this.props.gba.sendKeyAction("left", action, repeat);
                break;

            case "ArrowRight":
                this.props.gba.sendKeyAction("right", action, repeat);
                break;

            case "ArrowUp":
                this.props.gba.sendKeyAction("up", action, repeat);
                break;

            case "ArrowDown":
                this.props.gba.sendKeyAction("down", action, repeat);
                break;

            default:
                return false;
        }

        return true;
    }
}
