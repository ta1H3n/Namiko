import React, { Component } from 'react';
import { Route, Redirect, Switch } from 'react-router';
import GaTracker from './GaTracker';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { Waifus } from './components/waifus/Waifus';
import { Commands } from './components/commands/Commands';
import { Guide } from './components/commands/Guide';
import { WaifuShop } from './components/waifus/WaifuShop';
import { LoginState } from './components/authentication/LoginState';
import { User } from './components/dashbaords/User';
import { Guild } from './components/dashbaords/Guild';
import { GuildUser } from './components/dashbaords/GuildUser';

import './custom.css';

export default class App extends Component {
    static displayName = App.name;

    render() {
        const api_regex = /^\/api\/.*/
        if (api_regex.test(window.location.pathname)) {
            return <div />
        }
        else {
            return (
                <>
                    <Layout>
                        <Switch>
                            <Route exact path='/' component={GaTracker(Home)} />
                            <Route path='/waifus' component={GaTracker(Waifus)} />
                            <Route path='/waifushop/:guildId' component={GaTracker(WaifuShop)} />

                            <Route path='/commands' component={GaTracker(Commands)} />
                            <Route path='/guide' component={GaTracker(Guide)} />

                            <Route path='/user/me' component={GaTracker(User)} />
                            <Route path='/guild/:guildId/:userId' component={GaTracker(GuildUser)} />
                            <Route path='/guild/:guildId' component={GaTracker(Guild)} />

                            <Route path='/authenticationcallback' component={GaTracker(LoginState)} />

                            <Redirect to='/' />
                        </Switch>
                    </Layout>
                </>
            );
        }
    }
}