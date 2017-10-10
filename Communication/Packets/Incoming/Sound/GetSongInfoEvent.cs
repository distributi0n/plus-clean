﻿namespace Plus.Communication.Packets.Incoming.Sound
{
    using HabboHotel.GameClients;
    using Outgoing.Sound;

    internal class GetSongInfoEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new TraxSongInfoComposer());
        }
    }
}