namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;

    internal class ApplySignEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var SignId = Packet.PopInt();
            Room Room;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            User.UnIdle();
            User.SetStatus("sign", Convert.ToString(SignId));
            User.UpdateNeeded = true;
            User.SignTime = PlusEnvironment.GetUnixTimestamp() + 5;
        }
    }
}