namespace Plus.Communication.Packets.Incoming.Inventory.Purse
{
    using HabboHotel.GameClients;
    using Outgoing.Inventory.Purse;

    internal class GetCreditsInfoEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new CreditBalanceComposer(Session.GetHabbo().Credits));
            Session.SendPacket(new ActivityPointsComposer(Session.GetHabbo().Duckets, Session.GetHabbo().Diamonds,
                Session.GetHabbo().GOTWPoints));
        }
    }
}