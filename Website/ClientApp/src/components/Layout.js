import React, { Component } from 'react';
import { NavMenu } from './shared/NavMenu';
import { FooterMenu } from './shared/FooterMenu';

export class Layout extends Component {
    static displayName = Layout.name;

    render() {
        return (
            <div>
                <NavMenu />
                {this.props.children}
                <FooterMenu />
            </div>
        );
    }
}
