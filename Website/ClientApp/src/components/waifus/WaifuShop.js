import React, { Component } from 'react';
import { Table } from './Table'

export class WaifuShop extends Component {
    static displayName = WaifuShop.name;

    constructor(props) {
        super(props);
        this.state = {
            waifus: [],
            loading: true
        };
    }

    componentDidMount() {
        const { handle } = this.props.match.params;
        this.populateWaifus(handle);
    }

    render() {
        let contents = this.state.loading
            ? <h4 className="text-muted text-center align-middle"><em>Loading...</em></h4>
                : this.state.waifus.length === 0
                ? <h4 className="text-muted text-center align-middle"><em>~ Shop empty ~</em></h4>
                : WaifuShop.showWaifus(this.state.waifus)

        return (
            <div>
                {contents}
            </div>
        );
    }

    async populateWaifus(s) {
        var query = "api/waifushop";
        if (s) {
            query = query + "/" + s;
        }
        const response = await fetch(query);
        const data = await response.json();
        this.setState({ waifus: data, loading: false });
    }

    static showWaifus(waifus) {
        let tiers = Array.from(new Set(waifus.map(w => { return w.tier })));
        tiers.sort();

        return (
            <div>
                {tiers.map(t => { return WaifuShop.tieredList(waifus.filter(w => { return w.tier === t }), t)})}
            </div>
        );
    }

    static tieredList(waifus, tier) {
        return (
            <div>
                <h2 className="text-center"><span class="badge badge-secondary col-md-12">Tier {tier}</span></h2>
                <Table waifus={waifus} />
            </div>
        );
    }
}
