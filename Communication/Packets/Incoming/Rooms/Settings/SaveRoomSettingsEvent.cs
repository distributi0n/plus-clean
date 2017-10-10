namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using HabboHotel.GameClients;
    using HabboHotel.Navigator;
    using HabboHotel.Rooms;
    using Outgoing.Navigator;
    using Outgoing.Rooms.Engine;
    using Outgoing.Rooms.Settings;

    internal class SaveRoomSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }

            var Room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(Packet.PopInt());
            if (Room == null || !Room.CheckRights(Session, true))
            {
                return;
            }

            var Name = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            var Description = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            var Access = RoomAccessUtility.ToRoomAccess(Packet.PopInt());
            var Password = Packet.PopString();
            var MaxUsers = Packet.PopInt();
            var CategoryId = Packet.PopInt();
            var TagCount = Packet.PopInt();
            var Tags = new List<string>();
            var formattedTags = new StringBuilder();
            for (var i = 0; i < TagCount; i++)
            {
                if (i > 0)
                {
                    formattedTags.Append(",");
                }
                var tag = Packet.PopString().ToLower();
                Tags.Add(tag);
                formattedTags.Append(tag);
            }

            var TradeSettings = Packet.PopInt(); //2 = All can trade, 1 = owner only, 0 = no trading.
            var AllowPets = Convert.ToInt32(PlusEnvironment.BoolToEnum(Packet.PopBoolean()));
            var AllowPetsEat = Convert.ToInt32(PlusEnvironment.BoolToEnum(Packet.PopBoolean()));
            var RoomBlockingEnabled = Convert.ToInt32(PlusEnvironment.BoolToEnum(Packet.PopBoolean()));
            var Hidewall = Convert.ToInt32(PlusEnvironment.BoolToEnum(Packet.PopBoolean()));
            var WallThickness = Packet.PopInt();
            var FloorThickness = Packet.PopInt();
            var WhoMute = Packet.PopInt(); // mute
            var WhoKick = Packet.PopInt(); // kick
            var WhoBan = Packet.PopInt(); // ban
            var chatMode = Packet.PopInt();
            var chatSize = Packet.PopInt();
            var chatSpeed = Packet.PopInt();
            var chatDistance = Packet.PopInt();
            var extraFlood = Packet.PopInt();
            if (chatMode < 0 || chatMode > 1)
            {
                chatMode = 0;
            }
            if (chatSize < 0 || chatSize > 2)
            {
                chatSize = 0;
            }
            if (chatSpeed < 0 || chatSpeed > 2)
            {
                chatSpeed = 0;
            }
            if (chatDistance < 0)
            {
                chatDistance = 1;
            }
            if (chatDistance > 99)
            {
                chatDistance = 100;
            }
            if (extraFlood < 0 || extraFlood > 2)
            {
                extraFlood = 0;
            }
            if (TradeSettings < 0 || TradeSettings > 2)
            {
                TradeSettings = 0;
            }
            if (WhoMute < 0 || WhoMute > 1)
            {
                WhoMute = 0;
            }
            if (WhoKick < 0 || WhoKick > 1)
            {
                WhoKick = 0;
            }
            if (WhoBan < 0 || WhoBan > 1)
            {
                WhoBan = 0;
            }
            if (WallThickness < -2 || WallThickness > 1)
            {
                WallThickness = 0;
            }
            if (FloorThickness < -2 || FloorThickness > 1)
            {
                FloorThickness = 0;
            }
            if (Name.Length < 1)
            {
                return;
            }

            if (Name.Length > 60)
            {
                Name = Name.Substring(0, 60);
            }
            if (Access == RoomAccess.PASSWORD && Password.Length == 0)
            {
                Access = RoomAccess.OPEN;
            }
            if (MaxUsers < 0)
            {
                MaxUsers = 10;
            }
            if (MaxUsers > 50)
            {
                MaxUsers = 50;
            }
            SearchResultList SearchResultList = null;
            if (!PlusEnvironment.GetGame().GetNavigator().TryGetSearchResultList(CategoryId, out SearchResultList))
            {
                CategoryId = 36;
            }
            if (SearchResultList.CategoryType != NavigatorCategoryType.CATEGORY ||
                SearchResultList.RequiredRank > Session.GetHabbo().Rank ||
                Session.GetHabbo().Id != Room.OwnerId && Session.GetHabbo().Rank >= SearchResultList.RequiredRank)
            {
                CategoryId = 36;
            }
            if (TagCount > 2)
            {
                return;
            }

            Room.AllowPets = AllowPets;
            Room.AllowPetsEating = AllowPetsEat;
            Room.RoomBlockingEnabled = RoomBlockingEnabled;
            Room.Hidewall = Hidewall;
            Room.RoomData.AllowPets = AllowPets;
            Room.RoomData.AllowPetsEating = AllowPetsEat;
            Room.RoomData.RoomBlockingEnabled = RoomBlockingEnabled;
            Room.RoomData.Hidewall = Hidewall;
            Room.Name = Name;
            Room.Access = Access;
            Room.Description = Description;
            Room.Category = CategoryId;
            Room.Password = Password;
            Room.RoomData.Name = Name;
            Room.RoomData.Access = Access;
            Room.RoomData.Description = Description;
            Room.RoomData.Category = CategoryId;
            Room.RoomData.Password = Password;
            Room.WhoCanBan = WhoBan;
            Room.WhoCanKick = WhoKick;
            Room.WhoCanMute = WhoMute;
            Room.RoomData.WhoCanBan = WhoBan;
            Room.RoomData.WhoCanKick = WhoKick;
            Room.RoomData.WhoCanMute = WhoMute;
            Room.ClearTags();
            Room.AddTagRange(Tags);
            Room.UsersMax = MaxUsers;
            Room.RoomData.Tags.Clear();
            Room.RoomData.Tags.AddRange(Tags);
            Room.RoomData.UsersMax = MaxUsers;
            Room.WallThickness = WallThickness;
            Room.FloorThickness = FloorThickness;
            Room.RoomData.WallThickness = WallThickness;
            Room.RoomData.FloorThickness = FloorThickness;
            Room.chatMode = chatMode;
            Room.chatSize = chatSize;
            Room.chatSpeed = chatSpeed;
            Room.chatDistance = chatDistance;
            Room.extraFlood = extraFlood;
            Room.TradeSettings = TradeSettings;
            Room.RoomData.chatMode = chatMode;
            Room.RoomData.chatSize = chatSize;
            Room.RoomData.chatSpeed = chatSpeed;
            Room.RoomData.chatDistance = chatDistance;
            Room.RoomData.extraFlood = extraFlood;
            Room.RoomData.TradeSettings = TradeSettings;
            var AccessStr = Password.Length > 0 ? "password" : "open";
            switch (Access)
            {
                default:
                case RoomAccess.OPEN:
                    AccessStr = "open";
                    break;
                case RoomAccess.PASSWORD:
                    AccessStr = "password";
                    break;
                case RoomAccess.DOORBELL:
                    AccessStr = "locked";
                    break;
                case RoomAccess.INVISIBLE:
                    AccessStr = "invisible";
                    break;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "UPDATE `rooms` SET `caption` = @caption, `description` = @description, `password` = @password, `category` = @categoryId, `state` = @state, `tags` = @tags, `users_max` = @maxUsers, `allow_pets` = @allowPets, `allow_pets_eat` = @allowPetsEat, `room_blocking_disabled` = @roomBlockingDisabled, `allow_hidewall` = @allowHidewall, `floorthick` = @floorThick, `wallthick` = @wallThick, `mute_settings` = @muteSettings, `kick_settings` = @kickSettings, `ban_settings` = @banSettings, `chat_mode` = @chatMode, `chat_size` = @chatSize, `chat_speed` = @chatSpeed, `chat_extra_flood` = @extraFlood, `chat_hearing_distance` = @chatDistance, `trade_settings` = @tradeSettings WHERE `id` = @roomId LIMIT 1");
                dbClient.AddParameter("categoryId", CategoryId);
                dbClient.AddParameter("maxUsers", MaxUsers);
                dbClient.AddParameter("allowPets", AllowPets);
                dbClient.AddParameter("allowPetsEat", AllowPetsEat);
                dbClient.AddParameter("roomBlockingDisabled", RoomBlockingEnabled);
                dbClient.AddParameter("allowHidewall", Room.Hidewall);
                dbClient.AddParameter("floorThick", Room.FloorThickness);
                dbClient.AddParameter("wallThick", Room.WallThickness);
                dbClient.AddParameter("muteSettings", Room.WhoCanMute);
                dbClient.AddParameter("kickSettings", Room.WhoCanKick);
                dbClient.AddParameter("banSettings", Room.WhoCanBan);
                dbClient.AddParameter("chatMode", Room.chatMode);
                dbClient.AddParameter("chatSize", Room.chatSize);
                dbClient.AddParameter("chatSpeed", Room.chatSpeed);
                dbClient.AddParameter("extraFlood", Room.extraFlood);
                dbClient.AddParameter("chatDistance", Room.chatDistance);
                dbClient.AddParameter("tradeSettings", Room.TradeSettings);
                dbClient.AddParameter("roomId", Room.Id);
                dbClient.AddParameter("caption", Room.Name);
                dbClient.AddParameter("description", Room.Description);
                dbClient.AddParameter("password", Room.Password);
                dbClient.AddParameter("state", AccessStr);
                dbClient.AddParameter("tags", formattedTags.ToString());
                dbClient.RunQuery();
            }
            Room.GetGameMap().GenerateMaps();
            if (Session.GetHabbo().CurrentRoom == null)
            {
                Session.SendPacket(new RoomSettingsSavedComposer(Room.RoomId));
                Session.SendPacket(new RoomInfoUpdatedComposer(Room.RoomId));
                Session.SendPacket(new RoomVisualizationSettingsComposer(Room.WallThickness,
                    Room.FloorThickness,
                    PlusEnvironment.EnumToBool(Room.Hidewall.ToString())));
            }
            else
            {
                Room.SendPacket(new RoomSettingsSavedComposer(Room.RoomId));
                Room.SendPacket(new RoomInfoUpdatedComposer(Room.RoomId));
                Room.SendPacket(new RoomVisualizationSettingsComposer(Room.WallThickness,
                    Room.FloorThickness,
                    PlusEnvironment.EnumToBool(Room.Hidewall.ToString())));
            }
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModDoorModeSeen", 1);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModWalkthroughSeen", 1);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModChatScrollSpeedSeen", 1);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModChatFloodFilterSeen", 1);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModChatHearRangeSeen", 1);
        }
    }
}