import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { Table } from 'react-bootstrap';
import { TitleBar } from '../shared/TitleBar';
import '../commands/Commands.css';
import './static.css'

export class Pro extends Component {
    static displayName = Pro.name;

    render() {
        return (
            <>
                <TitleBar title="Premium Tiers" />
                <Container>
                    <div className="row pro-container">
                        <div className="col-12 pro-title">
                            Account upgrades
                        </div>
                        <br />
                        <div className="col-12 pro-description" >
                            Upgrades for a <strong>Discord account</strong>. Active in all Discord servers! Good for anyone enjoying Namiko.
                        </div>
                        <Table striped bordered variant="dark" className="pro-table">
                            <thead>
                                <tr>
                                    <th className="col-3"></th>
                                    <th className="col-2 pro-free">Free</th>
                                    <th className="col-3 pro-pro">Pro</th>
                                    <th className="col-3 pro-plus">Pro+</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>Waifu wishlist size</td>
                                    <td>5</td>
                                    <td>5</td>
                                    <td><b>12</b></td>
                                </tr>
                                <tr>
                                    <td>Increased wishlist droprate</td>
                                    <td>❌</td>
                                    <td>✔️</td>
                                    <td>✔️</td>
                                </tr>
                                <tr>
                                    <td>Daily cooldown</td>
                                    <td>20 hours</td>
                                    <td>20 hours</td>
                                    <td><b>10 hours</b></td>
                                </tr>
                                <tr>
                                    <td>Voting lootbox</td>
                                    <td>Basic</td>
                                    <td>Premium 🌟</td>
                                    <td>Premium 🌟</td>
                                </tr>
                                <tr>
                                    <td>Weekly</td>
                                    <td>Basic</td>
                                    <td>Basic</td>
                                    <td>Premium 🌟</td>
                                </tr>
                                <tr>
                                    <td>Your own custom waifu</td>
                                    <td>❌</td>
                                    <td>❌</td>
                                    <td>✔️</td>
                                </tr>
                                <tr>
                                    <td>Marriage limit</td>
                                    <td>1</td>
                                    <td>3</td>
                                    <td><b>10</b></td>
                                </tr>
                                <tr>
                                    <td></td>
                                    <td><div className="price">Free</div><br /></td>
                                    <td>
                                        <div className="price">$5/month</div><br />
                                        <a href="https://namiko.moe/redirect?type=Patreon&tag=moe-pro-pro&redirectUrl=https%3A%2F%2Fwww.patreon.com%2Fjoin%2FtaiHen%2Fcheckout%3Frid%3D3752646" className="btn btn-pro">Subscribe</a>
                                    </td>
                                    <td>
                                        <div className="price">$10/month</div><br />
                                        <a href="https://namiko.moe/redirect?type=Patreon&tag=moe-pro-proplus&redirectUrl=https%3A%2F%2Fwww.patreon.com%2Fjoin%2FtaiHen%2Fcheckout%3Frid%3D3359095" className="btn btn-pro">Subscribe</a>
                                    </td>
                                </tr>
                            </tbody>
                        </Table>
                        <div className="col-12 mb-3 text-muted2"><i>*subscriptions are made via Patreon</i></div>
                    </div>


                    <div className="row pro-container">
                        <div className="col-12 pro-title">Server upgrades</div>
                        <br />
                        <div className="col-12 pro-description" >Upgrades for a <strong>Discord server</strong>. Active for all members of a server! Good for communities.</div>
                        <Table striped bordered variant="dark" className="pro-table">
                            <thead>
                                <tr>
                                    <th className="col-3"></th>
                                    <th className="col-2 pro-free">Free</th>
                                    <th className="col-3 pro-pro">Guild</th>
                                    <th className="col-3 pro-plus">Guild+</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>Music</td>
                                    <td>❌</td>
                                    <td>✔️</td>
                                    <td>✔️</td>
                                </tr>
                                <tr>
                                    <td>All music features</td>
                                    <td>❌</td>
                                    <td>✔️</td>
                                    <td>✔️</td>
                                </tr>
                                <tr>
                                    <td>Music queue length</td>
                                    <td>-</td>
                                    <td>100</td>
                                    <td><b>500</b></td>
                                </tr>
                                <tr>
                                    <td>Weekly cooldown</td>
                                    <td>7 days</td>
                                    <td><b>3.5 days</b></td>
                                    <td><b>3.5 days</b></td>
                                </tr>
                                <tr>
                                    <td>Subreddit follow limit</td>
                                    <td>1</td>
                                    <td>5</td>
                                    <td><b>10</b></td>
                                </tr>
                                <tr>
                                    <td>Waifu/gacha shop size</td>
                                    <td>x1</td>
                                    <td><b>x3</b></td>
                                    <td><b>x3</b></td>
                                </tr>
                                <tr>
                                    <td>Mod controlled waifu shop</td>
                                    <td>❌</td>
                                    <td>❌</td>
                                    <td><b>✔️</b></td>
                                </tr>
                                <tr>
                                    <td>Weekly boost</td>
                                    <td>-</td>
                                    <td>-</td>
                                    <td>+1000 toasties 🌟</td>
                                </tr>
                                <tr>
                                    <td>Lootbox Shop</td>
                                    <td>❌</td>
                                    <td>❌</td>
                                    <td>✔️</td>
                                </tr>
                                <tr>
                                    <td>Ship waifu command</td>
                                    <td>❌</td>
                                    <td>❌</td>
                                    <td>✔️</td>
                                </tr>
                                <tr>
                                    <td></td>
                                    <td><div className="price">Free</div><br /></td>
                                    <td>
                                        <div className="price">$5/month</div><br />
                                        <a href="https://namiko.moe/redirect?type=Patreon&tag=moe-pro-guild&redirectUrl=https%3A%2F%2Fwww.patreon.com%2Fjoin%2FtaiHen%2Fcheckout%3Frid%3D3752701" className="btn btn-pro">Subscribe</a>
                                    </td>
                                    <td>
                                        <div className="price">$10/month</div><br />
                                        <a href="https://namiko.moe/redirect?type=Patreon&tag=moe-pro-guildplus&redirectUrl=https%3A%2F%2Fwww.patreon.com%2Fjoin%2FtaiHen%2Fcheckout%3Frid%3D3752710" className="btn btn-pro">Subscribe</a>
                                    </td>
                                </tr>
                            </tbody>
                        </Table>
                        <div className="col-12 mb-3 text-muted2"><i>*subscriptions are made via Patreon</i></div>
                    </div>
                </Container>
            </>
        );
    }
}
