import React, { Component } from 'react';
import { Card } from './Card'

export class Table extends Component {
    static displayName = Table.name;

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