﻿namespace Plus.Communication.Packets.Incoming.Rooms.Polls
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Polls;

    internal class PollStartEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new PollContentsComposer());
        }
    }
}