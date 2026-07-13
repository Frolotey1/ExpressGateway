using Meteohost.Core.Impl.Alert;
using Meteohost.Core.Impl.Messenger;
using Meteohost.Services;
using MeteoLib;
using MeteoLib.Impl.Delivery;
using MeteoLib.Impl.Repo;
using MeteoLib.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;

namespace xUnitTests.Utils
{
    internal static class Stub
    {
        public static Delivery CreateMemoDelivery(out MemoMessenger messenger) => CreateMemoDelivery(out messenger, out _, out AlertMemo tracker);

        public static Delivery CreateMemoDelivery(out MemoMessenger messenger, out MemoRepo repo, out AlertMemo tracker)
        {
            repo = new MemoRepo();
            messenger = new MemoMessenger();
            tracker = new AlertMemo();

            return new Delivery(messenger, messenger, tracker);
        }

        internal static DeliveryService CreateDeliveryService(out MemoMessenger messenger, out MemoRepo repo, out IIssueTracker tracker, out AlertMemo alert)
        {
            var d = CreateMemoDelivery(out messenger, out repo, out alert);
            tracker = NullMessenger.Instance;

            return new DeliveryService(NullLogger<DeliveryService>.Instance, repo, d, tracker);
        }
        internal static MessageBuilder MessageBuilder(string asset, Trigger trigger)
        {
            var mb = new MessageBuilder(asset, trigger, new TelegramFormatter());

            return mb;
        }
    }
}
