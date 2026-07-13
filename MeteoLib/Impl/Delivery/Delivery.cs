using MeteoLib.Interfaces;

namespace MeteoLib.Impl.Delivery;

public class Delivery
{

    private readonly IFlttMessenger _fltt;
    private readonly IMessenger _messenger;
    private readonly IAlert _alert;


    private static MessageBuilder FlttBuilder(string asset, Trigger trigger) => new(asset, trigger, new FlttFormatter());
    private static MessageBuilder TelegramBuilder(string asset, Trigger trigger) => new(asset, trigger, new TelegramFormatter());
    private static string ErrorMessageBuilder(Exception e) => "Meteohost Error\nType: " + e.Message + "\nError:" + e;

    public Delivery(IFlttMessenger fltt, IMessenger messenger, IAlert alert)
    {
        _fltt = fltt;
        _messenger = messenger;
        _alert = alert;
    }

    public void Send(string asset, Trigger trigger)
    {
        DeliveryFltt(asset, trigger);
        DeliveryMessenger(asset, trigger);
    }

    private void DeliveryMessenger(string asset, Trigger trigger)
    {
        try
        {
            var message = TelegramBuilder(asset, trigger).BuildMessage();

            ValidateMessageNotEmpty(message);

            _messenger.Send(asset, message);
        }
        catch(Exception e)
        {
            var errorMessage = ErrorMessageBuilder(e);
            _alert.SendAsync(errorMessage);
        }
    }


    private static void ValidateMessageNotEmpty(string message)
    {
        if (!message.ToLower().Contains("внимание"))
        {
            throw new MeteoException("empty trigger built");
        }
    }

    private void DeliveryFltt(string asset, Trigger trigger)
    {
        try
        {
            var message = FlttBuilder(asset, trigger).BuildMessage();

            ValidateMessageNotEmpty(message);

            _fltt.Send(asset, message);
        }
        catch (Exception e)
        {
            var errorMessage = ErrorMessageBuilder(e);
            _alert.SendAsync(errorMessage);
        }
    }
}
