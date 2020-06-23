import React, { Component } from 'react';

export class EarlyBuildBar extends Component {
    static displayName = EarlyBuildBar.name;

    constructor(props) {
        super(props);
    }

    render() {
        return (
            <div className="alert alert-info mb-0" role="alert">
                <h5 className="text-center">This is a very early preview build of Namiko Moe - anything might and will change!</h5>
            </div>
        );
    }
}
