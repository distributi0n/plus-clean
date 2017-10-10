﻿namespace Plus.Communication.Packets.Outgoing.Messenger
{
    using System.Collections.Generic;
    using HabboHotel.Users.Messenger;

    internal class BuddyRequestsComposer : ServerPacket
    {
        public BuddyRequestsComposer(ICollection<MessengerRequest> requests) : base(ServerPacketHeader
            .BuddyRequestsMessageComposer)
        {
            WriteInteger(requests.Count);
            WriteInteger(requests.Count);
            foreach (var Request in requests)
            {
                WriteInteger(Request.From);
                WriteString(Request.Username);
                var User = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Request.From);
                WriteString(User != null ? User.Look : "");
            }
        }
    }
}