namespace Plus.Communication.Packets.Incoming.GameCenter
{
    using System;
    using System.Text;
    using HabboHotel.GameClients;
    using HabboHotel.Games;
    using Outgoing.GameCenter;

    internal sealed class JoinPlayerQueueEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }

            var GameId = Packet.PopInt();
            GameData GameData = null;
            if (PlusEnvironment.GetGame().GetGameDataManager().TryGetGame(GameId, out GameData))
            {
                var SSOTicket = "HABBOON-Fastfood-" + GenerateSSO(32) + "-" + Session.GetHabbo().Id;
                Session.SendPacket(new JoinQueueComposer(GameData.GameId));
                Session.SendPacket(new LoadGameComposer(GameData, SSOTicket));
            }
        }

        // @distributi0n TODO: Utility this
        private string GenerateSSO(int length)
        {
            var random = new Random();
            var characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var result = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                result.Append(characters[random.Next(characters.Length)]);
            }

            return result.ToString();
        }
    }
}