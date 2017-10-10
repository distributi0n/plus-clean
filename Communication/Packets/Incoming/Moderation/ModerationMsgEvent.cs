namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;

    internal class ModerationMsgEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().GetPermissions().HasRight("mod_alert"))
            {
                return;
            }

            var UserId = Packet.PopInt();
            var Message = Packet.PopString();
            var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Client == null)
            {
                return;
            }

            Client.SendNotification(Message);
        }
    }
}