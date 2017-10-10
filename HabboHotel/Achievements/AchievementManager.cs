namespace Plus.HabboHotel.Achievements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Packets.Outgoing.Inventory.Achievements;
    using Communication.Packets.Outgoing.Inventory.Purse;
    using GameClients;
    using log4net;
    using Users.Messenger;

    public class AchievementManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Achievements.AchievementManager");

        public Dictionary<string, Achievement> _achievements;

        public AchievementManager()
        {
            _achievements = new Dictionary<string, Achievement>();
            LoadAchievements();
            log.Info("Achievement Manager -> LOADED");
        }

        public void LoadAchievements()
        {
            AchievementLevelFactory.GetAchievementLevels(out _achievements);
        }

        public bool ProgressAchievement(GameClient Session, string AchievementGroup, int ProgressAmount, bool FromZero = false)
        {
            if (!_achievements.ContainsKey(AchievementGroup) || Session == null)
            {
                return false;
            }

            Achievement AchievementData = null;
            AchievementData = _achievements[AchievementGroup];
            var UserData = Session.GetHabbo().GetAchievementData(AchievementGroup);
            if (UserData == null)
            {
                UserData = new UserAchievement(AchievementGroup, 0, 0);
                Session.GetHabbo().Achievements.TryAdd(AchievementGroup, UserData);
            }
            var TotalLevels = AchievementData.Levels.Count;
            if (UserData != null && UserData.Level == TotalLevels)
            {
                return false; // done, no more.
            }

            var TargetLevel = UserData != null ? UserData.Level + 1 : 1;
            if (TargetLevel > TotalLevels)
            {
                TargetLevel = TotalLevels;
            }
            var TargetLevelData = AchievementData.Levels[TargetLevel];
            var NewProgress = 0;
            if (FromZero)
            {
                NewProgress = ProgressAmount;
            }
            else
            {
                NewProgress = UserData != null ? UserData.Progress + ProgressAmount : ProgressAmount;
            }
            var NewLevel = UserData != null ? UserData.Level : 0;
            var NewTarget = NewLevel + 1;
            if (NewTarget > TotalLevels)
            {
                NewTarget = TotalLevels;
            }
            if (NewProgress >= TargetLevelData.Requirement)
            {
                NewLevel++;
                NewTarget++;
                var ProgressRemainder = NewProgress - TargetLevelData.Requirement;
                NewProgress = 0;
                if (TargetLevel == 1)
                {
                    Session.GetHabbo().GetBadgeComponent().GiveBadge(AchievementGroup + TargetLevel, true, Session);
                }
                else
                {
                    Session.GetHabbo().GetBadgeComponent().RemoveBadge(Convert.ToString(AchievementGroup + (TargetLevel - 1)));
                    Session.GetHabbo().GetBadgeComponent().GiveBadge(AchievementGroup + TargetLevel, true, Session);
                }
                if (NewTarget > TotalLevels)
                {
                    NewTarget = TotalLevels;
                }
                Session.SendPacket(new AchievementUnlockedComposer(AchievementData,
                    TargetLevel,
                    TargetLevelData.RewardPoints,
                    TargetLevelData.RewardPixels));
                Session.GetHabbo()
                    .GetMessenger()
                    .BroadcastAchievement(Session.GetHabbo().Id, MessengerEventTypes.ACHIEVEMENT_UNLOCKED,
                        AchievementGroup + TargetLevel);
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("REPLACE INTO `user_achievements` VALUES ('" +
                                      Session.GetHabbo().Id +
                                      "', @group, '" +
                                      NewLevel +
                                      "', '" +
                                      NewProgress +
                                      "')");
                    dbClient.AddParameter("group", AchievementGroup);
                    dbClient.RunQuery();
                }
                UserData.Level = NewLevel;
                UserData.Progress = NewProgress;
                Session.GetHabbo().Duckets += TargetLevelData.RewardPixels;
                Session.GetHabbo().GetStats().AchievementPoints += TargetLevelData.RewardPoints;
                Session.SendPacket(
                    new HabboActivityPointNotificationComposer(Session.GetHabbo().Duckets, TargetLevelData.RewardPixels));
                Session.SendPacket(new AchievementScoreComposer(Session.GetHabbo().GetStats().AchievementPoints));
                var NewLevelData = AchievementData.Levels[NewTarget];
                Session.SendPacket(new AchievementProgressedComposer(AchievementData,
                    NewTarget,
                    NewLevelData,
                    TotalLevels,
                    Session.GetHabbo().GetAchievementData(AchievementGroup)));
                return true;
            }

            UserData.Level = NewLevel;
            UserData.Progress = NewProgress;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("REPLACE INTO `user_achievements` VALUES ('" +
                                  Session.GetHabbo().Id +
                                  "', @group, '" +
                                  NewLevel +
                                  "', '" +
                                  NewProgress +
                                  "')");
                dbClient.AddParameter("group", AchievementGroup);
                dbClient.RunQuery();
            }
            Session.SendPacket(new AchievementProgressedComposer(AchievementData,
                TargetLevel,
                TargetLevelData,
                TotalLevels,
                Session.GetHabbo().GetAchievementData(AchievementGroup)));
            return false;
        }

        public ICollection<Achievement> GetGameAchievements(int GameId)
        {
            var GameAchievements = new List<Achievement>();
            foreach (var Achievement in _achievements.Values.ToList())
            {
                if (Achievement.Category == "games" && Achievement.GameId == GameId)
                {
                    GameAchievements.Add(Achievement);
                }
            }

            return GameAchievements;
        }
    }
}