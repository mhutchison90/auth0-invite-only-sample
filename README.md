# Invite Only Sample

Sample showing an invite only workflow. Users will not be able to sign-in, but provisioning will be managed by the (customer) administrators.

## Getting Started

Read through this document (todo!) to understand how we're tuning Auth0 to prevent sign-ups and work with an invite-only workflow (through provisioning from the application's backend). In addition to that you'll need to specify your credentials and the connection in the Web.config


    <!-- Auth0 Settings -->
    <add key="auth0:ClientId" value="vg1EfxJRLmSqOmyclientid" />
    <add key="auth0:ClientSecret" value="something-here-KgAlSCekALAkmM6zOK0dZ6chlzxnrWMnS9AIpapt5W" />
    <add key="auth0:Domain" value="me.auth0.com" />
    <add key="auth0:Connection" value="Username-Password-Authentication"/>

## What is Auth0?

Auth0 helps you to:

* Add authentication with [multiple authentication sources](https://docs.auth0.com/identityproviders), either social like **Google, Facebook, Microsoft Account, LinkedIn, GitHub, Twitter, Box, Salesforce, amont others**, or enterprise identity systems like **Windows Azure AD, Google Apps, Active Directory, ADFS or any SAML Identity Provider**.
* Add authentication through more traditional **[username/password databases](https://docs.auth0.com/mysql-connection-tutorial)**.
* Add support for **[linking different user accounts](https://docs.auth0.com/link-accounts)** with the same user.
* Support for generating signed [Json Web Tokens](https://docs.auth0.com/jwt) to call your APIs and **flow the user identity** securely.
* Analytics of how, when and where users are logging in.
* Pull data from other sources and add it to the user profile, through [JavaScript rules](https://docs.auth0.com/rules).

## Create a free account in Auth0

1. Go to [Auth0](https://auth0.com) and click Sign Up.
2. Use Google, GitHub or Microsoft Account to login.

## Issue Reporting

If you have found a bug or if you have a feature request, please report them at this repository issues section. Please do not report security vulnerabilities on the public GitHub issue tracker. The [Responsible Disclosure Program](https://auth0.com/whitehat) details the procedure for disclosing security issues.

## Author

[Auth0](auth0.com)

## License

This project is licensed under the MIT license. See the [LICENSE](LICENSE.txt) file for more info.
