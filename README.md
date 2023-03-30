# asp-oidc-azure-ad-b2c

This is a sample ASP.NET application that uses the Microsoft.AspNetCore.Authentication.OpenIdConnect namespace to connect to Azure AD B2C. The main aim is to prove that Azure AD B2C does indeed act like a standard OIDC (OpenID Connect) provider.

## TODO

* Figure out and document the most appropriate values for `ResponseCode`, `SaveTokens` and `Scopes` given various Azure AD B2C configurations. (For example: a plain App Registration will _not_ have a `profile` scope. Should it?)
* Figure out the client secret. For which flows is the necessary?
* Figure out how an elegant way to deal with authentication failures, particulary in production environments.
* Figure out if there's a way to mark a method with the `[Authorize]` attribute without specifying the `AuthenticationSchemes` property.
* How to handle authorisation given various claims.

## Important things I've learned

The authentication framework provides the mechanism for challenging users, for handling OAuth 2.0 callbacks and for checking scopes. Most of it hooks together using the `Authorize` attribute: marking a method as that means that the user will be challenged to authenticate if they aren't already.

However, it's not enough to simply mark a method as `[Authorize]` because the default scheme will be the cookie scheme. I found I got stuck in an auth loop without having this:

```csharp
[Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
```
