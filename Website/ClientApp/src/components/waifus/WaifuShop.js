import React, { Component } from 'react';
import { Table } from './Table'
import { Container } from 'reactstrap';

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
            <Container>
                <div>
                    {contents}
                </div>
            </Container>
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
                <h2>
                    <span className="badge color1 col-lg-9 col-md-8 col-sm-7 col-xs-6">Tier {tier}</span>
                    <span className="badge color3 col-lg-3 col-md-4 col-sm-5 col-xs-6">{priceFromTier(tier)} Toasties</span>
                </h2>
                <Table waifus={waifus} />
            </div>
        );
    }
}

function priceFromTier(tier) {
    let price = tier === 0
        ? '100,000'
        : tier === 1
            ? '20,000'
            : tier === 2
                ? '10,000'
                : tier === 3
                    ? '5,000'
                    : '0';
    return price;
}