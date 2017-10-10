namespace Plus.Communication.Packets.Incoming.Users
{
    using System;
    using System.Linq;
    using HabboHotel.GameClients;
    using Outgoing.Users;

    internal class GetRelationshipsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Habbo = PlusEnvironment.GetHabboById(Packet.PopInt());
            if (Habbo == null)
            {
                return;
            }

            var rand = new Random();
            Habbo.Relationships =
                Habbo.Relationships.OrderBy(x => rand.Next()).ToDictionary(item => item.Key, item => item.Value);
            var Loves = Habbo.Relationships.Count(x => x.Value.Type == 1);
            var Likes = Habbo.Relationships.Count(x => x.Value.Type == 2);
            var Hates = Habbo.Relationships.Count(x => x.Value.Type == 3);
            Session.SendPacket(new GetRelationshipsComposer(Habbo, Loves, Likes, Hates));
        }
    }
}