import React, { Component } from 'react';
import { Route, Redirect, Switch } from 'react-router';
import GaTracker from './GaTracker';
import { Layout } from './components/Layout';
import { Home } from './components/static/Home';
import { Waifus } from './components/waifus/Waifus';
import { Commands } from './components/commands/Commands';
import { Guide } from './components/commands/Guide';
import { WaifuShop } from './components/waifus/WaifuShop';
import { LoginState } from './components/authentication/LoginState';
import { User } from './components/dashbaords/User';
import { GuildUser } from './components/dashbaords/GuildUser';
import { Privacy } from './components/static/Privacy';
import { Contact } from './components/static/Contact';
import { Pro } from './components/static/Pro';

import './custom.css';

export default class App extends Component {
    static displayName = App.name;

    render() {
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

                        <Route path='/privacy' component={GaTracker(Privacy)} />
                        <Route path='/contact' component={GaTracker(Contact)} />
                        <Route path='/pro' component={GaTracker(Pro)} />

                        <Route path='/authenticationcallback' component={GaTracker(LoginState)} />

                        <Redirect to='/' />
                    </Switch>
                </Layout>
            </>
        );
    }
}