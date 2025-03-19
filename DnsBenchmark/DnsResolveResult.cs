namespace DnsBenchmark;

public readonly record struct DnsResolveResult(double? UdpResolveTime, double? DoHResolveTime, double? DoTResolveTime)
{
    public double? GetResolveTime(ResultValue resultValue)
    {
        switch (resultValue)
        {
            case ResultValue.Udp:
                return UdpResolveTime;
            case ResultValue.DoH:
                return DoHResolveTime;
            case ResultValue.DoT:
                return DoTResolveTime;
            default:
                throw new InvalidEnumArgumentException(nameof(resultValue), (int)resultValue, typeof(ResultValue));
        }
    }
};