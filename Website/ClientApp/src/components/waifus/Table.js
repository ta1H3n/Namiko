import React, { Component } from 'react';
import { Card } from './Card'

export class WaifuTable extends Component {
    static displayName = WaifuTable.name;

    render() {
        return (
            <div className="card-columns" >
                {this.props.waifus.map(waifu =>
                    <Card waifu={waifu} />
                )}
            </div>
        );
    }
}