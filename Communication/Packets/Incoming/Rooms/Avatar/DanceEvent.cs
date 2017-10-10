namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    using HabboHotel.GameClients;
    using HabboHotel.Quests;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Avatar;

    internal class DanceEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            Room Room;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            User.UnIdle();
            var DanceId = Packet.PopInt();
            if (DanceId < 0 || DanceId > 4)
            {
                DanceId = 0;
            }
            if (DanceId > 0 && User.CarryItemID > 0)
            {
                User.CarryItem(0);
            }
            if (Session.GetHabbo().Effects().CurrentEffect > 0)
            {
                Room.SendPacket(new AvatarEffectComposer(User.VirtualId, 0));
            }
            User.DanceId = DanceId;
            Room.SendPacket(new DanceComposer(User, DanceId));
            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_DANCE);
            if (Room.GetRoomUserManager().GetRoomUsers().Count > 19)
            {
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.MASS_DANCE);
            }
        }
    }
}