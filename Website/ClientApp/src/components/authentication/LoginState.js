import React, { Component } from 'react';
import { Redirect } from "react-router-dom";

export class LoginState extends Component {
    static displayName = LoginState.name;

    constructor(props) {
        super(props);
        this.state = {
            loading: true,
            returnUrl: '/User/Me'
        };
    }

    componentDidMount() {
        const queryString = require('query-string');
        const parsed = queryString.parse(window.location.search).returnUrl
        if (parsed) {
            this.setState({ returnUrl: parsed });
        }
        this.updateState();
    }

    render() {
        let contents = this.state.loading
            ? <h3 className="text-muted text-center align-middle mt-5"><em>Processing login...</em></h3>
            : <Redirect to={this.state.returnUrl} />

        return (
            <div>
                {contents}
            </div>
        );
    }

    async updateState() {
        const query = "api/authentication";
        const response = await fetch(query);
        const data = await response.json();
        if (data.loggedIn === true) {
            localStorage.setItem('name', data.name);
            localStorage.setItem('id', data.id);
            localStorage.setItem('discriminator', data.discriminator);
            localStorage.setItem('avatarHash', data.avatarHash);
            localStorage.setItem('avatarUrl', data.avatarUrl);
            localStorage.setItem('loggedIn', data.loggedIn);
        }
        else {
            localStorage.removeItem('name');
            localStorage.removeItem('id');
            localStorage.removeItem('discriminator');
            localStorage.removeItem('avatarHash');
            localStorage.removeItem('avatarUrl');
            localStorage.setItem('loggedIn', data.loggedIn);
            this.setState({ returnUrl: '/' });
        }
        this.setState({ loading: false });
    }
}