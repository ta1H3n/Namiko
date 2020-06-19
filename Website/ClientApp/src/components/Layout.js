import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';
import { EarlyBuildBar } from './EarlyBuildBar';
import { FooterMenu } from './FooterMenu';

export class Layout extends Component {
    static displayName = Layout.name;

    render() {
        return (
            <div>
                <EarlyBuildBar />
                <NavMenu />
                <Container>
                    {this.props.children}
                </Container>
                <FooterMenu />
            </div>
        );
    }
}
