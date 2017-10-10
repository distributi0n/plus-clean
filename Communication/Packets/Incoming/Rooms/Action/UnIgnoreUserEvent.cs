namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Action;

    internal class UnIgnoreUserEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
            {
                return;
            }

            var Room = session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            var Username = packet.PopString();
            var Player = PlusEnvironment.GetHabboByUsername(Username);
            if (Player == null)
            {
                return;
            }
            if (!session.GetHabbo().GetIgnores().TryGet(Player.Id))
            {
                return;
            }

            if (session.GetHabbo().GetIgnores().TryRemove(Player.Id))
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `user_ignores` WHERE `user_id` = @uid AND `ignore_id` = @ignoreId");
                    dbClient.AddParameter("uid", session.GetHabbo().Id);
                    dbClient.AddParameter("ignoreId", Player.Id);
                    dbClient.RunQuery();
                }
                session.SendPacket(new IgnoreStatusComposer(3, Player.Username));
            }
        }
    }
}