﻿using Microsoft.EntityFrameworkCore.Storage;
using System.Text.Json;
using System.Text.Json.Serialization;
using UKMCAB.Subscriptions.Core;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;
using UKMCAB.Subscriptions.Core.Services;
using static UKMCAB.Web.UI.Services.Subscriptions.SubscriptionsDateTimeProvider;

namespace UKMCAB.Web.UI.Services.Subscriptions;

public static class SubscriptionsDiagnostics
{
    public static IApplicationBuilder UseSubscriptionsDiagnostics(this WebApplication builder)
    {
        const string b = "~/__api/subscriptions";

        builder.MapPost($"{b}/process-subs", async (context) =>
        {
            var result = await context.RequestServices.GetRequiredService<ISubscriptionEngineCoordinator>().RequestProcessAsync(CancellationToken.None);
            
            await context.Response.WriteAsJsonAsync(new 
            {
                result.Stats,
                result.Status,
                Exception = result.Exception?.ToString(),
            }, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        });

        builder.MapPost($"{b}/clear-all-data", async (context) =>
        {
            var eng = (IClearable)context.RequestServices.GetRequiredService<ISubscriptionEngine>();
            var subs = (IClearable)context.RequestServices.GetRequiredService<ISubscriptionService>();
            await eng.ClearDataAsync();
            await subs.ClearDataAsync();
            await context.Response.WriteAsJsonAsync(new { message = "All subscriptions data has been cleared" });
        });

        builder.MapGet($"{b}/datetime", async (ctx) =>
        {
            var subscriptionsDateTimeProvider = ctx.RequestServices.GetRequiredService<ISubscriptionsDateTimeProvider>();
            await ctx.Response.WriteAsJsonAsync(subscriptionsDateTimeProvider.Get());
        });

        builder.MapPost($"{b}/datetime/clear", async (ctx) =>
        {
            var subscriptionsDateTimeProvider = ctx.RequestServices.GetRequiredService<ISubscriptionsDateTimeProvider>();
            subscriptionsDateTimeProvider.Clear();
            await ctx.Response.WriteAsJsonAsync(new { message = "ok" });
        });

        builder.MapPost($"{b}/datetime", async (ctx) =>
        {
            var payload = await ctx.Request.ReadFromJsonAsync<SetPayload>() ?? throw new Exception("Deserialised to null");
            var subscriptionsDateTimeProvider = ctx.RequestServices.GetRequiredService<ISubscriptionsDateTimeProvider>();
            subscriptionsDateTimeProvider.Set(payload);
            await ctx.Response.WriteAsJsonAsync(new { message = $"Ok. Set override {payload.DateTime}. This override expires in {payload.ExpiryHours} hours." });
        });

        builder.MapGet($"{b}/sent-emails", async (ctx) =>
        {
            var sender = ctx.RequestServices.GetRequiredService<IOutboundEmailSender>();
            var opt = new JsonSerializerOptions { WriteIndented = true };
            opt.Converters.Add(new EmailAddressConverter());
            await ctx.Response.WriteAsJsonAsync(new { mode = sender.Mode == OutboundEmailSenderMode.Send ?"sending":"pretending", requests = sender.Requests }, opt);
        });


        builder.MapGet($"{b}/echohost", async (ctx) =>
        {
            await ctx.Response.WriteAsJsonAsync(new { host = ctx.Request.Host });
        });

        builder.MapPost($"{b}/service/off", async (ctx) =>
        {
            ctx.RequestServices.GetRequiredService<SubscriptionsBackgroundService>().IsEnabled = false;
            await ctx.Response.WriteAsJsonAsync(new { backgroundServiceState = "disabled" });
        });

        builder.MapPost($"{b}/service/on", async (ctx) =>
        {
            ctx.RequestServices.GetRequiredService<SubscriptionsBackgroundService>().IsEnabled = true;
            await ctx.Response.WriteAsJsonAsync(new { backgroundServiceState = "enabled" });
        });

        return builder;
    }

}
