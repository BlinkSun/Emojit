using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EmojitServer.Application.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EmojitServer.Api.Configuration;

/// <summary>
/// Configures <see cref="JwtBearerOptions"/> using the application's <see cref="JwtOptions"/> settings.
/// </summary>
public sealed class JwtBearerOptionsConfigurator : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly ILogger<JwtBearerOptionsConfigurator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtBearerOptionsConfigurator"/> class.
    /// </summary>
    /// <param name="jwtOptions">The JWT configuration accessor.</param>
    /// <param name="logger">The logger instance.</param>
    public JwtBearerOptionsConfigurator(IOptions<JwtOptions> jwtOptions, ILogger<JwtBearerOptionsConfigurator> logger)
    {
        _jwtOptions = jwtOptions;
        _logger = logger;
    }

    /// <inheritdoc />
    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }

    /// <inheritdoc />
    public void Configure(JwtBearerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        JwtOptions configuration = _jwtOptions.Value ?? throw new InvalidOperationException("JWT configuration is missing.");
        configuration.Validate();

        options.RequireHttpsMetadata = configuration.RequireHttpsMetadata;
        options.SaveToken = true;
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuration.Issuer,
            ValidateAudience = true,
            ValidAudience = configuration.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(GetSigningKeyBytes(configuration.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(configuration.ClockSkewInSeconds),
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimTypes.Role,
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is not null)
                {
                    _logger.LogWarning(context.Exception, "JWT authentication failed for request {Path}.", context.Request.Path);
                }

                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                HttpRequest request = context.HttpContext.Request;
                string? accessToken = request.Query["access_token"];
                PathString requestPath = request.Path;

                if (!string.IsNullOrEmpty(accessToken) && requestPath.StartsWithSegments("/hubs/game", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
        };
}

    private static byte[] GetSigningKeyBytes(string signingKey)
    {
        try
        {
            return Convert.FromBase64String(signingKey);
        }
        catch (FormatException)
        {
            return Encoding.UTF8.GetBytes(signingKey);
        }
    }
}
