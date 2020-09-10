import React, { Component } from 'react';
import { Nav } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import Cookies from 'js-cookie';

export class LoginMenu extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        if (localStorage.getItem("id") && Cookies.get(".AspNetCore.Cookies")) {
            return this.authenticatedView();
        } else {
            return this.anonymousView();
        }
    }

    authenticatedView() {
        return (
            <Nav>
                <Nav.Link as={Link} to='/User/Me'>Hi {localStorage.getItem("name")}!</Nav.Link>
                <Nav.Link href='/authentication/logout'>Logout</Nav.Link>
            </Nav>
        );

    }

    anonymousView() {
        return (
            <Nav>
                <Nav.Link href='/authentication/login'>Login</Nav.Link>
            </Nav>
        );
    }
}
