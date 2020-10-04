import React, { Component } from 'react';
import { Container } from 'reactstrap';

export class Contact extends Component {
    static displayName = Contact.name;

    render() {
        return (
            <div className="box">
                <Container>
                    <h1>Contact</h1>
                    <p>You can reach the administrators of this site and the developers of the Namiko bot by sending an e-mail to <a href="mailto:namikomoe@gmail.com">namikomoe@gmail.com</a></p>
                </Container>
            </div>
        );
    }
}
