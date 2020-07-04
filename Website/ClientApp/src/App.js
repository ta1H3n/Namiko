import React, { Component } from 'react';
import { Route, Redirect, Switch } from 'react-router';
import GaTracker from './GaTracker';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { Waifus } from './components/waifus/Waifus'
import { Commands } from './components/commands/Commands'
import { WaifuShop } from './components/waifus/WaifuShop'

import './custom.css'

export default class App extends Component {
    static displayName = App.name;

    render() {
        return (
            <>
                <Layout>
                    <Switch>
                        <Route exact path='/' component={GaTracker(Home)} />
                        <Route path='/waifus' component={GaTracker(Waifus)} />
                        <Route path='/commands' component={GaTracker(Commands)} />
                        <Route path='/waifushop/:handle' component={GaTracker(WaifuShop)} />

                        <Redirect to='/' />
                    </Switch>
                </Layout>
            </>
        );
    }
}