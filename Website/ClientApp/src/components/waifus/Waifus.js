import React, { Component } from 'react';
import { Table } from './Table'
import { Container } from 'reactstrap';
import './Waifus.css';

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

    render() {
        let contents = this.state.loading
            ? <h4 className="text-muted text-center align-middle"><em>Loading...</em></h4>
            : this.state.waiting
            ? Waifus.waitingInfo()
            : this.state.waifus.length === 0
            ? Waifus.waitingInfo("No results...")
            : <Table waifus={this.state.waifus} />

        return (
            <div className="container-waifus">
                <div className="search-bar horizontal-center">
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
        this.setState({ waifus: data, loading: false, waiting: false });
    }

    async onKeyDown(e) {
        if (e.key === 'Enter') {
            if (e.target.value !== "") {
                this.setState({ waiting: false, loading: true });
                await this.populateWaifus(e.target.value);
            }
            else {
                this.setState({ waiting: true, loading: false })
            }
        }
    }

    static waitingInfo(pretext) {
        return (
            <div className="text-center align-middle text-muted2">
                <em>
                    <h3>{pretext}</h3>
                    <p>Search for waifus by typing a part of their name or source...</p>
                    <p>
                        Examples:<br/>
                        <code>re zero</code> - search by source<br/>
                        <code>emilia</code> - search by name<br/>
                        <code>emilia re zero</code> - both name and source<br/>
                    </p>
                    <p>
                        ✔ <code>emil zero</code> - partial name<br />
                        ✔ <code>shingeki no kyojin</code> - japanese anime title<br />
                        ❌ <code>attack on titan</code> - english anime title (might work sometimes, try both)<br />
                        ❌ <code>emila</code> - typo<br />
                        ❌ <code>emilia megumin</code> - two different characters<br />
                    </p>
                </em>
            </div>
        );
    }
}
