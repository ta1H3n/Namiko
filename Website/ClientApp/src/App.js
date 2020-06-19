import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { Waifus } from './components/waifus/Waifus'
import { WaifuShop } from './components/waifus/WaifuShop'

import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/waifus' component={Waifus} />
        <Route path='/waifushop/:handle' component={WaifuShop} />
      </Layout>
    );
  }
}
