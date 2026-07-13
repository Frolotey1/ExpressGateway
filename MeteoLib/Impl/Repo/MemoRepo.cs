using MeteoLib.Interfaces;
using MeteoLib.LoadService;

namespace MeteoLib.Impl.Repo;

public class MemoRepo : IRepo
{
    readonly Dictionary<string, Packet> Dict = new();

    long SN = 1;

    public Packet? FirstUnprocessed()
    {
        throw new NotImplementedException();
    }

    public Packet? GetPacket(string iata)
    {
        var key = iata;

        if (!Dict.ContainsKey(key)) return null;

        return Dict[key];
    }
    public long SavePacket(string asset, MeteoRecord? previous, MeteoRecord current, MeteoRecord? current_eng, Trigger? trigger)
    {


        var packet = new Packet(SN++, previous, current, trigger);



        if (Dict.ContainsKey(asset))
            Dict[asset] = packet;
        else
            Dict.Add(asset, packet);


        return packet.SN;
    }



    public void TriggerComplete(long sn)
    {
        var p = GetPacketBySN(sn);

        p.IsTriggerComplete = true;
    }

    private Packet GetPacketBySN(long sn)
    {
        return Dict.Values.First(p => p.SN == sn);
    }

    public IEnumerable<string> Unprocessed()
    {
        throw new NotImplementedException();
    }

    public void SaveTaf(string iata, TafMeteoRecord taf, TafMeteoRecord taf_eng)
    {
        throw new NotImplementedException();
    }
}