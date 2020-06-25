import React, { Component } from 'react';
import { Route, Redirect, Switch } from 'react-router';
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
                <script async src="https://www.googletagmanager.com/gtag/js?id=UA-142170725-2" />
                <script>{injectGA()}</script>
                <Layout>
                    <Switch>
                        <Route exact path='/' component={Home} />
                        <Route path='/waifus' component={Waifus} />
                        <Route path='/commands' component={Commands} />
                        <Route path='/waifushop/:handle' component={WaifuShop} />

                        <Redirect to='/' />
                    </Switch>
                </Layout>
            </>
        );
    }
}

const injectGA = () => {
    if (typeof window == 'undefined') {
        return;
    }
    window.dataLayer = window.dataLayer || [];
    function gtag() {
        window.dataLayer.push(arguments);
    }
    gtag('js', new Date());

    gtag('config', 'UA-142170725-2');
};