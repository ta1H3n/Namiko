import React, { Component } from 'react';
import { Clicklable } from './Clickable';

export class Card extends Component {
    static displayName = Card.name;

    constructor(props) {
        super(props)
    }

    render() {
        return (
            <div>   
                <div className="card">
                    <img className="card-img-top" src={this.props.waifu.imageUrl} alt="Card image cap"/>
                    <div className="card-body">
                        <h5 className="card-title">{this.props.waifu.longName}</h5>
                        <h6 className="card-subtitle mb-2">{this.props.waifu.source}</h6>
                        <Clicklable waifu={this.props.waifu} />
                    </div>
                </div>
            </div>
        );
    }
}
