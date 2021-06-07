import React, { Component } from 'react';
import { WaifuTable } from './Table'
import './Waifus.css';
import { get } from '../RequestHandler';
import { TitleBar } from '../shared/TitleBar';

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
            ? <h3 className="text-muted text-center align-middle mt-5"><em>Loading...</em></h3>
            : this.state.waiting
                ? Waifus.waitingInfo()
                : this.state.waifus.length === 0
                    ? Waifus.waitingInfo("No results...")
                    : <WaifuTable waifus={this.state.waifus} />

        return (
            <>
                <TitleBar title="Waifus" />
                <div className="container-waifus">
                    <div className="search-bar horizontal-center">
                        <input type="text" className="form-control search-bar" placeholder="Search for waifus..." onKeyDown={this.onKeyDown} />
                    </div>
                    <ol />
                    {contents}
                </div>
            </>
        );
    }

    async populateWaifus(s) {
        var query = "api/waifu";
        if (s) {
            query = query + "?search=";
            query = query + encodeURIComponent(s.toLowerCase());
        }
        const response = await get(query);
        const data = response.value;
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
                        Examples:<br />
                        <code>re zero</code> - search by source<br />
                        <code>emilia</code> - search by name<br />
                        <code>emilia re zero</code> - both name and source<br />
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
