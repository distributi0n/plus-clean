namespace Plus.Communication.Packets.Incoming.Users
{
    using HabboHotel.GameClients;

    internal class SetUserFocusPreferenceEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var FocusPreference = Packet.PopBoolean();
            Session.GetHabbo().FocusPreference = FocusPreference;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `focus_preference` = @focusPreference WHERE `id` = '" +
                                  Session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("focusPreference", PlusEnvironment.BoolToEnum(FocusPreference));
                dbClient.RunQuery();
            }
        }
    }
}