namespace Plus.Communication.Packets.Outgoing.Groups
{
    using System;
    using System.Collections.Generic;
    using HabboHotel.Rooms;

    internal class GroupCreationWindowComposer : ServerPacket
    {
        public GroupCreationWindowComposer(ICollection<RoomData> Rooms) : base(ServerPacketHeader
            .GroupCreationWindowMessageComposer)
        {
            WriteInteger(Convert.ToInt32(PlusEnvironment.GetSettingsManager()
                .TryGetValue("catalog.group.purchase.cost"))); //Price
            WriteInteger(Rooms.Count); //Room count that the user has.
            foreach (var Room in Rooms)
            {
                WriteInteger(Room.Id); //Room Id
                WriteString(Room.Name); //Room Name
                WriteBoolean(false); //What?
            }

            WriteInteger(5);
            WriteInteger(5);
            WriteInteger(11);
            WriteInteger(4);
            WriteInteger(6);
            WriteInteger(11);
            WriteInteger(4);
            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);
            WriteInteger(0);
        }
    }
}