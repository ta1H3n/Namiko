import React, { Component, Button } from 'react';
import { Modules } from './Modules'

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
            ? <h4 className="text-muted text-center align-middle"><em>Loading...</em></h4>
            : this.state.modules.length === 0
                ? <h4 className="text-muted text-center align-middle"><em>No Results...</em></h4>
                : <Modules modules={this.state.display} />

        return (
            <div>
                <button className="btn btn-primary" onClick={() => this.filterModules("All")}>Show all</button>
                {this.state.modules.map(module =>
                    <button className="btn btn-primary" onClick={() => this.filterModules(module.name)}>{module.name}</button>
                )}
                {contents}
            </div>
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
        const response = await fetch(query);
        const data = await response.json();
        this.setState({ modules: data, display: data, loading: false });
    }
}
