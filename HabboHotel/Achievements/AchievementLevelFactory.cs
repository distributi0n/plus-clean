namespace Plus.HabboHotel.Achievements
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    public class AchievementLevelFactory
    {
        public static void GetAchievementLevels(out Dictionary<string, Achievement> Achievements)
        {
            Achievements = new Dictionary<string, Achievement>();
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `id`,`category`,`group_name`,`level`,`reward_pixels`,`reward_points`,`progress_needed`,`game_id` FROM `achievements`");
                var dTable = dbClient.GetTable();
                if (dTable != null)
                {
                    foreach (DataRow dRow in dTable.Rows)
                    {
                        var id = Convert.ToInt32(dRow["id"]);
                        var category = Convert.ToString(dRow["category"]);
                        var groupName = Convert.ToString(dRow["group_name"]);
                        var level = Convert.ToInt32(dRow["level"]);
                        var rewardPixels = Convert.ToInt32(dRow["reward_pixels"]);
                        var rewardPoints = Convert.ToInt32(dRow["reward_points"]);
                        var progressNeeded = Convert.ToInt32(dRow["progress_needed"]);
                        var AchievementLevel = new AchievementLevel(level, rewardPixels, rewardPoints, progressNeeded);
                        if (!Achievements.ContainsKey(groupName))
                        {
                            var Achievement = new Achievement(id, groupName, category, Convert.ToInt32(dRow["game_id"]));
                            Achievement.AddLevel(AchievementLevel);
                            Achievements.Add(groupName, Achievement);
                        }
                        else
                        {
                            Achievements[groupName].AddLevel(AchievementLevel);
                        }
                    }
                }
            }
        }
    }
}