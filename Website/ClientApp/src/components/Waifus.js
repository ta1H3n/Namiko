import React, { Component } from 'react';
import './Waifus.css';

export class Waifus extends Component {
    static displayName = Waifus.name;

    constructor(props) {
        super(props);
        this.state = { waifus: [], loading: true };
    }

    componentDidMount() {
        this.populateWaifus();
    }

    static renderWaifuTable(waifus) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Waifu</th>
                        <th>Source</th>
                        <th>Description</th>
                    </tr>
                </thead>
                <tbody>
                    {waifus.map(w =>
                        <tr key={w.name}>
                            <td>{w.longName}</td>
                            <td>{w.source}</td>
                            <td>{w.description}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : Waifus.renderWaifuTable(this.state.waifus);

        return (
            <div>
                <h1 id="tabelLabel" >Blood for the Blood God</h1>
                <h2>Skulls for the Skull Throne</h2>
                <p>Search results.</p>
                {contents}
            </div>
        );
    }

    async populateWaifus() {
        const response = await fetch('api/waifu');
        const data = await response.json();
        this.setState({ waifus: data, loading: false });
    }
}
