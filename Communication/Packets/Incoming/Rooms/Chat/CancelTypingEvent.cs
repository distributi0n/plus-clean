﻿namespace Plus.Communication.Packets.Incoming.Rooms.Chat
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Chat;

    public class CancelTypingEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);
            if (User == null)
            {
                return;
            }

            Session.GetHabbo().CurrentRoom.SendPacket(new UserTypingComposer(User.VirtualId, false));
        }
    }
}