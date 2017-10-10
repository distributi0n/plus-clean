namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    using HabboHotel.GameClients;
    using HabboHotel.Quests;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Avatar;

    public class ActionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }

            var Action = Packet.PopInt();
            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
            {
                return;
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            if (User.DanceId > 0)
            {
                User.DanceId = 0;
            }
            if (Session.GetHabbo().Effects().CurrentEffect > 0)
            {
                Room.SendPacket(new AvatarEffectComposer(User.VirtualId, 0));
            }
            User.UnIdle();
            Room.SendPacket(new ActionComposer(User.VirtualId, Action));
            if (Action == 5) // idle
            {
                User.IsAsleep = true;
                Room.SendPacket(new SleepComposer(User, true));
            }
            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_WAVE);
        }
    }
}