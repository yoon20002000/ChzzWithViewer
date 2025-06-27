using System;

namespace ChzzAPI
{
    public interface IChzzAPIEvents
    {
        void OnMessage(Profile profile, string message);
        void OnDonation(Profile profile, string message, DonationExtras donation);
        void OnSubscription(Profile profile, SubscriptionExtras subscription);
        void OnClose();
        void OnOpen();
    }
} 