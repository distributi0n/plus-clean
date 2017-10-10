namespace Plus.Communication.Packets.Incoming.Moderation
{
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;

    internal class ModerationKickEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().GetPermissions().HasRight("mod_kick"))
            {
                return;
            }

            var UserId = Packet.PopInt();
            var Message = Packet.PopString();
            var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Client == null || Client.GetHabbo() == null || Client.GetHabbo().CurrentRoomId < 1 ||
                Client.GetHabbo().Id == Session.GetHabbo().Id)
            {
                return;
            }

            if (Client.GetHabbo().Rank >= Session.GetHabbo().Rank)
            {
                Session.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("moderation.kick.disallowed"));
                return;
            }

            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }

            Room.GetRoomUserManager().RemoveUserFromRoom(Client, true, false);
        }
    }
}