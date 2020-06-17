import React, { Component } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';

export class FooterMenu extends Component {
    static displayName = FooterMenu.name;

    constructor(props) {
        super(props);
    }

    render() {
        return (
            <footer class="main_footer">
                <div class="container">
                    <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-top mb-3" light>
                        <Container>
                            <NavbarBrand tag={Link} to="/">Privacy</NavbarBrand>
                            <NavbarToggler onClick={this.toggleNavbar} className="mr-2" />
                            <ul className="navbar-nav flex-grow">
                                <NavItem>
                                    <NavLink tag={Link} className="text-dark" to="/">Home</NavLink>
                                </NavItem>
                                <NavItem>
                                    <NavLink tag={Link} className="text-dark" to="/waifus">Waifus</NavLink>
                                </NavItem>
                            </ul>
                        </Container>
                    </Navbar>
                </div>
            </footer>
        );
    }
}
