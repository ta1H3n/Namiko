import React, { Component } from 'react';
import { Container } from 'reactstrap';

export class Home extends Component {
    static displayName = Home.name;

    render() {
        return (
            <>
                <img src="/images/banner.png" className="img-fluid front-page-cover" />
                <Container>
                    <div className="full-size">
                        <div className="container">
                        </div>
                        <div className="container-fluid w-100">
                            <div className="row">
                                <div className="col text-center mt-1">
                                    <h1 id="namiko">Namiko</h1>
                                </div>
                            </div>

                            <div className="row">
                                <div className="col">
                                    <p><a href="https://discordapp.com/oauth2/authorize?client_id=418823684459855882&amp;scope=bot&amp;permissions=268707844">Invite Namiko to your server</a>  </p>
                                    <p>Namiko&#39;s prefix can be changed by using the <code>!sp [new_prefix]</code> command and replacing <code>[new_prefix]</code> with your desired prefix. The default is <code>!</code> and it will be used in this wiki. Mentioning Namiko can also be used as a prefix e.g. <code>@Namiko sp [new_prefix]</code>.  </p>
                                    <p>Type <code>!help</code> or <code>!help [command]</code> in a server with Namiko for a list of commands and command usage instructions.</p>
                                    <h3 id="premium">Premium</h3>
                                    <p>Namiko now has premium upgrades! Both for users and for servers! You can find them <a href="https://www.patreon.com/taiHen">on Patreon</a>.  </p>
                                </div>
                            </div>
                        </div>
                        <hr />
                        <div className="container mt-5">
                            <div className="row">
                                <div className="col-md-5 col-sm-12 color1 round">
                                    <h2 id="waifus">Waifus</h2>
                                    Namiko has high quality handmade and tiered waifus from anime, games and more. They were all carefully made by our insiders, per request and per popularity. You can ask our insiders to create a waifu for you in <a href="https://discord.gg/W6Ru5sM">Namiko Test Realm.</a>
                                </div>
                                <div className="col-md-7 col-sm-12 text-right">
                                    <table className="command-table-right">
                                        <tr>
                                            <td>Search for waifus</td>
                                            <td className="command-table-space">  -  </td>
                                            <td className="text-left"><span class="badge badge-pill color1">!waifu</span></td>
                                        </tr>
                                        <tr>
                                            <td>View a list of waifus currently for sale</td>
                                            <td className="command-table-space">  -  </td>
                                            <td className="text-left"><span class="badge badge-pill color1">!waifushop</span></td>
                                        </tr>
                                        <tr>
                                            <td>Trade waifus with other users</td>
                                            <td className="command-table-space"> - </td>
                                            <td className="text-left"><span class="badge badge-pill color1">!givewaifu</span></td>
                                        </tr>
                                        <tr>
                                            <td>View a list of waifus you have</td>
                                            <td className="command-table-space"> - </td>
                                            <td className="text-left"><span class="badge badge-pill color1">!waifus</span></td>
                                        </tr>
                                        <tr>
                                            <td>Add a waifu to your wishlist</td>
                                            <td className="command-table-space"> - </td>
                                            <td className="text-left"><span class="badge badge-pill color1">!aww</span></td>
                                        </tr>
                                        <tr>
                                            <td>View your waifu wishlist</td>
                                            <td className="command-table-space"> - </td>
                                            <td className="text-left"><span class="badge badge-pill color1">!wwl</span></td>
                                        </tr>
                                    </table>
                                </div>
                            </div>
                        </div>

                        <div className="container mt-5">
                            <div className="row">
                                <div className="col-md-4 col-sm-12">
                                    <table>
                                        <tr>
                                            <td className="text-right"><span class="badge badge-pill color3">!nbr</span></td>
                                            <td className="command-table-space"> - </td>
                                            <td>Start a new banroulette</td>
                                        </tr>
                                        <tr>
                                            <td className="text-right"><span class="badge badge-pill color3">!jbr</span></td>
                                            <td className="command-table-space"> - </td>
                                            <td>Join the banroulette</td>
                                        </tr>
                                        <tr>
                                            <td className="text-right"><span class="badge badge-pill color3">!br</span></td>
                                            <td className="command-table-space"> - </td>
                                            <td>View the details</td>
                                        </tr>
                                        <tr>
                                            <td className="text-right"><span class="badge badge-pill color3">!ebr</span></td>
                                            <td className="command-table-space"> - </td>
                                            <td>End the banroulette</td>
                                        </tr>
                                        <tr>
                                            <td className="text-right"><span class="badge badge-pill color3">!cbr</span></td>
                                            <td className="command-table-space"> - </td>
                                            <td>Cancel the banroulette</td>
                                        </tr>
                                    </table>
                                </div>
                                <div className="col-md-4 col-sm-12">
                                        Customise the ban duration (0 - no ban).<br/>
                                        The maximum and minimum amount of participants.<br/>
                                        The reward pool.<br/>
                                        And optionally set a required role to join.<br/>
                                </div>
                                <div className="col-md-4 col-sm-12 color3 round">
                                    <h2 id="banroulette">Banroulette</h2>
                                    <p>Create a banroulette in a channel where users can join for a chance to be banned, while the others share a pool of rewards.</p>
                                </div>
                            </div>
                        </div>


                        <div className="container mt-5">
                            <div className="row">
                                <div className="col-md-4 col-sm-12 color1 round">
                                    <h2 id="user-profile">User Profile</h2>
                                    <p>Full profile customization. You can set an image, quote and a color for yourself. You can also display your favorite waifu on your profile, it&#39;s the last you bought by default. The quote, color and image is shared across all servers, but the waifu is not.  </p>

                                </div>
                                <div className="col-md-8 col-sm-12">
                                    <table className="command-table-right">
                                        <tr>
                                            <td className="text-right">View yours or someone&#39;s profile</td>
                                            <td className="command-table-space"> - </td>
                                            <td><span class="badge badge-pill color1">!profile</span></td>
                                        </tr>
                                        <tr>
                                            <td className="text-right">View yours or someone&#39;s unique quote and image</td>
                                            <td className="command-table-space"> - </td>
                                            <td><span class="badge badge-pill color1">!quote</span></td>
                                        </tr>
                                        <tr>
                                            <td className="text-right">Set image</td>
                                            <td className="command-table-space"> - </td>
                                            <td><span class="badge badge-pill color1">!si</span></td>
                                        </tr>
                                        <tr>
                                            <td className="text-right">Set quote</td>
                                            <td className="command-table-space"> - </td>
                                            <td><span class="badge badge-pill color1">!sq</span></td>
                                        </tr>
                                        <tr>
                                            <td className="text-right">Set color</td>
                                            <td className="command-table-space"> - </td>
                                            <td><span class="badge badge-pill color1">!sc</span></td>
                                        </tr>
                                        <tr>
                                            <td className="text-right">Set your featured waifu on your profile</td>
                                            <td className="command-table-space"> - </td>
                                            <td><span class="badge badge-pill color1">!sfw</span></td>
                                        </tr>
                                        <tr>
                                            <td className="text-right">View yours or someone&#39;s waifus</td>
                                            <td className="command-table-space"> - </td>
                                            <td><span class="badge badge-pill color1">!waifus</span></td>
                                        </tr>
                                        <tr>
                                            <td className="text-right">Marry another user</td>
                                            <td className="command-table-space"> - </td>
                                            <td><span class="badge badge-pill color1">!marry</span></td>
                                        </tr>
                                    </table>
                                </div>
                            </div>
                        </div>

                        <div className="container color5 round mt-5">
                            <div className="row">
                                <div className="col-md-5 col-sm-12">
                                    <h2 id="currency-and-gambling">Currency and Gambling</h2>
                                    <p>The currency is Toasties, and is server exclusive. Toasties can be used to buy waifus, or trade with other users. Toasties can be gambled, and Namiko&#39;s balance acts as the casino balance - meaning that if you gamble, you win or lose toasties from Namiko&#39;s balance. Namiko starts off with a balance of 1 million. This stops absurd of amounts of toasties being won - preventing ruining of the game experience. Namiko gains a small amount of your dailies and weeklies, based on how much you have, how much she has, and the whole net amount in the server.  </p>

                                </div>
                                <div className="col-md-7 col-sm-12">
                                    <p className="mt-4">Useful commands:</p>

                                    <span class="badge badge-pill color1">!bal</span> - view a users balance.<br />
                                    <span class="badge badge-pill color1">!daily</span> - claim a daily toastie reward. The value is randomized and increases with your streak. <br />
                                    <span class="badge badge-pill color1">!weekly</span> - claim a weekly reward. The weekly is like a daily of the person with the highest streak in the server, +15 days.<br />
                                    <span class="badge badge-pill color1">!flip</span> - flip a coin for a 50/50 chance to win or lose toasties.<br />
                                    <span class="badge badge-pill color1">!blackjack</span> - play blackjack with a bet of toasties.<br />
                                    <span class="badge badge-pill color1">!give</span> - give toasties to someone else.<br />

                                    <p>If Namiko has less than 200,000 toasties, every hour she has a 1/5 chance to steal 1% of toasties from every user.</p>

                                </div>
                            </div>
                        </div>

                        <div className="container color5 round mt-5">
                            <div className="row">
                                <div className="col-md-5 col-sm-12">
                                    <h2 id="reaction-images">Reaction Images</h2>
                                    <p>There a lot of reaction images you can use by typing an image command. Such as pat, hug, kick, hydrate and much more. Use them by simply typing the prefix <code>!</code> and the image name after it e.g. <code>!pat</code>. View the list of all the image commands by typing <code>!listall</code>. You can suggest adding new images and more image commands in <a href="https://discord.gg/W6Ru5sM">Namiko Test Realm</a>, we always appreciate new additions!  </p>

                                </div>
                                <div className="col-md-7 col-sm-12">
                                    <p className="mt-4">Useful commands:</p>

                                    <span class="badge badge-pill color1">![image_name]</span> - sends a random image from all the images under the name.<br />
                                    <span class="badge badge-pill color1">!listall</span> - lists all the image commands Namiko currently has.<br />
                                    <span class="badge badge-pill color1">!all [image_name]</span> - gives a link to an imgur album with all the images in the command, and their IDs.<br />
                                    <span class="badge badge-pill color1">!image</span> - sends a specific image by it&#39;s ID.<br />
                                    <span class="badge badge-pill color1">!newimage</span> - add a server exclusive reaction image. (Requires T1 Server Premium)<br />

                                </div>
                            </div>
                        </div>

                        <div className="container color5 round mt-5">
                            <div className="row">
                                <div className="col-md-4 col-sm-12">
                                    <h2 id="teams">Teams</h2>
                                    <p>Role based teams can be made, where a <em>leader</em> can invite users to their team, and if they accept they will be given the teams role. Invites expire in 24 hours. The leader roles must be assigned manually.  </p>

                                </div>
                                <div className="col-md-8 col-sm-12">
                                    <p className="mt-4">Useful commands:</p>

                                    <span class="badge badge-pill color1">!nt</span> - creates a new team with selected roles.<br />
                                    <span class="badge badge-pill color1">!invite</span> - invite a user to your team. <span class="badge badge-pill color1">Team Leader only.</span><br />
                                    <span class="badge badge-pill color1">!join</span> - accept an invitation.<br />
                                    <span class="badge badge-pill color1">!leaveteam</span> - leave your team.<br />
                                    <span class="badge badge-pill color1">!kick</span> - kick a user from your team. <span class="badge badge-pill color1">Team Leader only.</span><br />
                                    <span class="badge badge-pill color1">!team</span> - view details about a team.<br />

                                </div>
                            </div>
                        </div>

                        <div className="container color5 round mt-5">
                            <div className="row">
                                <div className="col-md-4 col-sm-12">
                                    <h2 id="server-settings">Server Settings</h2>
                                    <p>Various settings for the server, such as welcome messages, server join logs, team join logs, public roles and blacklisting channels. </p>

                                </div>
                                <div className="col-md-8 col-sm-12">
                                    <p className="mt-4">Useful commands:</p>

                                    <span class="badge badge-pill color1">!spr</span> - sets a role as public, where anyone can gain the role by typing: <code>!role [role_name]</code>.<br />
                                    <span class="badge badge-pill color1">!role</span> - gives or removes a public role from you.<br />
                                    <span class="badge badge-pill color1">!prl</span> - lists the public roles.<br />
                                    <span class="badge badge-pill color1">!wch</span> - sets a <span class="badge badge-pill color1">Welcome Channel</span>.<br />
                                    <span class="badge badge-pill color1">!blch</span> - blacklists a channel, disabling Namiko commands in that channel.<br />
                                    <span class="badge badge-pill color1">!jch</span> - sets a <span class="badge badge-pill color1">Server Join Log</span> channel.<br />
                                    <span class="badge badge-pill color1">!tch</span> - sets a <span class="badge badge-pill color1">Team Join Log</span> channel.<br />

                                </div>
                            </div>
                        </div>

                        <div className="container color5 round mt-5">
                            <div className="row">
                                <div className="col-md-4 col-sm-12">
                                    <h2 id="web-commands">Web Commands</h2>
                                    <p>Commands using various internet services.  </p>

                                </div>
                                <div className="col-md-8 col-sm-12">
                                    <p className="mt-4">Useful commands:</p>

                                    <span class="badge badge-pill color1">!source</span> - tries to find the source of an image using SauceNao.<br />
                                    <span class="badge badge-pill color1">!iqdb</span> - tries to find the source of an image using IQDB.<br />
                                    <span class="badge badge-pill color1">!anime</span> - looks up an anime on MAL.<br />
                                    <span class="badge badge-pill color1">!manga</span> - looks up a manga on MAL.<br />
                                    <span class="badge badge-pill color1">!subreddit</span> - subscribe to hop posts from a subreddit that reach a certain upvote limit.<br />

                                </div>
                            </div>
                        </div>
                    </div>
                </Container>
            </>
        );
    }
}
