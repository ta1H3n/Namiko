import React, { Component, Button } from 'react';
import { Modules } from './Modules'
import { Container } from 'reactstrap';

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
            <Container>
                <div>
                    <button className="btn color1 color1-hover mr-1 mt-1" onClick={() => this.filterModules("All")}>Show all</button>
                    {this.state.modules.map(module =>
                        <button className="btn color1 color1-hover mr-1 mt-1" onClick={() => this.filterModules(module.name)}>{module.name}</button>
                    )}
                    {contents}
                </div>
            </Container>
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
