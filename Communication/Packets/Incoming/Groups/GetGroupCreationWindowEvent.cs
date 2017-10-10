namespace Plus.Communication.Packets.Incoming.Groups
{
    using System.Collections.Generic;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using Outgoing.Groups;

    internal class GetGroupCreationWindowEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null)
            {
                return;
            }

            var ValidRooms = new List<RoomData>();
            foreach (var Data in Session.GetHabbo().UsersRooms)
            {
                if (Data.Group == null)
                {
                    ValidRooms.Add(Data);
                }
            }

            Session.SendPacket(new GroupCreationWindowComposer(ValidRooms));
        }
    }
}