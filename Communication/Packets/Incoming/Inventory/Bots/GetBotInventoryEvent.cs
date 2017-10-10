namespace Plus.Communication.Packets.Incoming.Inventory.Bots
{
    using HabboHotel.GameClients;
    using Outgoing.Inventory.Bots;

    internal class GetBotInventoryEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session.GetHabbo().GetInventoryComponent() == null)
            {
                return;
            }

            var Bots = Session.GetHabbo().GetInventoryComponent().GetBots();
            Session.SendPacket(new BotInventoryComposer(Bots));
        }
    }
}