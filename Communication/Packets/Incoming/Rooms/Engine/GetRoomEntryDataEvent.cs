namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    using HabboHotel.GameClients;
    using HabboHotel.Items.Wired;
    using HabboHotel.Rooms;
    using Outgoing.Rooms.Chat;
    using Outgoing.Rooms.Engine;

    internal class GetRoomEntryDataEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            if (Session.GetHabbo().InRoom)
            {
                Room OldRoom;
                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out OldRoom))
                {
                    return;
                }

                if (OldRoom.GetRoomUserManager() != null)
                {
                    OldRoom.GetRoomUserManager().RemoveUserFromRoom(Session, false, false);
                }
            }

            if (!Room.GetRoomUserManager().AddAvatarToRoom(Session))
            {
                Room.GetRoomUserManager().RemoveUserFromRoom(Session, false, false);
                return; //TODO: Remove?
            }

            Room.SendObjects(Session);

            //Status updating for messenger, do later as buggy.
            try
            {
                if (Session.GetHabbo().GetMessenger() != null)
                {
                    Session.GetHabbo().GetMessenger().OnStatusChanged(true);
                }
            }
            catch
            {
            }
            if (Session.GetHabbo().GetStats().QuestID > 0)
            {
                PlusEnvironment.GetGame().GetQuestManager().QuestReminder(Session, Session.GetHabbo().GetStats().QuestID);
            }
            Session.SendPacket(new RoomEntryInfoComposer(Room.RoomId, Room.CheckRights(Session, true)));
            Session.SendPacket(new RoomVisualizationSettingsComposer(Room.WallThickness,
                Room.FloorThickness,
                PlusEnvironment.EnumToBool(Room.Hidewall.ToString())));
            var ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);
            if (ThisUser != null && Session.GetHabbo().PetId == 0)
            {
                Room.SendPacket(new UserChangeComposer(ThisUser, false));
            }
            Session.SendPacket(new RoomEventComposer(Room.RoomData, Room.RoomData.Promotion));
            if (Room.GetWired() != null)
            {
                Room.GetWired().TriggerEvent(WiredBoxType.TriggerRoomEnter, Session.GetHabbo());
            }
            if (PlusEnvironment.GetUnixTimestamp() < Session.GetHabbo().FloodTime && Session.GetHabbo().FloodTime != 0)
            {
                Session.SendPacket(
                    new FloodControlComposer((int) Session.GetHabbo().FloodTime - (int) PlusEnvironment.GetUnixTimestamp()));
            }
        }
    }
}