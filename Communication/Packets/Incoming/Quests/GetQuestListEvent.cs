namespace Plus.Communication.Packets.Incoming.Quests
{
    using HabboHotel.GameClients;

    public class GetQuestListEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            PlusEnvironment.GetGame().GetQuestManager().GetList(Session, null);
        }
    }
}