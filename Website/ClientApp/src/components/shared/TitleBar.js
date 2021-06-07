import React, { Component } from 'react';
import './NavMenu.css';
import { Link } from 'react-router-dom';

export class TitleBar extends Component {
    static displayName = TitleBar.name;

    render() {
        return (
            <div className="title-bar">
                <div className="title">{this.props.title}</div>
            </div>
        );
    }
}
