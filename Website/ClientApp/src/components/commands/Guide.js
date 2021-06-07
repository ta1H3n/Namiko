import React, { Component } from 'react';
import './Commands.css';
import { TitleBar } from '../shared/TitleBar';

export class Guide extends Component {
    static displayName = Guide.name;

    render() {
        return (
            <>
                <TitleBar title="Guide" />
                <div className="container-guide">
                    <div className="guide-module-title" id="namiko">Namiko Usage Guides</div>
                    <div className="guide-module">
                        <p>This is a small collection of guides for each of the modules Namiko has. It includes the most useful commands to get started with Namiko and explains some of her functionality. Recommended to read for beginners and advanced users alike!</p>
                    </div>
                    <div className="row guide-module-button">
                        <div className="text-center horizontal-center">
                            <a href="Guide#waifus" className="btn btn-blank mr-1 mt-1">Waifus</a>
                            <a href="Guide#banroulette" className="btn btn-blank mr-1 mt-1">Banroulette</a>
                            <a href="Guide#music" className="btn btn-blank mr-1 mt-1">Music</a>
                            <a href="Guide#user-profile" className="btn btn-blank mr-1 mt-1">User Profile</a>
                            <a href="Guide#currency-and-gambling" className="btn btn-blank mr-1 mt-1">Currency &amp; Gambling</a>
                            <a href="Guide#reaction-images" className="btn btn-blank mr-1 mt-1">Reaction Images</a>
                            <a href="Guide#teams" className="btn btn-blank mr-1 mt-1">Teams</a>
                            <a href="Guide#server-settings" className="btn btn-blank mr-1 mt-1">Server Settings</a>
                            <a href="Guide#web-commands" className="btn btn-blank mr-1 mt-1">Web Commands</a>
                        </div>
                    </div>
                </div>
                <div className="container-guide">
                    <div className="guide-module-header" id="waifus">Waifus</div>
                    <div className="guide-module">
                        Namiko has ~1000 handmade and tiered waifus from anime, games and more. They were all carefully made by our users, per request and per popularity. You can ask our insiders to create a waifu for you in <a href="https://discord.gg/W6Ru5sM">Namiko Test Realm.</a>

                        <p>Useful commands:</p>
                        <ul>
                            <li><strong>!waifu</strong> - Searches for a waifu in the database, based on their name or source. If more than one match is found it returns a list. Searching for the short ID name will always return the specific waifu.</li>
                            <li><strong>!waifushop</strong> - Shows a list of waifus currently for sale, resets every 12 hours.</li>
                            <li><strong>!givewaifu</strong> - You can trade waifus with other users.</li>
                            <li><strong>!waifus</strong> - shows a list of the waifus you have.</li>
                            <li><strong>!aww</strong> - add a waifu to your wishlist. Namiko will dm you if the waifu goes for sale, and you will be listed on the waifu card. Limited to 5. Increase the limit to 12 with Waifu Premium.</li>
                            <li><strong>!wwl</strong> - shows yours or another users waifu wishlist.</li>
                        </ul>
                        <p>Your last bought waifu will also appear on your profile. To see your profile use the <strong>!profile</strong> command. To change the waifu previewed on your profile you can use the <strong>!sfw</strong> command.</p>
                    </div>
                    <div className="guide-module-header" id="banroulette">Banroulette</div>
                    <div className="guide-module">
                        <p>Create a banroulette in a channel where users can join for a chance to be banned, while the others share rewards from a pool of Toasties. The banroulette can be fully customized: </p>
                        <ul>
                            <li>The ban duration in hours (0 - no ban)</li>
                            <li>The minimum amount of users required to finish the banroulette</li>
                            <li>The maximum amount of users</li>
                            <li>The toastie reward pool (admin only)</li>
                            <li>And optionally setting a required role to join  </li>
                        </ul>
                        <p>Useful commands:</p>
                        <ul>
                            <li><strong>!nbr</strong> - starts a new banroulette.</li>
                            <li><strong>!jbr</strong> - join the banroulette.</li>
                            <li><strong>!br</strong> - view the details.</li>
                            <li><strong>!ebr</strong> - end the banroulette.</li>
                            <li><strong>!cbr</strong> - cancel the banroulette.</li>
                        </ul>
                        <p>Namiko will guide you through the process, it&#39;s really simple!</p>
                    </div>
                    <div className="guide-module-header" id="music">Music</div>
                    <div className="guide-module">
                        <p>High quality music module available for 5$/month. Supports many sources including youtube, twitch and mixer. Unlimited song length and up to 500 song queue length. Volume control, repeat, loop and shuffle. Possibility to limit music controls to a role. Create and save your own playlists. And Namiko will talk to you in voice chat ^^</p>
                        <p>Useful Commands:</p>
                        <ul>
                            <li><strong>!play</strong> - play a song. Supports links or a search.</li>
                            <li><strong>!quickplay</strong> - automatically picks the first result of search.</li>
                            <li><strong>!playnext</strong> - same as play but puts the song in front of the queue.</li>
                            <li><strong>!skip</strong> - skip a song.</li>
                            <li><strong>!loop</strong> - loop the queue.</li>
                            <li><strong>!saveplaylist</strong> - saves the current queue as a playlist.</li>
                            <li><strong>!loadplaylist</strong> - load a saved playlist into the queue.</li>
                            <li><strong>!volume</strong> - control music volume.</li>
                        </ul>
                    </div>
                    <div className="guide-module-header" id="user-profile">User Profile</div>
                    <div className="guide-module">
                        <p>Full profile customization. You can set an image, quote and a color for yourself. You can also display your favorite waifu on your profile, it&#39;s the last you bought by default. The quote, color and image is shared across all servers, but the waifu is not.  </p>
                        <p>Useful Commands:</p>
                        <ul>
                            <li><strong>!profile</strong> - view yours or someone&#39;s profile.</li>
                            <li><strong>!quote</strong> - view yours or someone&#39;s unique quote and image.</li>
                            <li><strong>!si</strong> - set image.</li>
                            <li><strong>!sq</strong> - set quote.</li>
                            <li><strong>!sc</strong> - set color. e.g. <code>!sc dark blue</code> or <code>!sc #121212</code> - hex value.</li>
                            <li><strong>!sfw</strong> - set your featured waifu on your profile.</li>
                            <li><strong>!waifus</strong> - view yours or someone&#39;s waifus.</li>
                            <li><strong>!marry</strong> - marry another user.</li>
                        </ul>
                    </div>
                    <div className="guide-module-header" id="currency-and-gambling">Currency & Gambling</div>
                    <div className="guide-module">
                        <p>The currency is Toasties, and is server exclusive. Toasties can be used to buy waifus, or trade with other users. Toasties can be gambled, and Namiko&#39;s balance acts as the casino balance - meaning that if you gamble, you win or lose toasties from Namiko&#39;s balance. Namiko starts off with a balance of 1 million. This stops absurd of amounts of toasties being won - keeping the economy more balanced. Namiko gains a small amount of your dailies and weeklies, based on how much you have, how much she has, and the whole net amount in the server.  </p>
                        <p>Useful Commands:</p>
                        <ul>
                            <li><strong>!bal</strong> - view a users balance.</li>
                            <li><strong>!daily</strong> - claim a daily toastie reward. The value is randomized and increases with your streak. </li>
                            <li><strong>!weekly</strong> - claim a weekly reward. The weekly is like a daily of the person with the highest streak in the server, +15 days.</li>
                            <li><strong>!flip</strong> - flip a coin for a 50/50 chance to win or lose toasties.</li>
                            <li><strong>!blackjack</strong> - play blackjack with a bet of toasties.</li>
                            <li><strong>!give</strong> - give toasties to someone else.</li>
                        </ul>
                        <p>If Namiko has less than 200,000 toasties, every hour she has a 1/5 chance to steal 1% of toasties from every user.</p>
                    </div>
                    <div className="guide-module-header" id="reaction-images">Reaction Images</div>
                    <div className="guide-module">
                        <p>There a lot of reaction images you can use by typing an image command. Such as pat, hug, kick, hydrate and much more. Use them by simply typing the prefix <code>!</code> and the image name after it e.g. <code>!pat</code>. View the list of all the image commands by typing <code>!listall</code>. You can suggest adding new images and more image commands in <a href="https://discord.gg/W6Ru5sM">Namiko Test Realm</a>, we always appreciate new additions!  </p>
                        <p>Useful Commands:</p>
                        <ul>
                            <li><strong>![image_name]</strong> - sends a random image from all the images under the name.</li>
                            <li><strong>!listall</strong> - lists all the image commands Namiko currently has.</li>
                            <li><strong>!all [image_name]</strong> - gives a link to an imgur album with all the images in the command, and their IDs.</li>
                            <li><strong>!image</strong> - sends a specific image by it&#39;s ID.</li>
                            <li><strong>!newimage</strong> - add a server exclusive reaction image. (Requires T1 Server Premium)</li>
                        </ul>
                    </div>
                    <div className="guide-module-header" id="teams">Teams</div>
                    <div className="guide-module">
                        <p>Role based teams can be made, where a <em>leader</em> can invite users to their team, and if they accept they will be given the teams role. Invites expire in 24 hours. The leader roles must be assigned manually.  </p>
                        <p>Useful Commands:</p>
                        <ul>
                            <li><strong>!nt</strong> - creates a new team with selected roles.</li>
                            <li><strong>!invite</strong> - invite a user to your team. <strong>Team Leader only.</strong></li>
                            <li><strong>!join</strong> - accept an invitation.</li>
                            <li><strong>!leaveteam</strong> - leave your team.</li>
                            <li><strong>!kick</strong> - kick a user from your team. <strong>Team Leader only.</strong></li>
                            <li><strong>!team</strong> - view details about a team.</li>
                        </ul>
                    </div>
                    <div className="guide-module-header" id="server-settings">Server Settings</div>
                    <div className="guide-module">
                        <p>Various settings for the server, such as welcome messages, server join logs, team join logs, public roles and blacklisting channels. </p>
                        <p>Useful Commands:</p>
                        <ul>
                            <li><strong>!spr</strong> - sets a role as public, where anyone can gain the role by typing: <code>!role [role_name]</code>.</li>
                            <li><strong>!role</strong> - gives or removes a public role from you.</li>
                            <li><strong>!prl</strong> - lists the public roles.</li>
                            <li><strong>!wch</strong> - sets a <strong>Welcome Channel</strong>.</li>
                            <li><strong>!blch</strong> - blacklists a channel, disabling Namiko commands in that channel.</li>
                            <li><strong>!jch</strong> - sets a <strong>Server Join Log</strong> channel.</li>
                            <li><strong>!tch</strong> - sets a <strong>Team Join Log</strong> channel.</li>
                        </ul>
                    </div>
                    <div className="guide-module-header" id="web-commands">Web Commands</div>
                    <div className="guide-module">
                        <p>Commands using various internet services.  </p>
                        <p>Useful Commands:</p>
                        <ul>
                            <li><strong>!source</strong> - tries to find the source of an image using SauceNao.</li>
                            <li><strong>!iqdb</strong> - tries to find the source of an image using IQDB.</li>
                            <li><strong>!anime</strong> - looks up an anime on MAL.</li>
                            <li><strong>!manga</strong> - looks up a manga on MAL.</li>
                            <li><strong>!subreddit</strong> - subscribe to hop posts from a subreddit that reach a certain upvote limit.</li>
                        </ul>
                    </div>
                </div>
            </>
        );
    }
}
