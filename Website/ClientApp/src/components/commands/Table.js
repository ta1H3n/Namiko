import React, { Component } from 'react';

export class Table extends Component {
    static displayName = Table.name;

    render() {
        return (
            <div>
                <div className="commands-header">
                    <p>Command</p>
                    <p>Description</p>
                    <p>Example</p>
                </div>
                {this.props.commands.map(c =>
                    <div className="command">
                        <div>
                            <section className="command-name">
                                <span>{c.name}</span>
                            </section>
                            <section className="command-aliases">
                                {c.aliasesArray.map(val =>
                                    <span>!{val}</span>
                                )}
                            </section>
                        </div>
                        <div className="command-description">
                            <section>{c.description}</section>
                            {Table.Conditions(c.conditionsArray)}
                        </div>
                        <div className="command-example">{c.example}</div>
                    </div>
                )}
            </div>
        );
    }

    static Conditions(array) {
        if (array[0] === "") {
            return "";
        }
        return (
            <section>
                <span className="command-requires">Requires</span>
                {array.map(val =>
                    <span className="condition">{val}</span>
                )}
            </section>
        );
    }
}
