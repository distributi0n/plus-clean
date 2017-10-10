namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;

    internal sealed class CloseIssueDefaultActionEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
            {
            }
        }
    }
}