import React, { Component } from 'react';
import { Clickable, ParseTier } from './Clickable';

export class Card extends Component {
    static displayName = Card.name;

    constructor(props) {
        super(props)
    }

    render() {
        return (
            <div>
                <div className={"card card-t" + ParseTier(this.props.waifu.tier)}>
                    <img className="card-img-top" src={this.props.waifu.imageMedium} alt={this.props.waifu.name} />
                    <div className="card-body">
                        <h5 className="card-title text-center">{this.props.waifu.longName.split('(')[0]}</h5>
                        <h6 className="card-subtitle text-center mb-1">{this.props.waifu.source}</h6>
                        <Clickable waifu={this.props.waifu} />
                    </div>
                </div>
            </div>
        );
    }
}
