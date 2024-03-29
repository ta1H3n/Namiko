﻿import React, { Component } from 'react';
import { Modules } from './Modules';
import './Commands.css';
import { get } from '../RequestHandler';
import { TitleBar } from '../shared/TitleBar';

export class Commands extends Component {
    static displayName = Commands.name;

    constructor(props) {
        super(props);
        this.state = {
            modules: [],
            display: [],
            loading: true,
            selected: "All"
        };
    }

    componentDidMount() {
        this.populateCommands();
    }

    render() {
        let contents = this.state.loading
            ? <h3 className="text-muted text-center align-middle mt-5"><em>Loading...</em></h3>
            : this.state.modules.length === 0
                ? <h3 className="text-muted text-center align-middle mt-5"><em>No Results...</em></h3>
                : <Modules modules={this.state.display} />

        return (
            <>
                <TitleBar title="Commands" />
                <div className="container-commands-list">
                    <div className="row">
                        <div className="text-center horizontal-center">
                            <button className="btn btn-blank mr-1 mt-1" onClick={() => this.filterModules("All")}>Show all</button>
                            {this.state.modules.map(module =>
                                <button className="btn btn-blank mr-1 mt-1" onClick={() => this.filterModules(module.name)}>{module.name}</button>
                            )}
                        </div>
                    </div>
                    {contents}
                </div>
            </>
        );
    }

    async filterModules(name) {
        if (name === "All") {
            this.setState({ display: this.state.modules, selected: name })
        }
        else {
            this.setState({ display: this.state.modules.filter(x => x.name === name), selected: name })
        }
    }

    async populateCommands() {
        var query = "api/commands";
        const response = await get(query);
        const data = response.value;
        this.setState({ modules: data, display: data, loading: false });
    }
}
