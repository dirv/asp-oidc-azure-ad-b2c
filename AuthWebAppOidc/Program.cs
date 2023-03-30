using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllersWithViews();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        // This next line means that config is taken from appsettings.json
        // (or appsettings.Development.json)
        builder.Configuration.Bind("Oidc", options);
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;
        options.CallbackPath = "/callback";
        // TODO: this next line doesn't work, i don't think Azure AD B2C supports this, but double check
        options.GetClaimsFromUserInfoEndpoint = true;
        // TODO: default has "profile" which won't work
        options.Scope.Clear();
        options.Scope.Add("openid");
        // TODO: figure out why blanking out the client secret is necessary
        options.Events.OnAuthorizationCodeReceived = context => { context.TokenEndpointRequest.ClientSecret = null; return Task.CompletedTask; };
        options.Events.OnAuthenticationFailed = context =>
            {
                context.HandleResponse();

                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain";
                // Debug only, in production do not share exceptions with the remote host.
                return context.Response.WriteAsync(context.Exception.ToString());
                //return c.Response.WriteAsync("An error occurred processing your authentication.");
            };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// DTI Added here
app.UseCookiePolicy(); // Before UseAuthentication or anything else that writes cookies.
app.UseAuthorization();
app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/protected", (ClaimsPrincipal user) => $"Hello {user.Identity?.Name}!")
    .RequireAuthorization();

app.Run();

