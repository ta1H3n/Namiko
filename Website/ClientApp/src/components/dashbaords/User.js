import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { Link } from 'react-router-dom';
import { parseCode } from '../authentication/ResponseHandler';
import './dashboard.css';
import { get } from '../RequestHandler';

export class User extends Component {
    static displayName = User.name;

    constructor(props) {
        super(props);
        this.state = {
            res: null,
            loading: true,
            fail: false
        };
    }

    componentDidMount() {
        this.load();
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
                        <h2 className="text-center title-text inline"><strong>{this.state.res.name}</strong></h2><h5 className="inline">#{this.state.res.discriminator}</h5>
                    </div>
                    <Container>
                        <div className="row mt-5">
                            <div className="card-columns" >
                                {this.state.res.guilds.map(guild =>
                                    <div className="card card-guild">
                                        <img className="card-img-top" src={guild.imageUrl + "?size=512"} alt={guild.id} />
                                        <div className="card-body">
                                            <h5 className="card-title text-center">{guild.name}</h5>
                                            <Link className="stretched-link" to={"/Guild/" + guild.id + "/" + this.state.res.id} />
                                        </div>
                                    </div>
                                )}
                            </div>
                        </div>
                    </Container>
                </>

        return contents;
    }

    async load() {
        const query = "api/user";
        const response = await get(query);

        var result = parseCode(response.status);
        if (result) {
            this.setState({ loading: false, fail: result });
            return;
        }

        const data = response.value;
        this.setState({ res: data, loading: false });
    }
}