namespace Plus.Communication.Packets.Outgoing.Navigator
{
    using System.Collections.Generic;
    using HabboHotel.Rooms;

    internal class GuestRoomSearchResultComposer : ServerPacket
    {
        public GuestRoomSearchResultComposer(int Mode, string UserQuery, ICollection<RoomData> Rooms) : base(ServerPacketHeader
            .GuestRoomSearchResultMessageComposer)
        {
            WriteInteger(Mode);
            WriteString(UserQuery);
            WriteInteger(Rooms.Count);
            foreach (var data in Rooms)
            {
                RoomAppender.WriteRoom(this, data, data.Promotion);
            }

            WriteBoolean(false);
        }
    }
}