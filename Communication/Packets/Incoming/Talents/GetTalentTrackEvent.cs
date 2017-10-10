namespace Plus.Communication.Packets.Incoming.Talents
{
    using HabboHotel.GameClients;
    using Outgoing.Talents;

    internal class GetTalentTrackEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Type = Packet.PopString();
            var Levels = PlusEnvironment.GetGame().GetTalentTrackManager().GetLevels();
            Session.SendPacket(new TalentTrackComposer(Levels, Type));
        }
    }
}