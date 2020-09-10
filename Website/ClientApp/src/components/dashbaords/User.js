import React, { Component } from 'react';
import { Container } from 'reactstrap';

export class User extends Component {
    static displayName = User.name;

    constructor(props) {
        super(props);
        this.state = {
            waifus: [],
            loading: true
        };
    }

    componentDidMount() {
        const { guildId, userId } = this.props.match.params;
        this.load(guildId, userId);
    }

    render() {
        let contents = this.state.loading
            ? <h4 className="text-muted text-center align-middle"><em>Loading...</em></h4>
            : this.state.waifus.length === 0
                ? <h4 className="text-muted text-center align-middle"><em>~ Shop empty ~</em></h4>
                : <h4 className="text-muted text-center align-middle"><em>asdf</em></h4>

        return (
            <Container>
                <div>
                    {contents}
                </div>
            </Container>
        );
    }

    async load(guildId, userId) {
        var query = "api/waifushop/" + guildId + "/" + userId;
        const response = await fetch(query);
        const data = await response.json();
        this.setState({ waifus: data, loading: false });
    }
}