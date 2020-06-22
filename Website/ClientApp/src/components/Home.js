import React, { Component } from 'react';
import { Container } from 'reactstrap';

export class Home extends Component {
  static displayName = Home.name;

  render () {
      return (
          <>  <img src="" alt=""/>
              <div className="bg"></div>
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
            <hr/>
            <div className="container">
                <div className="row">
                    <div className="col text-center">
                    <h2 id="contents">Contents</h2>
                    </div>
                </div>
                <div className="row mt-3 d-flex justify-content-center">
                    <div className="col-4 ">
                    <a className="colorless-link" href="#waifus">
                        <div className="card border">
                            <div className="card-body card-custom text-center">
                                Waifus
                            </div>
                        </div>
                    </a>
                    <a className="colorless-link" href="#banroulette">
                        <div className="card border mt-4">
                            <div className="card-body card-custom text-center">
                            Banroulette
                            </div>
                        </div>
                    </a>
                    <a className="colorless-link" href="#user-profile">
                        <div className="card border mt-4">
                            <div className="card-body card-custom text-center">
                            User Profile
                            </div>
                        </div>
                    </a>
                    </div>
                    <div className="col-4">
                    <a className="colorless-link" href="#currency-and-gambling">
                    <div className="card border">
                            <div className="card-body card-custom text-center">
                            Currency &amp; Gambling
                            </div>
                    </div>
                    </a>
                    <a className="colorless-link" href="#reaction-images">
                    <div className="card border mt-4">
                        <div className="card-body card-custom text-center">
                            Reaction Images
                        </div>
                    </div>
                    </a>
                    <a className="colorless-link" href="#teams">
                    <div className="card border mt-4">
                        <div className="card-body card-custom text-center">
                            Teams
                        </div>
                    </div>
                    </a>
                    </div>
                    <div className="col-4">
                    <a className="colorless-link" href="#server-settings">
                    <div className="card border">
                            <div className="card-body card-custom text-center">
                            Server Settings
                            </div>
                    </div>
                    </a>
                    <a className="colorless-link" href="#web-commands">
                        <div className="card border mt-4">
                            <div className="card-body card-custom text-center">
                            Web Commands
                            </div>
                        </div>
                        </a>
                        <a className="colorless-link" href="#full-command-list">
                        <div className="card border mt-4">
                            <div className="card-body card-custom text-center">
                            Full Command List
                            </div>
                        </div>
                        </a>
                    </div>
                </div>
            </div>
            <div className="container color5 round mt-5">
                <div className="row">
                    <div className="col-4">
                    <h2 id="waifus">Waifus</h2>
                        Namiko has ~1000 handmade and tiered waifus from anime, games and more. They were all carefully made by our users, per request and per popularity. You can ask our insiders to create a waifu for you in <a href="https://discord.gg/W6Ru5sM">Namiko Test Realm.</a>
                        <p>Your last bought waifu will also appear on your profile. To see your profile use the <strong>!profile</strong> command. To change the waifu previewed on your profile you can use the <strong>!sfw</strong> command.</p>
                        
                    </div>
                    <div className="col-8">
                    <p className="mt-4">Useful commands:</p>
                    <ul>
                        <li><strong>!waifu</strong> - Searches for a waifu in the database, based on their name or source. If more than one match is found it returns a list. Searching for the short ID name will always return the specific waifu.</li>
                        <li><strong>!waifushop</strong> - Shows a list of waifus currently for sale, resets every 12 hours.</li>
                        <li><strong>!givewaifu</strong> - You can trade waifus with other users.</li>
                        <li><strong>!waifus</strong> - shows a list of the waifus you have.</li>
                        <li><strong>!aww</strong> - add a waifu to your wishlist. Namiko will dm you if the waifu goes for sale, and you will be listed on the waifu card. Limited to 5. Increase the limit to 12 with Waifu Premium.</li>
                        <li><strong>!wwl</strong> - shows yours or another users waifu wishlist.</li>
                    </ul>

                    </div>
                </div>
            </div>

            <div className="container color5 round mt-5">
                <div className="row">
                
                    <div className="col-4">
                    <h2 id="banroulette">Banroulette</h2>
                    <p>Create a banroulette in a channel where users can join for a chance to be banned, while the others share rewards from a pool of Toasties.
                        The banroulette can be fully customized: </p>

                           
                    </div>
                    <div className="col-4 mt-4">
                    
                    <ul>
                            <li>The ban duration in hours (0 - no ban)</li>
                            <li>The minimum amount of users required to finish the banroulette</li>
                            <li>The maximum amount of users</li>
                            <li>The toastie reward pool (admin only)</li>
                            <li>And optionally setting a required role to join  </li>
                        </ul> 
                    </div>
                    <div className="col-4">
                    <p className="mt-4">Useful commands:</p>
                    <ul>
                        <li><strong>!nbr</strong> - starts a new banroulette.</li>
                        <li><strong>!jbr</strong> - join the banroulette.</li>
                        <li><strong>!br</strong> - view the details.</li>
                        <li><strong>!ebr</strong> - end the banroulette.</li>
                        <li><strong>!cbr</strong> - cancel the banroulette.</li>
                    </ul>
                    </div>
                </div>
            </div>

            
            <div className="container color5 round mt-5">
                <div className="row">
                    <div className="col-4">
                    <h2 id="user-profile">User Profile</h2>
                    <p>Full profile customization. You can set an image, quote and a color for yourself. You can also display your favorite waifu on your profile, it&#39;s the last you bought by default. The quote, color and image is shared across all servers, but the waifu is not.  </p>

                    </div>
                    <div className="col-8">
                    <p className="mt-4">Useful commands:</p>
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
                </div>
            </div>

            <div className="container color5 round mt-5">
                <div className="row">
                    <div className="col-5">
                    <h2 id="currency-and-gambling">Currency and Gambling</h2>
                    <p>The currency is Toasties, and is server exclusive. Toasties can be used to buy waifus, or trade with other users. Toasties can be gambled, and Namiko&#39;s balance acts as the casino balance - meaning that if you gamble, you win or lose toasties from Namiko&#39;s balance. Namiko starts off with a balance of 1 million. This stops absurd of amounts of toasties being won - preventing ruining of the game experience. Namiko gains a small amount of your dailies and weeklies, based on how much you have, how much she has, and the whole net amount in the server.  </p>
                        
                    </div>
                    <div className="col-7">
                    <p className="mt-4">Useful commands:</p>
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
                </div>
            </div>

            <div className="container color5 round mt-5">
                <div className="row">
                    <div className="col-5">
                    <h2 id="reaction-images">Reaction Images</h2>
                    <p>There a lot of reaction images you can use by typing an image command. Such as pat, hug, kick, hydrate and much more. Use them by simply typing the prefix <code>!</code> and the image name after it e.g. <code>!pat</code>. View the list of all the image commands by typing <code>!listall</code>. You can suggest adding new images and more image commands in <a href="https://discord.gg/W6Ru5sM">Namiko Test Realm</a>, we always appreciate new additions!  </p>
                        
                    </div>
                    <div className="col-7">
                    <p className="mt-4">Useful commands:</p>
                    <ul>
                        <li><strong>![image_name]</strong> - sends a random image from all the images under the name.</li>
                        <li><strong>!listall</strong> - lists all the image commands Namiko currently has.</li>
                        <li><strong>!all [image_name]</strong> - gives a link to an imgur album with all the images in the command, and their IDs.</li>
                        <li><strong>!image</strong> - sends a specific image by it&#39;s ID.</li>
                        <li><strong>!newimage</strong> - add a server exclusive reaction image. (Requires T1 Server Premium)</li>
                    </ul>
                    </div>
                </div>
            </div>
            
            <div className="container color5 round mt-5">
                <div className="row">
                    <div className="col-4">
                    <h2 id="teams">Teams</h2>
                    <p>Role based teams can be made, where a <em>leader</em> can invite users to their team, and if they accept they will be given the teams role. Invites expire in 24 hours. The leader roles must be assigned manually.  </p>
                        
                    </div>
                    <div className="col-8">
                    <p className="mt-4">Useful commands:</p>
                    <ul>
                        <li><strong>!nt</strong> - creates a new team with selected roles.</li>
                        <li><strong>!invite</strong> - invite a user to your team. <strong>Team Leader only.</strong></li>
                        <li><strong>!join</strong> - accept an invitation.</li>
                        <li><strong>!leaveteam</strong> - leave your team.</li>
                        <li><strong>!kick</strong> - kick a user from your team. <strong>Team Leader only.</strong></li>
                        <li><strong>!team</strong> - view details about a team.</li>
                    </ul>
                    </div>
                </div>
            </div>

            <div className="container color5 round mt-5">
                <div className="row">
                    <div className="col-4">
                    <h2 id="server-settings">Server Settings</h2>
                    <p>Various settings for the server, such as welcome messages, server join logs, team join logs, public roles and blacklisting channels. </p>
                        
                    </div>
                    <div className="col-8">
                    <p className="mt-4">Useful commands:</p>
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
                </div>
            </div>

            <div className="container color5 round mt-5">
                <div className="row">
                    <div className="col-4">
                    <h2 id="web-commands">Web Commands</h2>
                    <p>Commands using various internet services.  </p>
                        
                    </div>
                    <div className="col-8">
                    <p className="mt-4">Useful commands:</p>
                    <ul>
                        <li><strong>!source</strong> - tries to find the source of an image using SauceNao.</li>
                        <li><strong>!iqdb</strong> - tries to find the source of an image using IQDB.</li>
                        <li><strong>!anime</strong> - looks up an anime on MAL.</li>
                        <li><strong>!manga</strong> - looks up a manga on MAL.</li>
                        <li><strong>!subreddit</strong> - subscribe to hop posts from a subreddit that reach a certain upvote limit.</li>
                    </ul>
                    </div>
                </div>
            </div>
                  </div>
                  </Container>
        </>
    );
  }
}
