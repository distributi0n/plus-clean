namespace Plus.Communication.RCON.Commands.User
{
    using System;

    internal class ProgressUserAchievementCommand : IRCONCommand
    {
        public string Description => "This command is used to progress a users achievement.";

        public string Parameters => "%userId% %achievement% %progess%";

        public bool TryExecute(string[] parameters)
        {
            var userId = 0;
            if (!int.TryParse(parameters[0], out userId))
            {
                return false;
            }

            var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(userId);
            if (client == null || client.GetHabbo() == null)
            {
                return false;
            }

            // Validate the achievement
            if (string.IsNullOrEmpty(Convert.ToString(parameters[1])))
            {
                return false;
            }

            var achievement = Convert.ToString(parameters[1]);

            // Validate the progress
            var progress = 0;
            if (!int.TryParse(parameters[2], out progress))
            {
                return false;
            }

            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(client, achievement, progress);
            return true;
        }
    }
}