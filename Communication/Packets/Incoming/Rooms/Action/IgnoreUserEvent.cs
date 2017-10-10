namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using HabboHotel.GameClients;
    using Outgoing.Rooms.Action;

    internal class IgnoreUserEvent : IPacketEvent
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
            if (Player == null || Player.GetPermissions().HasRight("mod_tool"))
            {
                return;
            }
            if (session.GetHabbo().GetIgnores().TryGet(Player.Id))
            {
                return;
            }

            if (session.GetHabbo().GetIgnores().TryAdd(Player.Id))
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("INSERT INTO `user_ignores` (`user_id`,`ignore_id`) VALUES(@uid,@ignoreId);");
                    dbClient.AddParameter("uid", session.GetHabbo().Id);
                    dbClient.AddParameter("ignoreId", Player.Id);
                    dbClient.RunQuery();
                }
                session.SendPacket(new IgnoreStatusComposer(1, Player.Username));
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModIgnoreSeen", 1);
            }
        }
    }
}