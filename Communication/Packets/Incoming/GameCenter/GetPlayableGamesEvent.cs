namespace Plus.Communication.Packets.Incoming.GameCenter
{
    using HabboHotel.GameClients;
    using Outgoing.GameCenter;

    internal class GetPlayableGamesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var GameId = Packet.PopInt();
            Session.SendPacket(new GameAccountStatusComposer(GameId));
            Session.SendPacket(new PlayableGamesComposer(GameId));
            Session.SendPacket(new GameAchievementListComposer(Session,
                PlusEnvironment.GetGame().GetAchievementManager().GetGameAchievements(GameId),
                GameId));
        }
    }
}