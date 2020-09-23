import React, { Component } from 'react';
import { Nav } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import Cookies from 'js-cookie';

export class LoginMenu extends Component {

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
