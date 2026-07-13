using MeteoLib;

namespace MeteoLib.LoadService
{
    public class Packet
    {


        public Packet(long sn, MeteoRecord? previous, MeteoRecord current, Trigger? trigger)
        {
            CurrentMeteo = current;
            PreviousMeteo = previous;
            Trigger = trigger;
            SN = sn;
        }

        //public string IataCode { get;set; }
        public long SN { get; set; }
        public MeteoRecord CurrentMeteo { get; set; }
        public MeteoRecord? PreviousMeteo { get; set; }
        public Trigger? Trigger { get; set; }
        public bool IsTriggerComplete { get; set; }

    }
}