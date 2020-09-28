import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { get } from '../RequestHandler';

export class Guild extends Component {
    static displayName = Guild.name;

    constructor(props) {
        super(props);
        this.state = {
            waifus: [],
            loading: true
        };
    }

    componentDidMount() {
        const { guildId } = this.props.match.params;
        this.load(guildId);
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

    async load(guildId) {
        var query = "api/waifushop/" + guildId;
        const response = await get(query);
        const data = response.value;
        this.setState({ waifus: data, loading: false });
    }
}