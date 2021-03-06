﻿namespace Plus.HabboHotel.Rooms
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using AI;
    using Communication.Packets.Outgoing.Handshake;
    using Communication.Packets.Outgoing.Rooms.Avatar;
    using Communication.Packets.Outgoing.Rooms.Engine;
    using Communication.Packets.Outgoing.Rooms.Permissions;
    using Communication.Packets.Outgoing.Rooms.Session;
    using Core;
    using GameClients;
    using Games.Teams;
    using Items;
    using PathFinding;
    using Trading;
    using Utilities;

    public class RoomUserManager
    {
        private ConcurrentDictionary<int, RoomUser> _bots;
        private ConcurrentDictionary<int, RoomUser> _pets;
        private Room _room;
        private ConcurrentDictionary<int, RoomUser> _users;

        private int primaryPrivateUserID;
        private int secondaryPrivateUserID;

        public int userCount;

        public RoomUserManager(Room room)
        {
            _room = room;
            _users = new ConcurrentDictionary<int, RoomUser>();
            _pets = new ConcurrentDictionary<int, RoomUser>();
            _bots = new ConcurrentDictionary<int, RoomUser>();
            primaryPrivateUserID = 0;
            secondaryPrivateUserID = 0;
            PetCount = 0;
            userCount = 0;
        }

        public int PetCount { get; private set; }

        public RoomUser DeployBot(RoomBot Bot, Pet PetData)
        {
            var BotUser = new RoomUser(0, _room.RoomId, primaryPrivateUserID++, _room);
            Bot.VirtualId = primaryPrivateUserID;
            var PersonalID = secondaryPrivateUserID++;
            BotUser.InternalRoomID = PersonalID;
            _users.TryAdd(PersonalID, BotUser);
            var Model = _room.GetGameMap().Model;
            if (Bot.X > 0 && Bot.Y > 0 && Bot.X < Model.MapSizeX && Bot.Y < Model.MapSizeY)
            {
                BotUser.SetPos(Bot.X, Bot.Y, Bot.Z);
                BotUser.SetRot(Bot.Rot, false);
            }
            else
            {
                Bot.X = Model.DoorX;
                Bot.Y = Model.DoorY;
                BotUser.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                BotUser.SetRot(Model.DoorOrientation, false);
            }
            BotUser.BotData = Bot;
            BotUser.BotAI = Bot.GenerateBotAi(BotUser.VirtualId);
            if (BotUser.IsPet)
            {
                BotUser.BotAI.Init(Bot.BotId, BotUser.VirtualId, _room.RoomId, BotUser, _room);
                BotUser.PetData = PetData;
                BotUser.PetData.VirtualId = BotUser.VirtualId;
            }
            else
            {
                BotUser.BotAI.Init(Bot.BotId, BotUser.VirtualId, _room.RoomId, BotUser, _room);
            }

            //UpdateUserStatus(BotUser, false);
            BotUser.UpdateNeeded = true;
            _room.SendPacket(new UsersComposer(BotUser));
            if (BotUser.IsPet)
            {
                if (_pets.ContainsKey(BotUser.PetData.PetId)) //Pet allready placed
                {
                    _pets[BotUser.PetData.PetId] = BotUser;
                }
                else
                {
                    _pets.TryAdd(BotUser.PetData.PetId, BotUser);
                }
                PetCount++;
            }
            else if (BotUser.IsBot)
            {
                if (_bots.ContainsKey(BotUser.BotData.BotId))
                {
                    _bots[BotUser.BotData.BotId] = BotUser;
                }
                else
                {
                    _bots.TryAdd(BotUser.BotData.Id, BotUser);
                }
                _room.SendPacket(new DanceComposer(BotUser, BotUser.BotData.DanceId));
            }
            return BotUser;
        }

        public void RemoveBot(int VirtualId, bool Kicked)
        {
            var User = GetRoomUserByVirtualId(VirtualId);
            if (User == null || !User.IsBot)
            {
                return;
            }

            if (User.IsPet)
            {
                RoomUser PetRemoval = null;
                _pets.TryRemove(User.PetData.PetId, out PetRemoval);
                PetCount--;
            }
            else
            {
                RoomUser BotRemoval = null;
                _bots.TryRemove(User.BotData.Id, out BotRemoval);
            }
            User.BotAI.OnSelfLeaveRoom(Kicked);
            _room.SendPacket(new UserRemoveComposer(User.VirtualId));
            RoomUser toRemove;
            if (_users != null)
            {
                _users.TryRemove(User.InternalRoomID, out toRemove);
            }
            onRemove(User);
        }

        public RoomUser GetUserForSquare(int x, int y) => _room.GetGameMap().GetRoomUsers(new Point(x, y)).FirstOrDefault();

        public bool AddAvatarToRoom(GameClient Session)
        {
            if (_room == null)
            {
                return false;
            }
            if (Session == null)
            {
                return false;
            }
            if (Session.GetHabbo().CurrentRoom == null)
            {
                return false;
            }

            var User = new RoomUser(Session.GetHabbo().Id, _room.RoomId, primaryPrivateUserID++, _room);
            if (User == null || User.GetClient() == null)
            {
                return false;
            }

            User.UserId = Session.GetHabbo().Id;
            Session.GetHabbo().TentId = 0;
            var PersonalID = secondaryPrivateUserID++;
            User.InternalRoomID = PersonalID;
            Session.GetHabbo().CurrentRoomId = _room.RoomId;
            if (!_users.TryAdd(PersonalID, User))
            {
                return false;
            }

            var Model = _room.GetGameMap().Model;
            if (Model == null)
            {
                return false;
            }

            if (!_room.PetMorphsAllowed && Session.GetHabbo().PetId != 0)
            {
                Session.GetHabbo().PetId = 0;
            }
            if (!Session.GetHabbo().IsTeleporting && !Session.GetHabbo().IsHopping)
            {
                if (!Model.DoorIsValid())
                {
                    var Square = _room.GetGameMap().GetRandomWalkableSquare();
                    Model.DoorX = Square.X;
                    Model.DoorY = Square.Y;
                    Model.DoorZ = _room.GetGameMap().GetHeightForSquareFromData(Square);
                }
                User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                User.SetRot(Model.DoorOrientation, false);
            }
            else if (!User.IsBot && (User.GetClient().GetHabbo().IsTeleporting || User.GetClient().GetHabbo().IsHopping))
            {
                Item Item = null;
                if (Session.GetHabbo().IsTeleporting)
                {
                    Item = _room.GetRoomItemHandler().GetItem(Session.GetHabbo().TeleporterId);
                }
                else if (Session.GetHabbo().IsHopping)
                {
                    Item = _room.GetRoomItemHandler().GetItem(Session.GetHabbo().HopperId);
                }
                if (Item != null)
                {
                    if (Session.GetHabbo().IsTeleporting)
                    {
                        Item.ExtraData = "2";
                        Item.UpdateState(false, true);
                        User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                        User.SetRot(Item.Rotation, false);
                        Item.InteractingUser2 = Session.GetHabbo().Id;
                        Item.ExtraData = "0";
                        Item.UpdateState(false, true);
                    }
                    else if (Session.GetHabbo().IsHopping)
                    {
                        Item.ExtraData = "1";
                        Item.UpdateState(false, true);
                        User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                        User.SetRot(Item.Rotation, false);
                        User.AllowOverride = false;
                        Item.InteractingUser2 = Session.GetHabbo().Id;
                        Item.ExtraData = "2";
                        Item.UpdateState(false, true);
                    }
                }
                else
                {
                    User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ - 1);
                    User.SetRot(Model.DoorOrientation, false);
                }
            }
            _room.SendPacket(new UsersComposer(User));

            //Below = done
            if (_room.CheckRights(Session, true))
            {
                User.SetStatus("flatctrl", "useradmin");
                Session.SendPacket(new YouAreOwnerComposer());
                Session.SendPacket(new YouAreControllerComposer(4));
            }
            else if (_room.CheckRights(Session, false) && _room.Group == null)
            {
                User.SetStatus("flatctrl", "1");
                Session.SendPacket(new YouAreControllerComposer(1));
            }
            else if (_room.Group != null && _room.CheckRights(Session, false, true))
            {
                User.SetStatus("flatctrl", "3");
                Session.SendPacket(new YouAreControllerComposer(3));
            }
            else
            {
                Session.SendPacket(new YouAreNotControllerComposer());
            }
            User.UpdateNeeded = true;
            if (Session.GetHabbo().GetPermissions().HasRight("mod_tool") && !Session.GetHabbo().DisableForcedEffects)
            {
                Session.GetHabbo().Effects().ApplyEffect(102);
            }
            foreach (var Bot in _bots.Values.ToList())
            {
                if (Bot == null || Bot.BotAI == null)
                {
                    continue;
                }

                Bot.BotAI.OnUserEnterRoom(User);
            }

            return true;
        }

        public void RemoveUserFromRoom(GameClient Session, bool NotifyClient, bool NotifyKick = false)
        {
            try
            {
                if (_room == null)
                {
                    return;
                }
                if (Session == null || Session.GetHabbo() == null)
                {
                    return;
                }

                if (NotifyKick)
                {
                    Session.SendPacket(new GenericErrorComposer(4008));
                }
                if (NotifyClient)
                {
                    Session.SendPacket(new CloseConnectionComposer());
                }
                if (Session.GetHabbo().TentId > 0)
                {
                    Session.GetHabbo().TentId = 0;
                }
                var User = GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (User != null)
                {
                    if (User.RidingHorse)
                    {
                        User.RidingHorse = false;
                        var UserRiding = GetRoomUserByVirtualId(User.HorseID);
                        if (UserRiding != null)
                        {
                            UserRiding.RidingHorse = false;
                            UserRiding.HorseID = 0;
                        }
                    }
                    if (User.Team != TEAM.NONE)
                    {
                        var Team = _room.GetTeamManagerForFreeze();
                        if (Team != null)
                        {
                            Team.OnUserLeave(User);
                            User.Team = TEAM.NONE;
                            if (User.GetClient().GetHabbo().Effects().CurrentEffect != 0)
                            {
                                User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                            }
                        }
                    }
                    RemoveRoomUser(User);
                    if (User.CurrentItemEffect != ItemEffectType.NONE)
                    {
                        if (Session.GetHabbo().Effects() != null)
                        {
                            Session.GetHabbo().Effects().CurrentEffect = -1;
                        }
                    }
                    if (User.IsTrading)
                    {
                        Trade Trade = null;
                        if (_room.GetTrading().TryGetTrade(User.TradeId, out Trade))
                        {
                            Trade.EndTrade(User.TradeId);
                        }
                    }

                    //Session.GetHabbo().CurrentRoomId = 0;
                    if (Session.GetHabbo().GetMessenger() != null)
                    {
                        Session.GetHabbo().GetMessenger().OnStatusChanged(true);
                    }
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE user_roomvisits SET exit_timestamp = '" +
                                          PlusEnvironment.GetUnixTimestamp() +
                                          "' WHERE room_id = '" +
                                          _room.RoomId +
                                          "' AND user_id = '" +
                                          Session.GetHabbo().Id +
                                          "' ORDER BY exit_timestamp DESC LIMIT 1");
                        dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '" + _room.UsersNow + "' WHERE `id` = '" +
                                          _room.RoomId + "' LIMIT 1");
                    }
                    if (User != null)
                    {
                        User.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        private void onRemove(RoomUser user)
        {
            try
            {
                var session = user.GetClient();
                if (session == null)
                {
                    return;
                }

                var Bots = new List<RoomUser>();
                try
                {
                    foreach (var roomUser in GetUserList().ToList())
                    {
                        if (roomUser == null)
                        {
                            continue;
                        }

                        if (roomUser.IsBot && !roomUser.IsPet)
                        {
                            if (!Bots.Contains(roomUser))
                            {
                                Bots.Add(roomUser);
                            }
                        }
                    }
                }
                catch
                {
                }

                var PetsToRemove = new List<RoomUser>();
                foreach (var Bot in Bots.ToList())
                {
                    if (Bot == null || Bot.BotAI == null)
                    {
                        continue;
                    }

                    Bot.BotAI.OnUserLeaveRoom(session);
                    if (Bot.IsPet && Bot.PetData.OwnerId == user.UserId && !_room.CheckRights(session, true))
                    {
                        if (!PetsToRemove.Contains(Bot))
                        {
                            PetsToRemove.Add(Bot);
                        }
                    }
                }
                foreach (var toRemove in PetsToRemove.ToList())
                {
                    if (toRemove == null)
                    {
                        continue;
                    }
                    if (user.GetClient() == null || user.GetClient().GetHabbo() == null ||
                        user.GetClient().GetHabbo().GetInventoryComponent() == null)
                    {
                        continue;
                    }

                    user.GetClient().GetHabbo().GetInventoryComponent().TryAddPet(toRemove.PetData);
                    RemoveBot(toRemove.VirtualId, false);
                }

                _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));
            }
            catch (Exception e)
            {
                ExceptionLogger.LogCriticalException(e);
            }
        }

        private void RemoveRoomUser(RoomUser user)
        {
            if (user.SetStep)
            {
                _room.GetGameMap().GameMap[user.SetX, user.SetY] = user.SqState;
            }
            else
            {
                _room.GetGameMap().GameMap[user.X, user.Y] = user.SqState;
            }
            _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));
            _room.SendPacket(new UserRemoveComposer(user.VirtualId));
            RoomUser toRemove = null;
            if (_users.TryRemove(user.InternalRoomID, out toRemove))
            {
                // todo: uhmm, could put the below stuff in but idk.
            }
            user.InternalRoomID = -1;
            onRemove(user);
        }

        public bool TryGetPet(int PetId, out RoomUser Pet) => _pets.TryGetValue(PetId, out Pet);

        public bool TryGetBot(int BotId, out RoomUser Bot) => _bots.TryGetValue(BotId, out Bot);

        public RoomUser GetBotByName(string Name)
        {
            var FoundBot = _bots.Count(x => x.Value.BotData != null && x.Value.BotData.Name.ToLower() == Name.ToLower()) > 0;
            if (FoundBot)
            {
                var Id = _bots.FirstOrDefault(x => x.Value.BotData != null && x.Value.BotData.Name.ToLower() == Name.ToLower())
                    .Value.BotData.Id;
                return _bots[Id];
            }

            return null;
        }

        public void UpdateUserCount(int count)
        {
            userCount = count;
            _room.RoomData.UsersNow = count;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '" + count + "' WHERE `id` = '" + _room.RoomId + "' LIMIT 1");
            }
        }

        public RoomUser GetRoomUserByVirtualId(int VirtualId)
        {
            RoomUser User = null;
            if (!_users.TryGetValue(VirtualId, out User))
            {
                return null;
            }

            return User;
        }

        internal RoomUser GetRoomUserByHabbo(int id)
        {
            return GetUserList().FirstOrDefault(x => x?.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().Id == id);
        }

        internal List<RoomUser> GetRoomUsers()
        {
            var List = new List<RoomUser>();
            List = GetUserList().Where(x => !x.IsBot).ToList();
            return List;
        }

        internal List<RoomUser> GetRoomUserByRank(int minRank)
        {
            return GetUserList().ToList().Where(user => user != null).Where(user => !user.IsBot && user.GetClient() != null && user.GetClient().GetHabbo() != null && user.GetClient().GetHabbo().Rank >= minRank).ToList();
        }

        internal RoomUser GetRoomUserByHabbo(string pName)
        {
            return GetUserList().FirstOrDefault(x => x?.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().Username.Equals(pName, StringComparison.OrdinalIgnoreCase));
        }

        internal void UpdatePets()
        {
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                foreach (var Pet in GetPets().ToList())
                {
                    if (Pet == null)
                    {
                        continue;
                    }

                    if (Pet.DbState == DatabaseUpdateState.NeedsInsert)
                    {
                        dbClient.SetQuery("INSERT INTO `bots` (`id`,`user_id`,`room_id`,`name`,`x`,`y`,`z`) VALUES ('" +
                                          Pet.PetId +
                                          "','" +
                                          Pet.OwnerId +
                                          "','" +
                                          Pet.RoomId +
                                          "',@name,'0','0','0')");
                        dbClient.AddParameter("name", Pet.Name);
                        dbClient.RunQuery();
                        dbClient.SetQuery(
                            "INSERT INTO `bots_petdata` (`type`,`race`,`color`,`experience`,`energy`,`createstamp`,`nutrition`,`respect`) VALUES ('" +
                            Pet.Type +
                            "',@race,@color,'0','100','" +
                            Pet.CreationStamp +
                            "','0','0')");
                        dbClient.AddParameter(Pet.PetId + "race", Pet.Race);
                        dbClient.AddParameter(Pet.PetId + "color", Pet.Color);
                        dbClient.RunQuery();
                    }
                    else if (Pet.DbState == DatabaseUpdateState.NeedsUpdate)
                    {
                        // Surely this can be *99 better? TODO
                        
                        var user = GetRoomUserByVirtualId(Pet.VirtualId);
                        dbClient.RunQuery("UPDATE `bots` SET room_id = " + Pet.RoomId + ", x = " + (user?.X ?? 0) + ", Y = " + (user?.Y ?? 0) + ", Z = " + (user?.Z ?? 0) + " WHERE `id` = '" + Pet.PetId + "' LIMIT 1");
                        dbClient.RunQuery("UPDATE `bots_petdata` SET `experience` = '" +
                                          Pet.Experience +
                                          "', `energy` = '" +
                                          Pet.Energy +
                                          "', `nutrition` = '" +
                                          Pet.Nutrition +
                                          "', `respect` = '" +
                                          Pet.Respect +
                                          "' WHERE `id` = '" +
                                          Pet.PetId +
                                          "' LIMIT 1");
                    }
                    Pet.DbState = DatabaseUpdateState.Updated;
                }
            }
        }

        private void UpdateBots()
        {
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                foreach (var User in GetRoomUsers().ToList())
                {
                    if (User == null || !User.IsBot)
                    {
                        continue;
                    }

                    if (User.IsBot)
                    {
                        dbClient.SetQuery(
                            "UPDATE bots SET x=@x, y=@y, z=@z, name=@name, look=@look, rotation=@rotation WHERE id=@id LIMIT 1;");
                        dbClient.AddParameter("name", User.BotData.Name);
                        dbClient.AddParameter("look", User.BotData.Look);
                        dbClient.AddParameter("rotation", User.BotData.Rot);
                        dbClient.AddParameter("x", User.X);
                        dbClient.AddParameter("y", User.Y);
                        dbClient.AddParameter("z", User.Z);
                        dbClient.AddParameter("id", User.BotData.BotId);
                        dbClient.RunQuery();
                    }
                }
            }
        }

        public List<Pet> GetPets()
        {
            var Pets = new List<Pet>();
            foreach (var User in _pets.Values.ToList())
            {
                if (User == null || !User.IsPet)
                {
                    continue;
                }

                Pets.Add(User.PetData);
            }

            return Pets;
        }

        public void SerializeStatusUpdates()
        {
            var Users = new List<RoomUser>();
            var RoomUsers = GetUserList();
            if (RoomUsers == null)
            {
                return;
            }

            foreach (var User in RoomUsers.ToList())
            {
                if (User == null || !User.UpdateNeeded || Users.Contains(User))
                {
                    continue;
                }

                User.UpdateNeeded = false;
                Users.Add(User);
            }

            if (Users.Count > 0)
            {
                _room.SendPacket(new UserUpdateComposer(Users));
            }
        }

        internal void UpdateUserStatusses()
        {
            foreach (var user in GetUserList().ToList())
            {
                if (user == null)
                {
                    continue;
                }

                UpdateUserStatus(user, false);
            }
        }

        private bool IsValid(RoomUser user)
        {
            if (user == null)
            {
                return false;
            }
            if (user.IsBot)
            {
                return true;
            }
            if (user.GetClient() == null)
            {
                return false;
            }
            if (user.GetClient().GetHabbo() == null)
            {
                return false;
            }
            if (user.GetClient().GetHabbo().CurrentRoomId != _room.RoomId)
            {
                return false;
            }

            return true;
        }

        public void OnCycle()
        {
            var userCounter = 0;
            try
            {
                var ToRemove = new List<RoomUser>();
                foreach (var User in GetUserList().ToList())
                {
                    if (User == null)
                    {
                        continue;
                    }

                    if (!IsValid(User))
                    {
                        if (User.GetClient() != null)
                        {
                            RemoveUserFromRoom(User.GetClient(), false);
                        }
                        else
                        {
                            RemoveRoomUser(User);
                        }
                    }
                    if (User.NeedsAutokick && !ToRemove.Contains(User))
                    {
                        ToRemove.Add(User);
                        continue;
                    }

                    var updated = false;
                    User.IdleTime++;
                    User.HandleSpamTicks();
                    if (!User.IsBot && !User.IsAsleep && User.IdleTime >= 600)
                    {
                        User.IsAsleep = true;
                        _room.SendPacket(new SleepComposer(User, true));
                    }
                    if (User.CarryItemID > 0)
                    {
                        User.CarryTimer--;
                        if (User.CarryTimer <= 0)
                        {
                            User.CarryItem(0);
                        }
                    }
                    if (_room.GotFreeze())
                    {
                        _room.GetFreeze().CycleUser(User);
                    }
                    var InvalidStep = false;
                    if (User.isRolling)
                    {
                        if (User.rollerDelay <= 0)
                        {
                            UpdateUserStatus(User, false);
                            User.isRolling = false;
                        }
                        else
                        {
                            User.rollerDelay--;
                        }
                    }
                    if (User.SetStep)
                    {
                        if (_room.GetGameMap()
                            .IsValidStep2(User,
                                new Vector2D(User.X, User.Y),
                                new Vector2D(User.SetX, User.SetY),
                                User.GoalX == User.SetX && User.GoalY == User.SetY,
                                User.AllowOverride))
                        {
                            if (!User.RidingHorse)
                            {
                                _room.GetGameMap()
                                    .UpdateUserMovement(new Point(User.Coordinate.X, User.Coordinate.Y),
                                        new Point(User.SetX, User.SetY), User);
                            }
                            var items = _room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y));
                            foreach (var Item in items.ToList())
                            {
                                Item.UserWalksOffFurni(User);
                            }

                            if (!User.IsBot)
                            {
                                User.X = User.SetX;
                                User.Y = User.SetY;
                                User.Z = User.SetZ;
                            }
                            else if (User.IsBot && !User.RidingHorse)
                            {
                                User.X = User.SetX;
                                User.Y = User.SetY;
                                User.Z = User.SetZ;
                            }
                            if (!User.IsBot && User.RidingHorse)
                            {
                                var Horse = GetRoomUserByVirtualId(User.HorseID);
                                if (Horse != null)
                                {
                                    Horse.X = User.SetX;
                                    Horse.Y = User.SetY;
                                }
                            }
                            if (User.X == _room.GetGameMap().Model.DoorX &&
                                User.Y == _room.GetGameMap().Model.DoorY &&
                                !ToRemove.Contains(User) &&
                                !User.IsBot)
                            {
                                ToRemove.Add(User);
                                continue;
                            }

                            var Items = _room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y));
                            foreach (var Item in Items.ToList())
                            {
                                Item.UserWalksOnFurni(User);
                            }

                            UpdateUserStatus(User, true);
                        }
                        else
                        {
                            InvalidStep = true;
                        }

                        User.SetStep = false;
                    }

                    if (User.PathRecalcNeeded)
                    {
                        if (User.Path.Count > 1)
                        {
                            User.Path.Clear();
                        }
                        User.Path = PathFinder.FindPath(User,
                            _room.GetGameMap().DiagonalEnabled,
                            _room.GetGameMap(),
                            new Vector2D(User.X, User.Y),
                            new Vector2D(User.GoalX, User.GoalY));
                        if (User.Path.Count > 1)
                        {
                            User.PathStep = 1;
                            User.IsWalking = true;
                            User.PathRecalcNeeded = false;
                        }
                        else
                        {
                            User.PathRecalcNeeded = false;
                            if (User.Path.Count > 1)
                            {
                                User.Path.Clear();
                            }
                        }
                    }
                    if (User.IsWalking && !User.Freezed)
                    {
                        if (InvalidStep || User.PathStep >= User.Path.Count || User.GoalX == User.X && User.GoalY == User.Y
                        ) //No path found, or reached goal (:
                        {
                            User.IsWalking = false;
                            User.RemoveStatus("mv");
                            if (User.Statusses.ContainsKey("sign"))
                            {
                                User.RemoveStatus("sign");
                            }
                            if (User.IsBot && User.BotData.TargetUser > 0)
                            {
                                if (User.CarryItemID > 0)
                                {
                                    var Target = _room.GetRoomUserManager().GetRoomUserByHabbo(User.BotData.TargetUser);
                                    if (Target != null && Gamemap.TilesTouching(User.X, User.Y, Target.X, Target.Y))
                                    {
                                        User.SetRot(Rotation.Calculate(User.X, User.Y, Target.X, Target.Y), false);
                                        Target.SetRot(Rotation.Calculate(Target.X, Target.Y, User.X, User.Y), false);
                                        Target.CarryItem(User.CarryItemID);
                                    }
                                }
                                User.CarryItem(0);
                                User.BotData.TargetUser = 0;
                            }
                            if (User.RidingHorse && User.IsPet == false && !User.IsBot)
                            {
                                var mascotaVinculada = GetRoomUserByVirtualId(User.HorseID);
                                if (mascotaVinculada != null)
                                {
                                    mascotaVinculada.IsWalking = false;
                                    mascotaVinculada.RemoveStatus("mv");
                                    mascotaVinculada.UpdateNeeded = true;
                                }
                            }
                        }
                        else
                        {
                            var NextStep = User.Path[User.Path.Count - User.PathStep - 1];
                            User.PathStep++;
                            if (User.FastWalking && User.PathStep < User.Path.Count)
                            {
                                var s2 = User.Path.Count - User.PathStep - 1;
                                NextStep = User.Path[s2];
                                User.PathStep++;
                            }
                            if (User.SuperFastWalking && User.PathStep < User.Path.Count)
                            {
                                var s2 = User.Path.Count - User.PathStep - 1;
                                NextStep = User.Path[s2];
                                User.PathStep++;
                                User.PathStep++;
                            }
                            var nextX = NextStep.X;
                            var nextY = NextStep.Y;
                            User.RemoveStatus("mv");
                            if (_room.GetGameMap()
                                .IsValidStep2(User,
                                    new Vector2D(User.X, User.Y),
                                    new Vector2D(nextX, nextY),
                                    User.GoalX == nextX && User.GoalY == nextY,
                                    User.AllowOverride))
                            {
                                var nextZ = _room.GetGameMap().SqAbsoluteHeight(nextX, nextY);
                                if (!User.IsBot)
                                {
                                    if (User.isSitting)
                                    {
                                        User.Statusses.Remove("sit");
                                        User.Z += 0.35;
                                        User.isSitting = false;
                                        User.UpdateNeeded = true;
                                    }
                                    else if (User.isLying)
                                    {
                                        User.Statusses.Remove("sit");
                                        User.Z += 0.35;
                                        User.isLying = false;
                                        User.UpdateNeeded = true;
                                    }
                                }
                                if (!User.IsBot)
                                {
                                    User.Statusses.Remove("lay");
                                    User.Statusses.Remove("sit");
                                }
                                if (!User.IsBot && !User.IsPet && User.GetClient() != null)
                                {
                                    if (User.GetClient().GetHabbo().IsTeleporting)
                                    {
                                        User.GetClient().GetHabbo().IsTeleporting = false;
                                        User.GetClient().GetHabbo().TeleporterId = 0;
                                    }
                                    else if (User.GetClient().GetHabbo().IsHopping)
                                    {
                                        User.GetClient().GetHabbo().IsHopping = false;
                                        User.GetClient().GetHabbo().HopperId = 0;
                                    }
                                }
                                if (!User.IsBot && User.RidingHorse && User.IsPet == false)
                                {
                                    var Horse = GetRoomUserByVirtualId(User.HorseID);
                                    if (Horse != null)
                                    {
                                        Horse.SetStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));
                                    }
                                    User.SetStatus("mv", +nextX + "," + nextY + "," + TextHandling.GetString(nextZ + 1));
                                    User.UpdateNeeded = true;
                                    Horse.UpdateNeeded = true;
                                }
                                else
                                {
                                    User.SetStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));
                                }
                                var newRot = Rotation.Calculate(User.X, User.Y, nextX, nextY, User.moonwalkEnabled);
                                User.RotBody = newRot;
                                User.RotHead = newRot;
                                User.SetStep = true;
                                User.SetX = nextX;
                                User.SetY = nextY;
                                User.SetZ = nextZ;
                                UpdateUserEffect(User, User.SetX, User.SetY);
                                updated = true;
                                if (User.RidingHorse && User.IsPet == false && !User.IsBot)
                                {
                                    var Horse = GetRoomUserByVirtualId(User.HorseID);
                                    if (Horse != null)
                                    {
                                        Horse.RotBody = newRot;
                                        Horse.RotHead = newRot;
                                        Horse.SetStep = true;
                                        Horse.SetX = nextX;
                                        Horse.SetY = nextY;
                                        Horse.SetZ = nextZ;
                                    }
                                }
                                _room.GetGameMap().GameMap[User.X, User.Y] = User.SqState; // REstore the old one
                                User.SqState = _room.GetGameMap().GameMap[User.SetX, User.SetY]; //Backup the new one
                                if (_room.RoomBlockingEnabled == 0)
                                {
                                    var Users = _room.GetRoomUserManager().GetUserForSquare(nextX, nextY);
                                    if (Users != null)
                                    {
                                        _room.GetGameMap().GameMap[nextX, nextY] = 0;
                                    }
                                }
                                else
                                {
                                    _room.GetGameMap().GameMap[nextX, nextY] = 1;
                                }
                            }
                        }
                        if (!User.RidingHorse)
                        {
                            User.UpdateNeeded = true;
                        }
                    }
                    else
                    {
                        if (User.Statusses.ContainsKey("mv"))
                        {
                            User.RemoveStatus("mv");
                            User.UpdateNeeded = true;
                            if (User.RidingHorse)
                            {
                                var Horse = GetRoomUserByVirtualId(User.HorseID);
                                if (Horse != null)
                                {
                                    Horse.RemoveStatus("mv");
                                    Horse.UpdateNeeded = true;
                                }
                            }
                        }
                    }
                    if (User.RidingHorse)
                    {
                        User.ApplyEffect(77);
                    }
                    if (User.IsBot && User.BotAI != null)
                    {
                        User.BotAI.OnTimerTick();
                    }
                    else
                    {
                        userCounter++;
                    }
                    if (!updated)
                    {
                        UpdateUserEffect(User, User.X, User.Y);
                    }
                }
                foreach (var toRemove in ToRemove.ToList())
                {
                    var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(toRemove.HabboId);
                    if (client != null)
                    {
                        RemoveUserFromRoom(client, true);
                    }
                    else
                    {
                        RemoveRoomUser(toRemove);
                    }
                }

                if (userCount != userCounter)
                {
                    UpdateUserCount(userCounter);
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogCriticalException(e);
            }
        }

        public void UpdateUserStatus(RoomUser User, bool cyclegameitems)
        {
            if (User == null)
            {
                return;
            }

            try
            {
                var isBot = User.IsBot;
                if (isBot)
                {
                    cyclegameitems = false;
                }
                if (PlusEnvironment.GetUnixTimestamp() > PlusEnvironment.GetUnixTimestamp() + User.SignTime)
                {
                    if (User.Statusses.ContainsKey("sign"))
                    {
                        User.Statusses.Remove("sign");
                        User.UpdateNeeded = true;
                    }
                }
                if (User.Statusses.ContainsKey("lay") && !User.isLying || User.Statusses.ContainsKey("sit") && !User.isSitting)
                {
                    if (User.Statusses.ContainsKey("lay"))
                    {
                        User.Statusses.Remove("lay");
                    }
                    if (User.Statusses.ContainsKey("sit"))
                    {
                        User.Statusses.Remove("sit");
                    }
                    User.UpdateNeeded = true;
                }
                else if (User.isLying || User.isSitting)
                {
                    return;
                }

                double newZ;
                var ItemsOnSquare = _room.GetGameMap().GetAllRoomItemForSquare(User.X, User.Y);
                if (ItemsOnSquare != null || ItemsOnSquare.Count != 0)
                {
                    if (User.RidingHorse && User.IsPet == false)
                    {
                        newZ = _room.GetGameMap().SqAbsoluteHeight(User.X, User.Y, ItemsOnSquare.ToList()) + 1;
                    }
                    else
                    {
                        newZ = _room.GetGameMap().SqAbsoluteHeight(User.X, User.Y, ItemsOnSquare.ToList());
                    }
                }
                else
                {
                    newZ = 1;
                }
                if (newZ != User.Z)
                {
                    User.Z = newZ;
                    User.UpdateNeeded = true;
                }
                var Model = _room.GetGameMap().Model;
                if (Model.SqState[User.X, User.Y] == SquareState.SEAT)
                {
                    if (!User.Statusses.ContainsKey("sit"))
                    {
                        User.Statusses.Add("sit", "1.0");
                    }
                    User.Z = Model.SqFloorHeight[User.X, User.Y];
                    User.RotHead = Model.SqSeatRot[User.X, User.Y];
                    User.RotBody = Model.SqSeatRot[User.X, User.Y];
                    User.UpdateNeeded = true;
                }
                if (ItemsOnSquare.Count == 0)
                {
                    User.LastItem = null;
                }
                foreach (var Item in ItemsOnSquare.ToList())
                {
                    if (Item == null)
                    {
                        continue;
                    }

                    if (Item.GetBaseItem().IsSeat)
                    {
                        if (!User.Statusses.ContainsKey("sit"))
                        {
                            if (!User.Statusses.ContainsKey("sit"))
                            {
                                User.Statusses.Add("sit", TextHandling.GetString(Item.GetBaseItem().Height));
                            }
                        }
                        User.Z = Item.GetZ;
                        User.RotHead = Item.Rotation;
                        User.RotBody = Item.Rotation;
                        User.UpdateNeeded = true;
                    }
                    switch (Item.GetBaseItem().InteractionType)
                    {
                        case InteractionType.Bed:
                        case InteractionType.TentSmall:
                        {
                            if (!User.Statusses.ContainsKey("lay"))
                            {
                                User.Statusses.Add("lay", TextHandling.GetString(Item.GetBaseItem().Height) + " null");
                            }
                            User.Z = Item.GetZ;
                            User.RotHead = Item.Rotation;
                            User.RotBody = Item.Rotation;
                            User.UpdateNeeded = true;
                            break;
                        }
                        case InteractionType.Banzaigategreen:
                        case InteractionType.Banzaigateblue:
                        case InteractionType.Banzaigatered:
                        case InteractionType.Banzaigateyellow:
                        {
                            if (cyclegameitems)
                            {
                                var effectID = Convert.ToInt32(Item.Team + 32);
                                var t = User.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForBanzai();
                                if (User.Team == TEAM.NONE)
                                {
                                    if (t.CanEnterOnTeam(Item.Team))
                                    {
                                        if (User.Team != TEAM.NONE)
                                        {
                                            t.OnUserLeave(User);
                                        }
                                        User.Team = Item.Team;
                                        t.AddUser(User);
                                        if (User.GetClient().GetHabbo().Effects().CurrentEffect != effectID)
                                        {
                                            User.GetClient().GetHabbo().Effects().ApplyEffect(effectID);
                                        }
                                    }
                                }
                                else if (User.Team != TEAM.NONE && User.Team != Item.Team)
                                {
                                    t.OnUserLeave(User);
                                    User.Team = TEAM.NONE;
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                }
                                else
                                {
                                    //usersOnTeam--;
                                    t.OnUserLeave(User);
                                    if (User.GetClient().GetHabbo().Effects().CurrentEffect == effectID)
                                    {
                                        User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                    }
                                    User.Team = TEAM.NONE;
                                }

                                //Item.ExtraData = usersOnTeam.ToString();
                                //Item.UpdateState(false, true);                                
                            }
                            break;
                        }
                        case InteractionType.FreezeYellowGate:
                        case InteractionType.FreezeRedGate:
                        case InteractionType.FreezeGreenGate:
                        case InteractionType.FreezeBlueGate:
                        {
                            if (cyclegameitems)
                            {
                                var effectID = Convert.ToInt32(Item.Team + 39);
                                var t = User.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForFreeze();
                                if (User.Team == TEAM.NONE)
                                {
                                    if (t.CanEnterOnTeam(Item.Team))
                                    {
                                        if (User.Team != TEAM.NONE)
                                        {
                                            t.OnUserLeave(User);
                                        }
                                        User.Team = Item.Team;
                                        t.AddUser(User);
                                        if (User.GetClient().GetHabbo().Effects().CurrentEffect != effectID)
                                        {
                                            User.GetClient().GetHabbo().Effects().ApplyEffect(effectID);
                                        }
                                    }
                                }
                                else if (User.Team != TEAM.NONE && User.Team != Item.Team)
                                {
                                    t.OnUserLeave(User);
                                    User.Team = TEAM.NONE;
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                }
                                else
                                {
                                    //usersOnTeam--;
                                    t.OnUserLeave(User);
                                    if (User.GetClient().GetHabbo().Effects().CurrentEffect == effectID)
                                    {
                                        User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                    }
                                    User.Team = TEAM.NONE;
                                }

                                //Item.ExtraData = usersOnTeam.ToString();
                                //Item.UpdateState(false, true);                                
                            }
                            break;
                        }
                        case InteractionType.Banzaitele:
                        {
                            if (User.Statusses.ContainsKey("mv"))
                            {
                                _room.GetGameItemHandler().onTeleportRoomUserEnter(User, Item);
                            }
                            break;
                        }
                        case InteractionType.Effect:
                        {
                            if (User == null)
                            {
                                return;
                            }

                            if (!User.IsBot)
                            {
                                if (Item == null ||
                                    Item.GetBaseItem() == null ||
                                    User.GetClient() == null ||
                                    User.GetClient().GetHabbo() == null ||
                                    User.GetClient().GetHabbo().Effects() == null)
                                {
                                    return;
                                }
                                if (Item.GetBaseItem().EffectId == 0 && User.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                                {
                                    return;
                                }

                                User.GetClient().GetHabbo().Effects().ApplyEffect(Item.GetBaseItem().EffectId);
                                Item.ExtraData = "1";
                                Item.UpdateState(false, true);
                                Item.RequestUpdate(2, true);
                            }

                            break;
                        }
                        case InteractionType.Arrow:
                        {
                            if (User.GoalX == Item.GetX && User.GoalY == Item.GetY)
                            {
                                if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                                {
                                    continue;
                                }

                                Room Room;
                                if (!PlusEnvironment.GetGame().GetRoomManager()
                                    .TryGetRoom(User.GetClient().GetHabbo().CurrentRoomId, out Room))
                                {
                                    return;
                                }

                                if (!ItemTeleporterFinder.IsTeleLinked(Item.Id, Room))
                                {
                                    User.UnlockWalking();
                                }
                                else
                                {
                                    var LinkedTele = ItemTeleporterFinder.GetLinkedTele(Item.Id);
                                    var TeleRoomId = ItemTeleporterFinder.GetTeleRoomId(LinkedTele, Room);
                                    if (TeleRoomId == Room.RoomId)
                                    {
                                        var TargetItem = Room.GetRoomItemHandler().GetItem(LinkedTele);
                                        if (TargetItem == null)
                                        {
                                            if (User.GetClient() != null)
                                            {
                                                User.GetClient().SendWhisper("Hey, that arrow is poorly!");
                                            }
                                            return;
                                        }

                                        Room.GetGameMap().TeleportToItem(User, TargetItem);
                                    }
                                    else if (TeleRoomId != Room.RoomId)
                                    {
                                        if (User != null && !User.IsBot && User.GetClient() != null &&
                                            User.GetClient().GetHabbo() != null)
                                        {
                                            User.GetClient().GetHabbo().IsTeleporting = true;
                                            User.GetClient().GetHabbo().TeleportingRoomId = TeleRoomId;
                                            User.GetClient().GetHabbo().TeleporterId = LinkedTele;
                                            User.GetClient().GetHabbo().PrepareRoom(TeleRoomId, "");
                                        }
                                    }
                                    else if (_room.GetRoomItemHandler().GetItem(LinkedTele) != null)
                                    {
                                        User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                                        User.SetRot(Item.Rotation, false);
                                    }
                                    else
                                    {
                                        User.UnlockWalking();
                                    }
                                }
                            }

                            break;
                        }
                    }
                }

                if (User.isSitting && User.TeleportEnabled)
                {
                    User.Z -= 0.35;
                    User.UpdateNeeded = true;
                }
                if (cyclegameitems)
                {
                    if (_room.GotSoccer())
                    {
                        _room.GetSoccer().OnUserWalk(User);
                    }
                    if (_room.GotBanzai())
                    {
                        _room.GetBanzai().OnUserWalk(User);
                    }
                    if (_room.GotFreeze())
                    {
                        _room.GetFreeze().OnUserWalk(User);
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        private void UpdateUserEffect(RoomUser User, int x, int y)
        {
            if (User == null || User.IsBot || User.GetClient() == null || User.GetClient().GetHabbo() == null)
            {
                return;
            }

            try
            {
                var NewCurrentUserItemEffect = _room.GetGameMap().EffectMap[x, y];
                if (NewCurrentUserItemEffect > 0)
                {
                    if (User.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                    {
                        User.CurrentItemEffect = ItemEffectType.NONE;
                    }
                    var Type = ByteToItemEffectEnum.Parse(NewCurrentUserItemEffect);
                    if (Type != User.CurrentItemEffect)
                    {
                        switch (Type)
                        {
                            case ItemEffectType.Iceskates:
                            {
                                User.GetClient().GetHabbo().Effects()
                                    .ApplyEffect(User.GetClient().GetHabbo().Gender == "M" ? 38 : 39);
                                User.CurrentItemEffect = ItemEffectType.Iceskates;
                                break;
                            }
                            case ItemEffectType.Normalskates:
                            {
                                User.GetClient().GetHabbo().Effects()
                                    .ApplyEffect(User.GetClient().GetHabbo().Gender == "M" ? 55 : 56);
                                User.CurrentItemEffect = Type;
                                break;
                            }
                            case ItemEffectType.SWIM:
                            {
                                User.GetClient().GetHabbo().Effects().ApplyEffect(29);
                                User.CurrentItemEffect = Type;
                                break;
                            }
                            case ItemEffectType.SwimLow:
                            {
                                User.GetClient().GetHabbo().Effects().ApplyEffect(30);
                                User.CurrentItemEffect = Type;
                                break;
                            }
                            case ItemEffectType.SwimHalloween:
                            {
                                User.GetClient().GetHabbo().Effects().ApplyEffect(37);
                                User.CurrentItemEffect = Type;
                                break;
                            }
                            case ItemEffectType.NONE:
                            {
                                User.GetClient().GetHabbo().Effects().ApplyEffect(-1);
                                User.CurrentItemEffect = Type;
                                break;
                            }
                        }
                    }
                }
                else if (User.CurrentItemEffect != ItemEffectType.NONE && NewCurrentUserItemEffect == 0)
                {
                    User.GetClient().GetHabbo().Effects().ApplyEffect(-1);
                    User.CurrentItemEffect = ItemEffectType.NONE;
                }
            }
            catch
            {
            }
        }

        public ICollection<RoomUser> GetUserList() => _users.Values;

        public void Dispose()
        {
            UpdatePets();
            UpdateBots();
            _room.RoomData.UsersNow = 0;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `id` = '" + _room.Id + "' LIMIT 1");
            }
            _users.Clear();
            _pets.Clear();
            _bots.Clear();
            userCount = 0;
            PetCount = 0;
            _users = null;
            _pets = null;
            _bots = null;
            _room = null;
        }
    }
}