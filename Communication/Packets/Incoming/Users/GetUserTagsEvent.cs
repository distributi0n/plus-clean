namespace Plus.Communication.Packets.Incoming.Users
{
    using HabboHotel.GameClients;
    using Outgoing.Users;

    internal class GetUserTagsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var UserId = Packet.PopInt();
            Session.SendPacket(new UserTagsComposer(UserId));
        }
    }
}