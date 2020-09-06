import React, { Component } from 'react';
import { Redirect } from "react-router-dom";
import { Container } from 'reactstrap';

export class LoginState extends Component {
    static displayName = LoginState.name;

    constructor(props) {
        super(props);
        this.state = {
            loading: true
        };
    }

    componentDidMount() {
        this.updateState();
    }

    render() {
        let contents = this.state.loading
            ? <h4 className="text-muted text-center align-middle"><em>Signing in...</em></h4>
            : <Redirect to='/' />

        return (
            <Container>
                <div>
                    {contents}
                </div>
            </Container>
        );
    }

    async updateState() {
        var query = "authentication";
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
        }
        this.setState({ loading: false });
    }
}