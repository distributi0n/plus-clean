﻿namespace Plus.Communication.Packets.Outgoing.Groups
{
    using HabboHotel.Groups;

    internal class ManageGroupComposer : ServerPacket
    {
        public ManageGroupComposer(Group Group, string[] BadgeParts) : base(ServerPacketHeader.ManageGroupMessageComposer)
        {
            WriteInteger(0);
            WriteBoolean(true);
            WriteInteger(Group.Id);
            WriteString(Group.Name);
            WriteString(Group.Description);
            WriteInteger(1);
            WriteInteger(Group.Colour1);
            WriteInteger(Group.Colour2);
            WriteInteger(Group.GroupType == GroupType.OPEN ? 0 : Group.GroupType == GroupType.LOCKED ? 1 : 2);
            WriteInteger(Group.AdminOnlyDeco);
            WriteBoolean(false);
            WriteString("");
            WriteInteger(5);
            for (var x = 0; x < BadgeParts.Length; x++)
            {
                var symbol = BadgeParts[x];
                WriteInteger(symbol.Length >= 6 ? int.Parse(symbol.Substring(0, 3)) : int.Parse(symbol.Substring(0, 2)));
                WriteInteger(symbol.Length >= 6 ? int.Parse(symbol.Substring(3, 2)) : int.Parse(symbol.Substring(2, 2)));
                WriteInteger(symbol.Length < 5
                    ? 0
                    : symbol.Length >= 6
                        ? int.Parse(symbol.Substring(5, 1))
                        : int.Parse(symbol.Substring(4, 1)));
            }

            var i = 0;
            while (i < 5 - BadgeParts.Length)
            {
                WriteInteger(0);
                WriteInteger(0);
                WriteInteger(0);
                i++;
            }

            WriteString(Group.Badge);
            WriteInteger(Group.MemberCount);
        }
    }
}