﻿namespace Plus.HabboHotel.Talents
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using log4net;

    public class TalentTrackManager
    {
        private static ILog log = LogManager.GetLogger("Plus.HabboHotel.Talents.TalentManager");

        private readonly Dictionary<int, TalentTrackLevel> _citizenshipLevels;

        public TalentTrackManager()
        {
            _citizenshipLevels = new Dictionary<int, TalentTrackLevel>();
            Init();
        }

        public void Init()
        {
            DataTable GetTable = null;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `type`,`level`,`data_actions`,`data_gifts` FROM `talents`");
                GetTable = dbClient.GetTable();
            }
            if (GetTable != null)
            {
                foreach (DataRow Row in GetTable.Rows)
                {
                    _citizenshipLevels.Add(Convert.ToInt32(Row["level"]),
                        new TalentTrackLevel(Convert.ToString(Row["type"]),
                            Convert.ToInt32(Row["level"]),
                            Convert.ToString(Row["data_actions"]),
                            Convert.ToString(Row["data_gifts"])));
                }
            }
        }

        public ICollection<TalentTrackLevel> GetLevels() => _citizenshipLevels.Values;
    }
}