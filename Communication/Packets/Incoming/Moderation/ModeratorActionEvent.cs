namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using Outgoing.Moderation;

    internal sealed class ModeratorActionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().GetPermissions().HasRight("mod_caution"))
            {
                return;
            }
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var CurrentRoom = Session.GetHabbo().CurrentRoom;
            if (CurrentRoom == null)
            {
                return;
            }

            var AlertMode = Packet.PopInt();
            var AlertMessage = Packet.PopString();
            var IsCaution = AlertMode != 3;
            AlertMessage = IsCaution
                ? "Caution from Moderator:\n\n" + AlertMessage
                : "Message from Moderator:\n\n" + AlertMessage;
            Session.GetHabbo().CurrentRoom.SendPacket(new BroadcastMessageAlertComposer(AlertMessage));
        }
    }
}