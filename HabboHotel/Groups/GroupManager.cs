﻿namespace Plus.HabboHotel.Groups
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using log4net;
    using Users;

    public class GroupManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Groups.GroupManager");
        private readonly Dictionary<int, GroupColours> _backgroundColours;
        private readonly List<GroupColours> _baseColours;

        private readonly List<GroupBadgeParts> _bases;

        private readonly object _groupLoadingSync;
        private readonly ConcurrentDictionary<int, Group> _groups;
        private readonly Dictionary<int, GroupColours> _symbolColours;
        private readonly List<GroupBadgeParts> _symbols;

        public GroupManager()
        {
            _groupLoadingSync = new object();
            _groups = new ConcurrentDictionary<int, Group>();
            _bases = new List<GroupBadgeParts>();
            _symbols = new List<GroupBadgeParts>();
            _baseColours = new List<GroupColours>();
            _symbolColours = new Dictionary<int, GroupColours>();
            _backgroundColours = new Dictionary<int, GroupColours>();
        }

        public ICollection<GroupBadgeParts> BadgeBases => _bases;

        public ICollection<GroupBadgeParts> BadgeSymbols => _symbols;

        public ICollection<GroupColours> BadgeBaseColours => _baseColours;

        public ICollection<GroupColours> BadgeSymbolColours => _symbolColours.Values;

        public ICollection<GroupColours> BadgeBackColours => _backgroundColours.Values;

        public void Init()
        {
            _bases.Clear();
            _symbols.Clear();
            _baseColours.Clear();
            _symbolColours.Clear();
            _backgroundColours.Clear();
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`type`,`firstvalue`,`secondvalue` FROM `groups_items` WHERE `enabled` = '1'");
                var dItems = dbClient.GetTable();
                foreach (DataRow dRow in dItems.Rows)
                {
                    switch (dRow["type"].ToString())
                    {
                        case "base":
                            _bases.Add(new GroupBadgeParts(Convert.ToInt32(dRow["id"]), dRow["firstvalue"].ToString(),
                                dRow["secondvalue"].ToString()));
                            break;
                        case "symbol":
                            _symbols.Add(
                                new GroupBadgeParts(Convert.ToInt32(dRow["id"]), dRow["firstvalue"].ToString(),
                                    dRow["secondvalue"].ToString()));
                            break;
                        case "color":
                            _baseColours.Add(new GroupColours(Convert.ToInt32(dRow["id"]), dRow["firstvalue"].ToString()));
                            break;
                        case "color2":
                            _symbolColours.Add(Convert.ToInt32(dRow["id"]),
                                new GroupColours(Convert.ToInt32(dRow["id"]), dRow["firstvalue"].ToString()));
                            break;
                        case "color3":
                            _backgroundColours.Add(Convert.ToInt32(dRow["id"]),
                                new GroupColours(Convert.ToInt32(dRow["id"]), dRow["firstvalue"].ToString()));
                            break;
                    }
                }
            }
        }

        public bool TryGetGroup(int id, out Group Group)
        {
            Group = null;
            if (_groups.ContainsKey(id))
            {
                return _groups.TryGetValue(id, out Group);
            }

            lock (_groupLoadingSync)
            {
                if (_groups.ContainsKey(id))
                {
                    return _groups.TryGetValue(id, out Group);
                }

                DataRow Row = null;
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM `groups` WHERE `id` = @id LIMIT 1");
                    dbClient.AddParameter("id", id);
                    Row = dbClient.GetRow();
                    if (Row != null)
                    {
                        Group = new Group(Convert.ToInt32(Row["id"]),
                            Convert.ToString(Row["name"]),
                            Convert.ToString(Row["desc"]),
                            Convert.ToString(Row["badge"]),
                            Convert.ToInt32(Row["room_id"]),
                            Convert.ToInt32(Row["owner_id"]),
                            Convert.ToInt32(Row["created"]),
                            Convert.ToInt32(Row["state"]),
                            Convert.ToInt32(Row["colour1"]),
                            Convert.ToInt32(Row["colour2"]),
                            Convert.ToInt32(Row["admindeco"]));
                        _groups.TryAdd(Group.Id, Group);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TryCreateGroup(Habbo Player, string Name, string Description, int RoomId, string Badge, int Colour1,
                                   int Colour2, out Group Group)
        {
            Group = new Group(0, Name, Description, Badge, RoomId, Player.Id, (int) PlusEnvironment.GetUnixTimestamp(), 0,
                Colour1, Colour2, 0);
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Badge))
            {
                return false;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `groups` (`name`, `desc`, `badge`, `owner_id`, `created`, `room_id`, `state`, `colour1`, `colour2`, `admindeco`) VALUES (@name, @desc, @badge, @owner, UNIX_TIMESTAMP(), @room, '0', @colour1, @colour2, '0')");
                dbClient.AddParameter("name", Group.Name);
                dbClient.AddParameter("desc", Group.Description);
                dbClient.AddParameter("owner", Group.CreatorId);
                dbClient.AddParameter("badge", Group.Badge);
                dbClient.AddParameter("room", Group.RoomId);
                dbClient.AddParameter("colour1", Group.Colour1);
                dbClient.AddParameter("colour2", Group.Colour2);
                Group.Id = Convert.ToInt32(dbClient.InsertQuery());
                Group.AddMember(Player.Id);
                Group.MakeAdmin(Player.Id);
                if (!_groups.TryAdd(Group.Id, Group))
                {
                    return false;
                }

                dbClient.SetQuery("UPDATE `rooms` SET `group_id` = @gid WHERE `id` = @rid LIMIT 1");
                dbClient.AddParameter("gid", Group.Id);
                dbClient.AddParameter("rid", Group.RoomId);
                dbClient.RunQuery();
                dbClient.RunQuery("DELETE FROM `room_rights` WHERE `room_id` = '" + RoomId + "'");
            }

            return true;
        }

        public string GetColourCode(int id, bool colourOne)
        {
            if (colourOne)
            {
                if (_symbolColours.ContainsKey(id))
                {
                    return _symbolColours[id].Colour;
                }
            }
            else
            {
                if (_backgroundColours.ContainsKey(id))
                {
                    return _backgroundColours[id].Colour;
                }
            }

            return "";
        }

        public void DeleteGroup(int id)
        {
            Group Group = null;
            if (_groups.ContainsKey(id))
            {
                _groups.TryRemove(id, out Group);
            }
            if (Group != null)
            {
                Group.Dispose();
            }
        }

        public List<Group> GetGroupsForUser(int userId)
        {
            var Groups = new List<Group>();
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT g.id FROM `group_memberships` AS m RIGHT JOIN `groups` AS g ON m.group_id = g.id WHERE m.user_id = @user");
                dbClient.AddParameter("user", userId);
                var GetGroups = dbClient.GetTable();
                if (GetGroups != null)
                {
                    foreach (DataRow Row in GetGroups.Rows)
                    {
                        Group Group = null;
                        if (TryGetGroup(Convert.ToInt32(Row["id"]), out Group))
                        {
                            Groups.Add(Group);
                        }
                    }
                }
            }

            return Groups;
        }
    }
}