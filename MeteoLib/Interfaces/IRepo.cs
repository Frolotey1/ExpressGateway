using MeteoLib.LoadService;

namespace MeteoLib.Interfaces
{
    public interface IRepo
    {
        Packet? GetPacket(string iata);
        long SavePacket(string iata, MeteoRecord? previous, MeteoRecord current, MeteoRecord? current_eng, Trigger trigger);
        void SaveTaf(string iata, TafMeteoRecord taf, TafMeteoRecord taf_eng);
        void TriggerComplete(long saveNumber);
        IEnumerable<string> Unprocessed();
    }
}
