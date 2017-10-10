namespace Plus.HabboHotel.GameClients
{
    using System;
    using Communication;
    using Communication.ConnectionManager;
    using Communication.Encryption.Crypto.Prng;
    using Communication.Interfaces;
    using Communication.Packets.Incoming;
    using Communication.Packets.Outgoing.BuildersClub;
    using Communication.Packets.Outgoing.Handshake;
    using Communication.Packets.Outgoing.Inventory.Achievements;
    using Communication.Packets.Outgoing.Inventory.AvatarEffects;
    using Communication.Packets.Outgoing.Moderation;
    using Communication.Packets.Outgoing.Navigator;
    using Communication.Packets.Outgoing.Notifications;
    using Communication.Packets.Outgoing.Rooms.Chat;
    using Communication.Packets.Outgoing.Sound;
    using Core;
    using Moderation;
    using Permissions;
    using Subscriptions;
    using Users;
    using Users.Messenger.FriendBar;
    using Users.UserData;

    public sealed class GameClient
    {
        private ConnectionInformation _connection;
        private bool _disconnected;
        private Habbo _habbo;
        private GamePacketParser _packetParser;
        public string MachineId;
        public Arc4 RC4Client;

        public GameClient(int ClientId, ConnectionInformation pConnection)
        {
            ConnectionID = ClientId;
            _connection = pConnection;
            _packetParser = new GamePacketParser(this);
            PingCount = 0;
        }

        public int PingCount { get; set; }

        public int ConnectionID { get; }

        private void SwitchParserRequest()
        {
            _packetParser.SetConnection(_connection);
            _packetParser.OnNewPacket += parser_onNewPacket;
            var data = (_connection.parser as InitialPacketParser).currentData;
            _connection.parser.Dispose();
            _connection.parser = _packetParser;
            _connection.parser.handlePacketData(data);
        }

        private void parser_onNewPacket(ClientPacket Message)
        {
            try
            {
                PlusEnvironment.GetGame().GetPacketManager().TryExecutePacket(this, Message);
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        private void PolicyRequest()
        {
            _connection.SendData(PlusEnvironment.GetDefaultEncoding()
                .GetBytes("<?xml version=\"1.0\"?>\r\n" +
                          "<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">\r\n" +
                          "<cross-domain-policy>\r\n" +
                          "<allow-access-from domain=\"*\" to-ports=\"1-31111\" />\r\n" +
                          "</cross-domain-policy>\x0"));
        }

        public void StartConnection()
        {
            if (_connection == null)
            {
                return;
            }

            PingCount = 0;
            (_connection.parser as InitialPacketParser).PolicyRequest += PolicyRequest;
            (_connection.parser as InitialPacketParser).SwitchParserRequest += SwitchParserRequest;
            _connection.startPacketProcessing();
        }

        public bool TryAuthenticate(string AuthTicket)
        {
            try
            {
                byte errorCode = 0;
                var userData = UserDataFactory.GetUserData(AuthTicket, out errorCode);
                if (errorCode == 1 || errorCode == 2)
                {
                    Disconnect();
                    return false;
                }

                //Let's have a quick search for a ban before we successfully authenticate..
                ModerationBan BanRecord = null;
                if (!string.IsNullOrEmpty(MachineId))
                {
                    if (PlusEnvironment.GetGame().GetModerationManager().IsBanned(MachineId, out BanRecord))
                    {
                        if (PlusEnvironment.GetGame().GetModerationManager().MachineBanCheck(MachineId))
                        {
                            Disconnect();
                            return false;
                        }
                    }
                }

                if (userData.user != null)
                {
                    //Now let us check for a username ban record..
                    BanRecord = null;
                    if (PlusEnvironment.GetGame().GetModerationManager().IsBanned(userData.user.Username, out BanRecord))
                    {
                        if (PlusEnvironment.GetGame().GetModerationManager().UsernameBanCheck(userData.user.Username))
                        {
                            Disconnect();
                            return false;
                        }
                    }
                }

                PlusEnvironment.GetGame().GetClientManager().RegisterClient(this, userData.userID, userData.user.Username);
                _habbo = userData.user;
                if (_habbo != null)
                {
                    userData.user.Init(this, userData);
                    SendPacket(new AuthenticationOKComposer());
                    SendPacket(new AvatarEffectsComposer(_habbo.Effects().GetAllEffects));
                    SendPacket(new NavigatorSettingsComposer(_habbo.HomeRoom));
                    SendPacket(new FavouritesComposer(userData.user.FavoriteRooms));
                    SendPacket(new FigureSetIdsComposer(_habbo.GetClothing().GetClothingParts));
                    SendPacket(new UserRightsComposer(_habbo.Rank));
                    SendPacket(new AvailabilityStatusComposer());
                    SendPacket(new AchievementScoreComposer(_habbo.GetStats().AchievementPoints));
                    SendPacket(new BuildersClubMembershipComposer());
                    SendPacket(new CfhTopicsInitComposer(PlusEnvironment.GetGame().GetModerationManager().UserActionPresets));
                    SendPacket(new BadgeDefinitionsComposer(PlusEnvironment.GetGame().GetAchievementManager()._achievements));
                    SendPacket(new SoundSettingsComposer(_habbo.ClientVolume,
                        _habbo.ChatPreference,
                        _habbo.AllowMessengerInvites,
                        _habbo.FocusPreference,
                        FriendBarStateUtility.GetInt(_habbo.FriendbarState)));

                    //SendMessage(new TalentTrackLevelComposer());
                    if (GetHabbo().GetMessenger() != null)
                    {
                        GetHabbo().GetMessenger().OnStatusChanged(true);
                    }
                    if (!string.IsNullOrEmpty(MachineId))
                    {
                        if (_habbo.MachineId != MachineId)
                        {
                            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("UPDATE `users` SET `machine_id` = @MachineId WHERE `id` = @id LIMIT 1");
                                dbClient.AddParameter("MachineId", MachineId);
                                dbClient.AddParameter("id", _habbo.Id);
                                dbClient.RunQuery();
                            }
                        }
                        _habbo.MachineId = MachineId;
                    }
                    PermissionGroup PermissionGroup = null;
                    if (PlusEnvironment.GetGame().GetPermissionManager().TryGetGroup(_habbo.Rank, out PermissionGroup))
                    {
                        if (!string.IsNullOrEmpty(PermissionGroup.Badge))
                        {
                            if (!_habbo.GetBadgeComponent().HasBadge(PermissionGroup.Badge))
                            {
                                _habbo.GetBadgeComponent().GiveBadge(PermissionGroup.Badge, true, this);
                            }
                        }
                    }
                    SubscriptionData SubData = null;
                    if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(_habbo.VIPRank, out SubData))
                    {
                        if (!string.IsNullOrEmpty(SubData.Badge))
                        {
                            if (!_habbo.GetBadgeComponent().HasBadge(SubData.Badge))
                            {
                                _habbo.GetBadgeComponent().GiveBadge(SubData.Badge, true, this);
                            }
                        }
                    }
                    if (!PlusEnvironment.GetGame().GetCacheManager().ContainsUser(_habbo.Id))
                    {
                        PlusEnvironment.GetGame().GetCacheManager().GenerateUser(_habbo.Id);
                    }
                    _habbo.Look = PlusEnvironment.GetFigureManager()
                        .ProcessFigure(_habbo.Look, _habbo.Gender, _habbo.GetClothing().GetClothingParts, true);
                    _habbo.InitProcess();
                    if (userData.user.GetPermissions().HasRight("mod_tickets"))
                    {
                        SendPacket(new ModeratorInitComposer(PlusEnvironment.GetGame().GetModerationManager().UserMessagePresets,
                            PlusEnvironment.GetGame().GetModerationManager().RoomMessagePresets,
                            PlusEnvironment.GetGame().GetModerationManager().GetTickets));
                    }
                    if (PlusEnvironment.GetSettingsManager().TryGetValue("user.login.message.enabled") == "1")
                    {
                        SendPacket(new MOTDNotificationComposer(PlusEnvironment.GetLanguageManager()
                            .TryGetValue("user.login.message")));
                    }
                    PlusEnvironment.GetGame().GetRewardManager().CheckRewards(this);
                    return true;
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }

            return false;
        }

        public void SendWhisper(string Message, int Colour = 0)
        {
            if (GetHabbo() == null || GetHabbo().CurrentRoom == null)
            {
                return;
            }

            var User = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().Username);
            if (User == null)
            {
                return;
            }

            SendPacket(new WhisperComposer(User.VirtualId, Message, 0, Colour == 0 ? User.LastBubble : Colour));
        }

        public void SendNotification(string Message)
        {
            SendPacket(new BroadcastMessageAlertComposer(Message));
        }

        public void SendPacket(IServerPacket Message)
        {
            var bytes = Message.GetBytes();
            if (Message == null)
            {
                return;
            }
            if (GetConnection() == null)
            {
                return;
            }

            GetConnection().SendData(bytes);
        }

        public ConnectionInformation GetConnection() => _connection;

        public Habbo GetHabbo() => _habbo;

        public void Disconnect()
        {
            try
            {
                if (GetHabbo() != null)
                {
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery(GetHabbo().GetQueryString);
                    }
                    GetHabbo().OnDisconnect();
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
            if (!_disconnected)
            {
                if (_connection != null)
                {
                    _connection.Dispose();
                }
                _disconnected = true;
            }
        }

        public void Dispose()
        {
            if (GetHabbo() != null)
            {
                GetHabbo().OnDisconnect();
            }
            MachineId = string.Empty;
            _disconnected = true;
            _habbo = null;
            _connection = null;
            RC4Client = null;
            _packetParser = null;
        }
    }
}