import React, { Component } from 'react';

export class Card extends Component {
    static displayName = Card.name;

    render() {
        return (
            <div>   
                Test: {this.props.waifu}
            </div>
        );
    }
}