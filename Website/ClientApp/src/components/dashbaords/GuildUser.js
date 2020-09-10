import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { parseCode } from '../authentication/ResponseHandler';
import { Table } from 'react-bootstrap';
import { WaifuTable } from '../waifus/Table';
import { Link } from 'react-router-dom';
import './dashboard.css';

export class GuildUser extends Component {
    static displayName = GuildUser.name;

    constructor(props) {
        super(props);
        this.state = {
            res: null,
            loading: true,
            fail: false
        };
    }

    componentDidMount() {
        var guildId = this.props.match.params.guildId;
        var userId = this.props.match.params.userId;
        this.load(guildId, userId);
    }

    render() {
        let contents = this.state.loading
            ? <h3 className="text-muted text-center align-middle mt-5"><em>Loading...</em></h3>
            : this.state.fail
                ? <Container>
                    <div>
                        {this.state.fail}
                    </div>
                </Container>
                : <>
                    <div className="dashboard-header">
                        <img className="avatar" alt="avatar" src={this.state.res.avatarUrl} /><br />
                        <h2 className="text-center title-text inline"><strong>{this.state.res.name}</strong><h5 className="inline">#{this.state.res.discriminator}</h5></h2>
                    </div>
                    <Container>
                        <div className="row mt-2">
                            <div className="col-sm-12 col-md-4 mt-3">
                                <div className="container-stats">
                                    <div className="stats-header">
                                        <img className="avatar" alt="avatar" src={this.state.res.guild.imageUrl} /><br />
                                        <h4 className="text-center title-text inline"><strong>{this.state.res.guild.name}</strong></h4>
                                        <Table borderless size="sm" striped className="stat">
                                            <tbody>
                                                <tr>
                                                    <td><section className="stat-title">Join date</section></td>
                                                    <td>{this.state.res.joinedAt.split('T')[0]}</td>
                                                </tr>
                                                <tr>
                                                    <td><section className="stat-title">Rep</section></td>
                                                    <td>{this.state.res.rep}</td>
                                                </tr>
                                                <tr>
                                                    <td><section className="stat-title">Lootboxes opened</section></td>
                                                    <td>{this.state.res.lootboxesOpened}</td>
                                                </tr>
                                                <tr>
                                                    <td><section className="stat-title">Daily streak</section></td>
                                                    <td>{this.state.res.daily}</td>
                                                </tr>
                                                <tr>
                                                    <td><section className="stat-title">Toasties</section></td>
                                                    <td>{this.state.res.balance}</td>
                                                </tr>
                                                <tr>
                                                    <td><section className="stat-title">Waifu amount</section></td>
                                                    <td>{this.state.res.waifuAmount}</td>
                                                </tr>
                                                <tr>
                                                    <td><section className="stat-title">Waifu value</section></td>
                                                    <td>{this.state.res.waifuValue}</td>
                                                </tr>
                                            </tbody>
                                        </Table>
                                        <div className="horizontal-center">
                                            <Link type="button" className="btn btn-blank mt-2" to={"/WaifuShop/" + this.state.res.guild.id}>Waifu&nbsp;shop</Link>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div className="col-sm-12 col-md-8 mt-3">
                                <div className="row container-adds mb-3">
                                    <section className="quote">{this.state.res.quote}</section>
                                </div>
                                <div className="row container-adds">
                                    <img className="img-fluid" alt={this.state.res.imageUrl} src={this.state.res.imageUrl} /><br />
                                </div>
                            </div>
                        </div>
                    </Container>
                    <div className="row container-waifus mt-5">
                        <WaifuTable waifus={this.state.res.waifus} />
                    </div>
                </>

        return contents;
    }

    async load(guildId, userId) {
        const query = "api/guild/" + guildId + "/" + userId;
        const response = await fetch(query);

        var result = parseCode(response.status);
        if (result) {
            this.setState({ loading: false, fail: result });
            return;
        }

        const data = await response.json();
        this.setState({ res: data, loading: false });
    }
}