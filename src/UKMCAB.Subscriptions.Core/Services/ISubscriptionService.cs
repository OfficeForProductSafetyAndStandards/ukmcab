using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Domain.Exceptions;
using static UKMCAB.Subscriptions.Core.Services.SubscriptionService;

namespace UKMCAB.Subscriptions.Core.Services;

public interface ISubscriptionService
{

    /// <summary>
    /// Confirms a requested search subscription.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    Task<ConfirmSearchSubscriptionResult> ConfirmSearchSubscriptionAsync(string payload);

    /// <summary>
    /// Confirms a requested cab subscription.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    Task<ConfirmCabSubscriptionResult> ConfirmCabSubscriptionAsync(string payload);

    /// <summary>
    /// Requests a subscription to a search.  The user will be emailed with a confirmation link which they need to click on to activate/confirm the subscription.
    /// </summary>
    /// <param name="request">The search subscription request</param>
    Task<RequestSubscriptionResult> RequestSubscriptionAsync(SearchSubscriptionRequest request);

    /// <summary>
    /// Requests a subscription to a CAB.  The user will be emailed with a confirmation link which they need to click on to activate/confirm the subscription.
    /// </summary>
    /// <param name="request">The subscription request</param>
    Task<RequestSubscriptionResult> RequestSubscriptionAsync(CabSubscriptionRequest request);
    
    /// <summary>
    /// Block email address (stops all future email being sent to this email address)
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <returns></returns>
    Task<bool> BlockEmailAsync(EmailAddress emailAddress);
    
    /// <summary>
    /// Unblocks an email address by removing it from the block list
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <returns></returns>
    Task<bool> UnblockEmailAsync(EmailAddress emailAddress);

    /// <summary>
    /// Unsubscribes/deletes a subscription
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <returns></returns>
    Task<bool> UnsubscribeAsync(string subscriptionId);

    /// <summary>
    /// Unsubscribes an email address from all subscriptions
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <returns></returns>
    Task<int> UnsubscribeAllAsync(EmailAddress emailAddress);

    /// <summary>
    /// Requests to update an email address on a subscription
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="SubscriptionsCoreDomainException">Raised if the email address is on a blocked list, or the email is the same as the one on the current subscription or if there's another subscription for the same topic on that email address</exception>
    /// <returns></returns>
    Task<string> RequestUpdateEmailAddressAsync(UpdateEmailAddressOptions options);

    /// <summary>
    /// Confirms an updated email address
    /// </summary>
    /// <param name="payload"></param>
    /// <returns>The new subscription id</returns>
    Task<string> ConfirmUpdateEmailAddressAsync(string payload);

    /// <summary>
    /// Returns whether the user is already subscribed to a given search
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="searchQueryString"></param>
    /// <returns></returns>
    Task<bool> IsSubscribedToSearchAsync(EmailAddress emailAddress, string? searchQueryString);

    /// <summary>
    /// Returns whether the user is already subscribed to a given cab
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="cabId"></param>
    /// <returns></returns>
    Task<bool> IsSubscribedToCabAsync(EmailAddress emailAddress, Guid cabId);

    /// <summary>
    /// Updates the frequency of a subscription
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <param name="frequency"></param>
    /// <returns></returns>
    Task UpdateFrequencyAsync(string subscriptionId, Frequency frequency);

    /// <summary>
    /// Retrieves a particular subscription
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <returns></returns>
    Task<SubscriptionModel?> GetSubscriptionAsync(string subscriptionId);

    /// <summary>
    /// Lists all subscriptions that belong to an email address
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="continuationToken"></param>
    /// <param name="take"></param>
    /// <returns></returns>
    Task<ListSubscriptionsResult> ListSubscriptionsAsync(EmailAddress emailAddress);
    Task<SearchResultsChangesModel?> GetSearchResultsChangesAsync(string id);
}
