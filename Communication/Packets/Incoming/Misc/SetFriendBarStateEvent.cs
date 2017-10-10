namespace Plus.Communication.Packets.Incoming.Misc
{
    using HabboHotel.GameClients;
    using HabboHotel.Users.Messenger.FriendBar;
    using Outgoing.Sound;

    internal sealed class SetFriendBarStateEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }

            Session.GetHabbo().FriendbarState = FriendBarStateUtility.GetEnum(Packet.PopInt());
            Session.SendPacket(new SoundSettingsComposer(Session.GetHabbo().ClientVolume,
                Session.GetHabbo().ChatPreference,
                Session.GetHabbo().AllowMessengerInvites,
                Session.GetHabbo().FocusPreference,
                FriendBarStateUtility.GetInt(Session.GetHabbo().FriendbarState)));
        }
    }
}