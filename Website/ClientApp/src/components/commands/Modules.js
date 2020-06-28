import React, { Component } from 'react';
import { Table } from './Table'

export class Modules extends Component {
    static displayName = Modules.name;

    render() {
        return (
            <div>
                <div className="commands">
                    {this.props.modules.map(module =>
                        <>
                            <div className="commands-title col-lg-12">{module.name}</div>
                            <Table commands={module.commands} />
                        </>
                    )}
                </div>
            </div>
        );
    }
}
