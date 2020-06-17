import React, { Component } from 'react';

export class Card extends Component {
    static displayName = Card.name;

    render() {
        return (
            <div>   
                <div class="card">
                    <img class="card-img-top" src={this.props.waifu.imageUrl} alt="Card image cap"/>
                    <div class="card-body">
                        <h5 class="card-title">{this.props.waifu.longName}</h5>
                        <h6 class="card-subtitle mb-2">{this.props.waifu.source}</h6>
                    </div>
                </div>
            </div>
        );
    }
}
