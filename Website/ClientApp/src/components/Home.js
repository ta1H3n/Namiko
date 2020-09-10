import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { Link } from 'react-router-dom';

export class Home extends Component {
    static displayName = Home.name;

    render() {
        return (
            <>
                <div className="container-header">
                    <video className="video-banner" muted autoPlay loop id="vid">
                        <source src="images/video.webm" type="video/webm" />
                        <source src="images/video.mp4" type="video/mp4" />
                        Sorry, your browser does not support html5 video.
                    </video>
                    <div className="centered">
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
                        <div className="container mt-4">
                            <div className="row module-row">
                                <div className="col-md-5 col-sm-12">
                                    <div className="color1 round module-left">
                                        <h2 id="waifus">Waifus</h2>
                                        <p>Namiko has high quality tiered waifus from anime, games and more. They were all carefully made by our insiders, per request and per popularity.</p>
                                    </div>
                                </div>
                                <div className="col-md-7 col-sm-12">
                                    <img src="/images/waifus.png" className="img-module horizontal-center" alt="" />
                                </div>
                            </div>
                        </div>

                        <div className="container mt-4">
                            <div className="row module-row">
                                <div className="col-md-7 col-sm-12">
                                    <img src="/images/banroulette.png" className="img-module horizontal-center" alt="" />
                                </div>
                                <div className="col-md-5 col-sm-12">
                                    <div className="color3 round module-right">
                                        <h2 id="banroulette">Banroulette</h2>
                                        <p>Start a banroulette where users can join for a chance to be banned, while others share a pool of rewards!</p>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="container mt-4">
                            <div className="row module-row">
                                <div className="col-md-5 col-sm-12">
                                    <div className="color1 round module-left">
                                        <h2 id="user-profile">User Profile</h2>
                                        <p>Full profile customization. Set a custom image, quote and a color for yourself. Show off your waifu on your profile!</p>
                                    </div>
                                </div>
                                <div className="col-md-7 col-sm-12">
                                    <img src="/images/profile.png" className="img-module horizontal-center" alt="" />
                                </div>
                            </div>
                        </div>

                        <div className="container mt-4">
                            <div className="row module-row">
                                <div className="col-md-7 col-sm-12">
                                    <img src="/images/music.png" className="img-module horizontal-center" alt="" />
                                </div>
                                <div className="col-md-5 col-sm-12">
                                    <div className="color3 round module-right">
                                        <h2 id="music">Music</h2>
                                        <p>High quality music from many sources including Youtube, Soundcloud and Mixer. Create and save your own playlists! Namiko will talk to you in voice chat ^^</p>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="container mt-4">
                            <div className="row module-row">
                                <div className="col-md-5 col-sm-12">
                                    <div className="color1 round module-left">
                                        <h2 id="currency-and-gambling">Currency & Gambling</h2>
                                        <p>Server exclusive currency for buying waifus, gambling and trading. Complex scaling algorithms are used to provide both users and Namiko with a sufficient balance without bloating the economy!</p>
                                    </div>
                                </div>
                                <div className="col-md-7 col-sm-12">
                                    <img src="/images/currency.png" className="img-module horizontal-center" alt="" />
                                </div>
                            </div>
                        </div>

                        <div className="container mt-4">
                            <div className="row module-row">
                                <div className="col-md-7 col-sm-12">
                                    <img src="/images/reaction-images.png" className="img-module horizontal-center" alt="" />
                                </div>
                                <div className="col-md-5 col-sm-12">
                                    <div className="color3 round module-right">
                                        <h2 id="reaction-images">Reaction Images</h2>
                                        <p>Over 200 reaction image commands, and over 2000 images. Such as pat, hug, kick, hydrate and much more!</p>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="container mt-4">
                            <div className="row module-row">
                                <div className="col-md-5 col-sm-12">
                                    <div className="color1 round module-left">
                                        <h2 id="web-commands">Web Commands</h2>
                                        <p>Commands using various internet services. Subscribe to a subreddit, search for anime, get image sources and more!</p>
                                    </div>
                                </div>
                                <div className="col-md-7 col-sm-12">
                                    <img src="/images/web.png" className="img-module horizontal-center" alt="" />
                                </div>
                            </div>
                        </div>
                </Container>
                <div className="mt-5 home-footer">
                    <Container>
                        <div className="container row mt-4 mr-0 ml-0">
                            <div className="horizontal-center text-center">
                                <h1 className="title-text"><strong>Try Namiko Now!</strong></h1>
                            </div>
                        </div>
                        <div className="container row mt-2 mr-0 ml-0">
                            <div className="horizontal-center">
                                <div className="btn-group color-discord color-discord-hover round-sm mt-2 mr-2" role="group">
                                    <div className="btn btn-static color-discord color-discord-hover small-padding">
                                        <img src="/images/Discord-Logo-White.png" className="img" />
                                    </div>
                                    <a href="https://discordapp.com/oauth2/authorize?client_id=418823684459855882&scope=bot&permissions=268707844" type="button" className="btn color-discord color-discord-hover">Invite&nbsp;Namiko</a>
                                </div>
                            </div>
                        </div>
                        <div className="container row mt-2 mr-0 ml-0">
                            <div className="horizontal-center">
                                <Link type="button" className="btn btn-blank mr-2 mt-2" to="/Commands">Commands</Link>
                                <a href="https://www.patreon.com/taiHen" className="btn btn-blank mr-2 mt-2">Get&nbsp;Pro</a>
                                <Link type="button" className="btn btn-blank mr-2 mt-2" to="/User/Me">Web&nbsp;dashboard</Link>
                                <Link type="button" className="btn btn-blank mr-2 mt-2" to="/Waifus">Waifus</Link>
                            </div>
                        </div>
                    </Container>
                </div>
            </>
        );
    }
}
