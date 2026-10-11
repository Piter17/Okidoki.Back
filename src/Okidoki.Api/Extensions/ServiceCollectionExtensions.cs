using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Okidoki.Api.Configuration;
using Okidoki.Api.Hubs;
using Okidoki.Api.Json;
using Okidoki.App.Interfaces.Repositories;
using Okidoki.App.Interfaces.Services;
using Okidoki.App.Interfaces.Services.RealTime;
using Okidoki.App.Services;
using Okidoki.Domain.Entities;
using Okidoki.Infrastructure.Data;
using Okidoki.Infrastructure.RealTime;
using Okidoki.Infrastructure.Repositories;
using Okidoki.Infrastructure.Security;
using Okidoki.Infrastructure.Storage;
using System.Security.Claims;

namespace Okidoki.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureOptions<JsonOptionsSetup>();

        var connectionString = configuration.GetConnectionString("AppDb")
            ?? throw new InvalidOperationException("ConnectionStrings:AppDb must be configured.");

        var storageConnectionString = configuration.GetConnectionString("BlobStorage")
            ?? throw new InvalidOperationException("ConnectionStrings:BlobStorage must be configured.");

        var sqlConnectionString = new SqlConnectionStringBuilder(connectionString)
        {
            MultipleActiveResultSets = true,
        }.ConnectionString;

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(sqlConnectionString));

        services.Configure<IdentityOptions>(configuration.GetSection("Identity"));
        services.Configure<AnonymousRegisterOptions>(configuration.GetSection("AnonymousRegister"));

        services.AddIdentityCore<Identity>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddApiEndpoints();

        services.AddAuthentication().ConfigureBearerToken();

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(IdentityConstants.BearerScheme)
                .Build();
            options.FallbackPolicy = options.DefaultPolicy;
        });

        services.AddAzureClients(x => x.AddBlobServiceClient(storageConnectionString));
        services.AddScoped<IImageStorageService, AzureBlobImageStorageService>();
        services.AddSingleton<IImageStorageSecureService, ImageStorageSecureService>();
        services.AddSingleton<ISecureStringGenerator, SecureStringGenerator>();
        services.AddSingleton<Random>(_ => Random.Shared);

        services.AddScoped(sp =>
            sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User
            ?? new ClaimsPrincipal());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGuildRepository, GuildRepository>();
        services.AddScoped<IFriendRepository, FriendRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();

        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFriendService, FriendService>();
        services.AddScoped<IGuildAuthorizationService, GuildAuthorizationService>();
        services.AddScoped<IGuildService, GuildService>();
        services.AddScoped<IChatService, ChatService>();

        services.AddSignalR(options =>
        {
            // options.KeepAliveInterval = TimeSpan.FromSeconds(10);
            // options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        });
        services.AddScoped<IRealTimeNotifier, SignalRRealTimeNotifier>();
        services.AddScoped<ICacheLoader, CacheLoader>();
        services.AddSingleton<IUserPresence, InMemoryUserPresence>();
        services.AddSingleton<IOnlineCache, OnlineCache>();

        return services;
    }
}
