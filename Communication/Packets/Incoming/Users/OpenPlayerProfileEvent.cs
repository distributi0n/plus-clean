namespace Plus.Communication.Packets.Incoming.Users
{
    using HabboHotel.GameClients;
    using Outgoing.Users;

    internal class OpenPlayerProfileEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var userID = Packet.PopInt();
            var IsMe = Packet.PopBoolean();
            var targetData = PlusEnvironment.GetHabboById(userID);
            if (targetData == null)
            {
                Session.SendNotification("An error occured whilst finding that user's profile.");
                return;
            }

            var Groups = PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(targetData.Id);
            var friendCount = 0;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT COUNT(0) FROM `messenger_friendships` WHERE (`user_one_id` = @userid OR `user_two_id` = @userid)");
                dbClient.AddParameter("userid", userID);
                friendCount = dbClient.GetInteger();
            }
            Session.SendPacket(new ProfileInformationComposer(targetData, Session, Groups, friendCount));
        }
    }
}