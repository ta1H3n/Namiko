import React from 'react';

export function parseCode(code) {
    try {
        if (code === 200) {
            return null;
        }

        let result;

        if (code === 401) {
            result = <h3 className="text-muted text-center align-middle mt-5"><em>You need to login.</em></h3>
            const returnUrl = window.location.pathname;
            if (returnUrl) {
                window.location = '/authentication/login?returnUrl=' + returnUrl;
            }
            else {
                window.location = "/authentication/login";
            }
        }

        else if (code === 412) {
            result = <>
                <h3 className="row text-muted text-center align-middle mt-5"><em>Namiko is not in this guild...</em></h3>
                <a href="https://discordapp.com/oauth2/authorize?client_id=418823684459855882&scope=bot&permissions=268707844" type="button" className="row btn color-discord color-discord-hover">Invite&nbsp;Namiko</a>
            </>
        }

        else if (code === 403) {
            result = <h3 className="text-muted text-center align-middle mt-5"><em>You don't have access to this resource...</em></h3>
        }

        else if (code === 404) {
            result = <h3 className="text-muted text-center align-middle mt-5"><em>Not found...</em></h3>
        }

        else {
            result = <h3 className="text-muted text-center align-middle mt-5"><em>Something went wrong... Please try again later :)</em></h3>
        }

        return result;
    }
    catch (err) {
        return <h3 className="text-muted text-center align-middle mt-5"><em>Something went horribly wrong... Please try again later :)</em></h3>
    }
}