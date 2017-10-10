namespace Plus.Communication.Packets.Incoming.Users
{
    using HabboHotel.GameClients;
    using Outgoing.Users;

    internal class GetSelectedBadgesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var UserId = Packet.PopInt();
            var Habbo = PlusEnvironment.GetHabboById(UserId);
            if (Habbo == null)
            {
                return;
            }

            Session.SendPacket(new HabboUserBadgesComposer(Habbo));
        }
    }
}