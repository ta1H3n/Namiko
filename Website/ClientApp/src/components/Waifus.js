import React, { Component } from 'react';
import { Card } from './waifus/Card'

export class Waifus extends Component {
    static displayName = Waifus.name;

    constructor(props) {
        super(props);
        this.state = { waifus: [], loading: true };
    }

    componentDidMount() {
        this.populateWaifus();
    }

    //static renderWaifuTable(waifus) {
    //    return (
    //        <table className='table table-striped' aria-labelledby="tabelLabel">
    //            <thead>
    //                <tr>
    //                    <th>Waifu</th>
    //                    <th>Source</th>
    //                    <th>Description</th>
    //                    <th>Image</th>
    //                </tr>
    //            </thead>
    //            <tbody>
    //                {waifus.map(w =>
    //                    <tr key={w.name}>
    //                        <td>{w.longName}</td>
    //                        <td>{w.source}</td>
    //                        <td>{w.description}</td>
    //                        <td><img src={w.imageUrl} alt="sample"/></td>
    //                    </tr>
    //                )}
    //            </tbody>
    //        </table>
    //    );
    //}

    static renderWaifuTable(waifus) {
        return (
            <div className="container">
                <div className="row">
                    {waifus.map(w =>
                        <div className="col-sm-3"> {w.name} </div>)}
                </div>
            </div>
        );
    }

    render() {
        
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : Waifus.renderWaifuTable(this.state.waifus);

        return (
            <div className="content">
                <h1 id="tabelLabel" >Blood for the Blood God</h1>
                <h2>Skulls for the Skull Throne</h2>

                <div className="row">
                    <div className="col-sm-8">col-sm-8</div>
                    <div className="col-sm-4">col-sm-4</div>
                </div>
                <div className="row">
                    <div className="col-sm">col-sm</div>
                    <div className="col-sm">col-sm</div>
                    <div className="col-sm">col-sm</div>
                </div>

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
