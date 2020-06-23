import React, { Component } from 'react';
import { Table } from './Table'

export class Modules extends Component {
    static displayName = Modules.name;

    render() {
        return (
            <div>
                {this.props.modules.map(module =>
                    <>
                        <h2><span className="badge color3 col-lg-12">{module.name}</span></h2>
                        <Table commands={module.commands} />
                    </>
                )}
            </div>
        );
    }
}
