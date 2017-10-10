namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;

    internal class ModerationCautionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().GetPermissions().HasRight("mod_caution"))
            {
                return;
            }

            var UserId = Packet.PopInt();
            var Message = Packet.PopString();
            var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Client == null || Client.GetHabbo() == null)
            {
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `cautions` = `cautions` + '1' WHERE `user_id` = '" +
                                  Client.GetHabbo().Id + "' LIMIT 1");
            }
            Client.SendNotification(Message);
        }
    }
}