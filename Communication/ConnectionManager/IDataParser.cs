namespace Plus.Communication.ConnectionManager
{
    using System;

    public interface IDataParser : IDisposable, ICloneable
    {
        void handlePacketData(byte[] packet);
    }
}