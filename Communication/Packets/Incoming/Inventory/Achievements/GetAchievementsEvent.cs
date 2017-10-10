namespace Plus.Communication.Packets.Incoming.Inventory.Achievements
{
    using System.Linq;
    using HabboHotel.GameClients;
    using Outgoing.Inventory.Achievements;

    internal sealed class GetAchievementsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new AchievementsComposer(Session,
                PlusEnvironment.GetGame().GetAchievementManager()._achievements.Values.ToList()));
        }
    }
}