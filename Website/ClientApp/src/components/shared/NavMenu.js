import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { Navbar, Nav } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import { LoginMenu } from '../authentication/LoginMenu';
import './NavMenu.css';

export class NavMenu extends Component {
    static displayName = NavMenu.name;

    constructor(props) {
        super(props);

        this.toggleNavbar = this.toggleNavbar.bind(this);
        this.state = {
            collapsed: true
        };
    }

    toggleNavbar() {
        this.setState({
            collapsed: !this.state.collapsed
        });
    }

    render() {
        return (
            <header>
                <Navbar collapseOnSelect expand="md" className="mb-3 namiko-navbar" variant="dark">
                    <Container>
                        <Navbar.Brand as={Link} to="/">
                            <img
                                src="/images/NamikoMoe.png"
                                width="auto"
                                height="30"
                                className="d-inline-block align-top"
                                alt="Namiko Moe logo"
                            />
                        </Navbar.Brand>
                        <Navbar.Toggle aria-controls="responsive-navbar-nav" />
                        <Navbar.Collapse id="responsive-navbar-nav">
                            <Nav className="mr-auto">
                                <Nav.Link as={Link} to="/Guide">Guide</Nav.Link>
                                <Nav.Link as={Link} to="/Commands">Commands</Nav.Link>
                                <Nav.Link as={Link} to="/Waifus">Waifus</Nav.Link>
                                <Nav.Link as={Link} to="/Pro">Pro</Nav.Link>
                            </Nav>
                            <LoginMenu />
                        </Navbar.Collapse>
                    </Container>
                </Navbar>
            </header>
        );
    }
}
