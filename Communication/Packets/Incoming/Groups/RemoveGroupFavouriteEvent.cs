namespace Plus.Communication.Packets.Incoming.Groups
{
    using HabboHotel.GameClients;
    using Outgoing.Groups;

    internal class RemoveGroupFavouriteEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.GetHabbo().GetStats().FavouriteGroupId = 0;
            if (Session.GetHabbo().InRoom)
            {
                var User = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (User != null)
                {
                    Session.GetHabbo().CurrentRoom.SendPacket(new UpdateFavouriteGroupComposer(null, User.VirtualId));
                }
                Session.GetHabbo().CurrentRoom.SendPacket(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
            }
            else
            {
                Session.SendPacket(new RefreshFavouriteGroupComposer(Session.GetHabbo().Id));
            }
        }
    }
}