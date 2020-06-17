import React, { Component } from 'react';
import { Card } from './waifus/Card'

export class Waifus extends Component {
    static displayName = Waifus.name;

    constructor(props) {
        super(props);
        this.state = {
            waifus: [],
            waiting: true,
            loading: false
        };

        this.onKeyDown = this.onKeyDown.bind(this);
    }

    static renderWaifuTable(waifus) {
        return (
            <div className="card-columns" >
                {waifus.map(waifu =>
                    <Card waifu={waifu} />
                )}
            </div>
        );
    }

    render() {
        let contents = this.state.loading
            ? <h4 className="text-muted text-center align-middle"><em>Loading...</em></h4>
            : this.state.waiting
            ? <h4 className="text-muted text-center align-middle"><em>Waiting...</em></h4>
            : Waifus.renderWaifuTable(this.state.waifus);

        return (
            <div>
                <div>
                    <input type="text" className="form-control" placeholder="Search for waifus..." onKeyDown={this.onKeyDown} />
                </div>
                <ol/>
                {contents}
            </div>
        );
    }

    async populateWaifus(s) {
        var query = "api/waifu";
        if (s) {
            query = query + "?search=";
            query = query + encodeURIComponent(s);
        }
        const response = await fetch(query);
        const data = await response.json();
        this.setState({ waifus: data, loading: false });
    }

    async onKeyDown(e) {
        if (e.key === 'Enter') {
            if (e.target.value !== "") {
                this.setState({ waiting: false, loading: true });
                await this.populateWaifus(e.target.value);
            }
        }
    }
}
