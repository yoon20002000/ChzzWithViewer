using System;

namespace ChzzAPI
{
    public interface IChzzAPIEvents
    {
        void OnMessage(ChzzkUnity.Profile profile, string message);
        void OnDonation(ChzzkUnity.Profile profile, string message, ChzzkUnity.DonationExtras donation);
        void OnSubscription(ChzzkUnity.Profile profile, ChzzkUnity.SubscriptionExtras subscription);
        void OnClose();
        void OnOpen();
    }
} 