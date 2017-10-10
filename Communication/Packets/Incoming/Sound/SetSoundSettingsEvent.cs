namespace Plus.Communication.Packets.Incoming.Sound
{
    using HabboHotel.GameClients;

    internal class SetSoundSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Volume = "";
            for (var i = 0; i < 3; i++)
            {
                var Vol = Packet.PopInt();
                if (Vol < 0 || Vol > 100)
                {
                    Vol = 100;
                }
                if (i < 2)
                {
                    Volume += Vol + ",";
                }
                else
                {
                    Volume += Vol;
                }
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET volume = @volume WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("volume", Volume);
                dbClient.RunQuery();
            }
        }
    }
}