import React, { Component } from 'react';
import './NavMenu.css';
import { Link } from 'react-router-dom';

export class FooterMenu extends Component {
    static displayName = FooterMenu.name;

    render() {
        return (
            <footer>
                <div className="footer">
                    Namiko Moe / Namiko / <Link to="/Contact">Contact</Link> / <Link to="/Privacy">Terms</Link>
                </div>
            </footer>
        );
    }
}
