import * as React from "react";
import { Gba } from "./gba";

export class IndexPage extends React.Component {
    public render(): React.ReactNode {
        return <React.Fragment>
            <Header>OptimeGBA<span className="decoration">.</span>io</Header>
            <Content><Gba /></Content>
            <Footer>Powered by <a href="https://github.com/Powerlated/OptimeGBA">OptimeGBA</a></Footer>
        </React.Fragment>;
    }
}

class Header extends React.Component {
    public render(): React.ReactNode {
        return <div id="header-container">
            <h1>{this.props.children}</h1>
        </div>;
    }
}

class Content extends React.Component {
    public render(): React.ReactNode {
        return <div id="content-container">
            {this.props.children}
        </div>;
    }
}

class Footer extends React.Component {
    public render(): React.ReactNode {
        return <div id="footer-container">
            <div>{this.props.children}</div>
        </div>;
    }
}
