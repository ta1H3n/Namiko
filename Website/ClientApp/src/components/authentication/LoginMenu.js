import React, { Component } from 'react';
import { Nav } from 'react-bootstrap';
import { Link } from 'react-router-dom';

export class LoginMenu extends Component {

    render() {
        if (localStorage.getItem("id")) {
            return this.authenticatedView();
        } else {
            return this.anonymousView();
        }
    }

    authenticatedView() {
        return (
            <Nav>
                <Nav.Link as={Link} to='/User/Me'>Hi {localStorage.getItem("name")}!</Nav.Link>
                <a className="navLink" href='/authentication/logout'>Logout</a>
            </Nav>
        );
    }

    anonymousView() {
        return (
            <Nav>
                <a className="navLink" href='/authentication/login'>Login</a>
            </Nav>
        );
    }
}
