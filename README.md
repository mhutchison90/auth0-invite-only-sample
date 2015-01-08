# Invite Only Sample

Sample showing an invite only workflow. Users will not be able to sign-in, but provisioning will be managed by the (customer) administrators.

## Getting Started

Read through this document (todo!) to understand how we're tuning Auth0 to prevent sign-ups and work with an invite-only workflow (through provisioning from the application's backend). In addition to that you'll need to specify your credentials and the connection in the Web.config


    <!-- Auth0 Settings -->
    <add key="auth0:ClientId" value="vg1EfxJRLmSqOmyclientid" />
    <add key="auth0:ClientSecret" value="something-here-KgAlSCekALAkmM6zOK0dZ6chlzxnrWMnS9AIpapt5W" />
    <add key="auth0:Domain" value="me.auth0.com" />
    <add key="auth0:Connection" value="Username-Password-Authentication"/>
