namespace Plus.Communication.Packets.Incoming.Users
{
    using HabboHotel.GameClients;

    internal class SetMessengerInviteStatusEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Status = Packet.PopBoolean();
            Session.GetHabbo().AllowMessengerInvites = Status;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `ignore_invites` = @MessengerInvites WHERE `id` = '" +
                                  Session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("MessengerInvites", PlusEnvironment.BoolToEnum(Status));
                dbClient.RunQuery();
            }
        }
    }
}