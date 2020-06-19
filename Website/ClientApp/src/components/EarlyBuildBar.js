import React, { Component } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';

export class EarlyBuildBar extends Component {
    static displayName = EarlyBuildBar.name;

    constructor(props) {
        super(props);
    }

    render() {
        return (
            <div className="alert alert-info" role="alert">
                <h5 className="text-center">This is a very early preview build of Namiko Moe - anything might and will change!</h5>
            </div>
        );
    }
}
