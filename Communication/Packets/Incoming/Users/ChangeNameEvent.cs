namespace Plus.Communication.Packets.Incoming.Users
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using HabboHotel.Rooms;
    using HabboHotel.Users;
    using Outgoing.Navigator;
    using Outgoing.Rooms.Engine;
    using Outgoing.Rooms.Session;
    using Outgoing.Users;

    internal class ChangeNameEvent : IPacketEvent
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

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);
            if (User == null)
            {
                return;
            }

            var NewName = Packet.PopString();
            var OldName = Session.GetHabbo().Username;
            if (NewName == OldName)
            {
                Session.GetHabbo().ChangeName(OldName);
                Session.SendPacket(new UpdateUsernameComposer(NewName));
                return;
            }

            if (!CanChangeName(Session.GetHabbo()))
            {
                Session.SendNotification("Oops, it appears you currently cannot change your username!");
                return;
            }

            var InUse = false;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `users` WHERE `username` = @name LIMIT 1");
                dbClient.AddParameter("name", NewName);
                InUse = dbClient.GetInteger() == 1;
            }
            var Letters = NewName.ToLower().ToCharArray();
            var AllowedCharacters = "abcdefghijklmnopqrstuvwxyz.,_-;:?!1234567890";
            foreach (var Chr in Letters)
            {
                if (!AllowedCharacters.Contains(Chr))
                {
                    return;
                }
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool") && NewName.ToLower().Contains("mod") ||
                NewName.ToLower().Contains("adm") ||
                NewName.ToLower().Contains("admin") ||
                NewName.ToLower().Contains("m0d") ||
                NewName.ToLower().Contains("mob") ||
                NewName.ToLower().Contains("m0b"))
            {
                return;
            }
            if (!NewName.ToLower().Contains("mod") && (Session.GetHabbo().Rank == 2 || Session.GetHabbo().Rank == 3))
            {
                return;
            }
            if (NewName.Length > 15)
            {
                return;
            }
            if (NewName.Length < 3)
            {
                return;
            }
            if (InUse)
            {
                return;
            }

            if (!PlusEnvironment.GetGame().GetClientManager().UpdateClientUsername(Session, OldName, NewName))
            {
                Session.SendNotification("Oops! An issue occoured whilst updating your username.");
                return;
            }

            Session.GetHabbo().ChangingName = false;
            Room.GetRoomUserManager().RemoveUserFromRoom(Session, true, false);
            Session.GetHabbo().ChangeName(NewName);
            Session.GetHabbo().GetMessenger().OnStatusChanged(true);
            Session.SendPacket(new UpdateUsernameComposer(NewName));
            Room.SendPacket(new UserNameChangeComposer(Room.Id, User.VirtualId, NewName));
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `logs_client_namechange` (`user_id`,`new_name`,`old_name`,`timestamp`) VALUES ('" +
                                  Session.GetHabbo().Id +
                                  "', @name, '" +
                                  OldName +
                                  "', '" +
                                  PlusEnvironment.GetUnixTimestamp() +
                                  "')");
                dbClient.AddParameter("name", NewName);
                dbClient.RunQuery();
            }
            ICollection<RoomData> Rooms = Session.GetHabbo().UsersRooms;
            foreach (var Data in Rooms)
            {
                if (Data == null)
                {
                    continue;
                }

                Data.OwnerName = NewName;
            }
            foreach (var UserRoom in PlusEnvironment.GetGame().GetRoomManager().GetRooms().ToList())
            {
                if (UserRoom == null || UserRoom.RoomData.OwnerName != NewName)
                {
                    continue;
                }

                UserRoom.OwnerName = NewName;
                UserRoom.RoomData.OwnerName = NewName;
                UserRoom.SendPacket(new RoomInfoUpdatedComposer(UserRoom.RoomId));
            }

            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_Name", 1);
            Session.SendPacket(new RoomForwardComposer(Room.Id));
        }

        private static bool CanChangeName(Habbo Habbo)
        {
            if (Habbo.Rank == 1 && Habbo.VIPRank == 0 && Habbo.LastNameChange == 0)
            {
                return true;
            }
            if (Habbo.Rank == 1 &&
                Habbo.VIPRank == 1 &&
                (Habbo.LastNameChange == 0 || PlusEnvironment.GetUnixTimestamp() + 604800 > Habbo.LastNameChange))
            {
                return true;
            }
            if (Habbo.Rank == 1 &&
                Habbo.VIPRank == 2 &&
                (Habbo.LastNameChange == 0 || PlusEnvironment.GetUnixTimestamp() + 86400 > Habbo.LastNameChange))
            {
                return true;
            }
            if (Habbo.Rank == 1 && Habbo.VIPRank == 3)
            {
                return true;
            }
            if (Habbo.GetPermissions().HasRight("mod_tool"))
            {
                return true;
            }

            return false;
        }
    }
}