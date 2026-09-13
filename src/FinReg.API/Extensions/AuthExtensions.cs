using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FinReg.API.Extensions;

public static class AuthExtensions
{
    public const string ReadAccess = "FinReg.ReadAccess";
    public const string ComplianceReview = "FinReg.ComplianceReview";
    public const string UnavailableOperation = "FinReg.UnavailableOperation";

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret not configured.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }

    public static IServiceCollection AddFinRegAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(ReadAccess, policy => policy.RequireRole("Auditor", "ComplianceOfficer"));
            options.AddPolicy(ComplianceReview, policy => policy.RequireRole("ComplianceOfficer"));
            // No operational role exists in the seeded model. Unsupported mutations stay closed.
            options.AddPolicy(UnavailableOperation, policy => policy.RequireAssertion(_ => false));
        });

        return services;
    }
}
