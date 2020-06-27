import React, { Component } from 'react';

export class Table extends Component {
    static displayName = Table.name;

    render() {
        return (
            <div>
                <table className='table table-striped text-light' aria-labelledby="tabelLabel">
                    <thead>
                        <tr>
                            <th>Command</th>
                            <th>Description</th>
                            <th>Aliases</th>
                            <th>Example</th>
                        </tr>
                    </thead>
                    <tbody>
                        {this.props.commands.map(c =>
                            <tr key={c.id}>
                                <td>{c.name}</td>
                                <td>{c.description}</td>
                                <td>{c.aliases}</td>
                                <td>{c.example}</td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>
        );
    }
}
