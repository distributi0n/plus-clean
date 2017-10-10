namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Quests;
    using HabboHotel.Rooms;

    internal class GiveHandItemEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().InRoom)
            {
                return;
            }

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

            var TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Packet.PopInt());
            if (TargetUser == null)
            {
                return;
            }

            if (!(Math.Abs(User.X - TargetUser.X) >= 3 || Math.Abs(User.Y - TargetUser.Y) >= 3) ||
                Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                if (User.CarryItemID > 0 && User.CarryTimer > 0)
                {
                    if (User.CarryItemID == 8)
                    {
                        PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.GIVE_COFFEE);
                    }
                    TargetUser.CarryItem(User.CarryItemID);
                    User.CarryItem(0);
                    TargetUser.DanceId = 0;
                }
            }
        }
    }
}