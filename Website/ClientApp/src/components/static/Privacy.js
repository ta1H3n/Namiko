import React, { Component } from 'react';
import { Container } from 'reactstrap';
import './static.css';

export class Privacy extends Component {
    static displayName = Privacy.name;

    render() {
        return (
            <div className="box">
                <Container>
                    <div id="c-static">
                        <div id="a-terms-of-service">
                            <h1>Terms of Service</h1>
                            <p>Last updated: October 4, 2020</p>
                            <p>These Terms of Service ("Terms") govern your access to and use of our
                            Site and Services. By accessing or using our Site or Services, you agree to be bound by
                            these Terms. If you do not agree to these Terms, you may not access or use
                            the Site or Services.</p>

                            <h4>User-Generated Content</h4>

                            <p>All content submitted to the Site directly or by other means, including text, links, translations,
                            images, videos, animations, songs, games, and any other materials
                            (collectively referred to as "Content") is the sole responsibility of the
                            person who submitted the Content. We take no responsibility for Content
                            submitted to the Site by users.</p>

                            <p>Any Content you submit to the Site must abide by these Terms of Service. Although we have
                            no obligation to monitor, review, or moderate any Content submitted to the
                            Site, we may, in our sole discretion, delete or remove access to Content at
                            any time and for any reason, including for a violation of these Terms,
                            or if you otherwise create liability for us.</p>

                            <p>You understand that by using the Site, you may be exposed to Content
                            that might be considered offensive, harmful, obscene, inaccurate,
                            mislabeled, mistagged, unlawful in your jurisdiction, or otherwise
                            inappropriate. The use of any Content posted on the Site or obtained by you
                            through the Site is at your own risk. We do not endorse, support, represent
                            or guarantee the completeness, truthfulness, accuracy, or reliability of
                            any Content or communications posted on the Site or endorse any opinions
                            expressed via the Site.</p>

                            <p>By submitting, posting, or uploading any Content to the Site, you
                            represent and warrant that you have all rights, power, and authority
                            necessary to grant the rights to Your Content contained within these Terms.
                            You agree you are responsible for any Content you provide to the Site,
                            including compliance with any applicable laws, rules, and regulations.
                            Because you alone are responsible for Your Content, you may expose yourself
                            to liability if you post or share Content without all necessary rights.</p>

                            <p>By submitting Content to the Site, you grant Namiko Moe a
                            worldwide, royalty-free, perpetual, irrevocable, non-exclusive,
                            transferable, and sublicensable license to use, copy, modify, adapt,
                            distribute, create derivative works from, allow downloads of, and display
                            Your Content in all media formats and channels now known or later
                            developed.</p>

                            <h4>Copyright Infringment</h4>

                            <p>If you believe any Content on the Site infringes upon your copyright,
                            you may send an email to the <a href="mailto:namikomoe@gmail.com">webmaster</a> with
                            the following pieces of information:</p>

                            <ul>
                                <li>A list of links to each post you believe infringes on your copyright.</li>
                                <li>Your contact information.</li>
                                <li>Proof of your identity.</li>
                            </ul>

                            <p>Acceptable proof of identity includes any of the following:</p>

                            <ul>
                                <li>
                                    An email from an email address listed in one of your social media
                                    profiles.
                                </li>
                                <li>
                                    A copy or screenshot of the original .psd file or other source file
                                    of one of your works.
                                </li>
                            </ul>

                            <h4>Miscellaneous</h4>

                            <p>"Namiko Moe", "we", or "us" refers to Namiko Moe, its
                            successors, and its assigns. "You" refers to any person who has consented
                            to these terms or has become contractually bound to them, whether such
                            person is identified or not at the time.</p>

                            <p>These Terms constitute the entire agreement between you and us regarding
                            your access to and use of the Services. Our failure to exercise or enforce
                            any right or provision of these Terms will not operate as a waiver of such
                            right or provision. If any provision of these Terms is, for any reason,
                            held to be illegal, invalid or unenforceable, the rest of the Terms will
                            remain in effect. You may not assign or transfer any of your rights or
                            obligations under these Terms without our consent. We may freely assign
                            these Terms.</p>
                        </div>
                    </div>
                </Container>
            </div>
        );
    }
}
