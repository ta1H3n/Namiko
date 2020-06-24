import React, { Component } from 'react';
import { Container } from 'reactstrap';

export class Home extends Component {
    static displayName = Home.name;

    render() {
        return (
            <>
                <div class="container-image front-page-cover">
                    <img src="/images/banner.png" className="img-banner" alt="banner"/>
                    <div class="centered">
                        <h1 className="text-center title-text"><strong>Namiko</strong></h1>
                        <h3 className="text-center title-text">Anime Powered Discord Bot</h3>
                        <div className="btn-group color-discord round-sm mt-2" role="group">
                            <div className="btn btn-static color-discord small-padding">
                                <img src="/images/Discord-Logo-White.png" className="img" />
                            </div>
                            <a href="https://discordapp.com/oauth2/authorize?client_id=418823684459855882&scope=bot&permissions=268707844" type="button" className="btn color-discord color-discord-hover">Invite&nbsp;Namiko</a>
                            <a href="https://discord.gg/W6Ru5sM" type="button" className="btn color-discord color-discord-hover">Join&nbsp;Server</a>
                        </div>
                    </div>
                </div>
                <Container>
                    <div className="full-size">
                        <div className="container mt-2">
                            <div className="row">
                                <div className="text-center">
                                    <a href="#waifus" className="btn color6 color6-hover mr-1 mt-1">Waifus</a>
                                    <a href="#banroulette" className="btn color6 color6-hover mr-1 mt-1">Banroulette</a>
                                    <a href="#user-profile" className="btn color6 color6-hover mr-1 mt-1">User Profile</a>
                                    <a href="#music" className="btn color6 color6-hover mr-1 mt-1">Music</a>
                                    <a href="#currency-and-gambling" className="btn color6 color6-hover mr-1 mt-1">Currency & Gambling</a>
                                    <a href="#reaction-images" className="btn color6 color6-hover mr-1 mt-1">Reaction Images</a>
                                    <a href="#teams" className="btn color6 color6-hover mr-1 mt-1">Teams</a>
                                    <a href="#server-settings" className="btn color6 color6-hover mr-1 mt-1">Server Settings</a>
                                    <a href="#web-commands" className="btn color6 color6-hover mr-1 mt-1">Web Commands</a>
                                </div>
                            </div>
                        </div>
                        <div className="container mt-5">
                            <div className="row">
                                <div className="col-md-5 col-sm-12 color1 round">
                                    <h2 id="waifus">Waifus</h2>
                                    <p>Namiko has high quality tiered waifus from anime, games and more. They were all carefully made by our insiders, per request and per popularity.</p>
                                </div>
                            </div>
                        </div>

                        <div className="container mt-5">
                            <div className="row">
                                <div className="col-6 color3 round">
                                    <h2 id="banroulette">Banroulette</h2>
                                    <p>Start a banroulette where users can join for a chance to be banned, while others share a pool of rewards!</p>
                                </div>
                            </div>
                        </div>

                        <div className="container mt-5">
                            <div className="row">
                                <div className="col-md-5 col-sm-12 color1 round">
                                    <h2 id="user-profile">User Profile</h2>
                                    <p>Full profile customization. Set a custom image, quote and a color for yourself. Show off your waifu on your profile!</p>
                                </div>
                            </div>
                        </div>

                        <div className="container mt-5">
                            <div className="row">
                                <div className="col-md-6 col-sm-12 color3 round">
                                    <h2 id="music">Music</h2>
                                    <p>High quality music from many sources including Youtube, Soundcloud and Mixer. Create and save your own playlists! Namiko will talk to you in voice chat ^^</p>
                                </div>
                            </div>
                        </div>

                        <div className="container mt-5">
                            <div className="row">
                                <div className="col-md-5 col-sm-12 color1 round">
                                    <h2 id="currency-and-gambling">Currency & Gambling</h2>
                                    <p>Server exclusive currency for buying waifus, gambling and trading. Complex scaling algorithms are used to provide both users and Namiko with a sufficient balance without bloating the economy!</p>
                                </div>
                            </div>
                        </div>

                        <div className="container mt-5">
                            <div className="row">
                                <div className="col-md-5 col-sm-12 color3 round">
                                    <h2 id="reaction-images">Reaction Images</h2>
                                    <p>Over 200 reaction image commands, and over 2000 images. Such as pat, hug, kick, hydrate and much more.</p>
                                </div>
                            </div>
                        </div>

                        <div className="container mt-5">
                            <div className="row">
                                <div className="col-md-4 col-sm-12 color1 round">
                                    <h2 id="web-commands">Web Commands</h2>
                                    <p>Commands using various internet services.  </p>
                                </div>
                            </div>
                        </div>
                    </div>
                </Container>
            </>
        );
    }
}
