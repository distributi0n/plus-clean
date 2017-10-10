namespace Plus.Communication.Packets.Incoming.Users
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.GameClients;
    using Outgoing.Users;

    internal class CheckValidNameEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var InUse = false;
            var Name = Packet.PopString();
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `users` WHERE `username` = @name LIMIT 1");
                dbClient.AddParameter("name", Name);
                InUse = dbClient.GetInteger() == 1;
            }
            var Letters = Name.ToLower().ToCharArray();
            var AllowedCharacters = "abcdefghijklmnopqrstuvwxyz.,_-;:?!1234567890";
            foreach (var Chr in Letters)
            {
                if (!AllowedCharacters.Contains(Chr))
                {
                    Session.SendPacket(new NameChangeUpdateComposer(Name, 4));
                    return;
                }
            }

            if (PlusEnvironment.GetGame().GetChatManager().GetFilter().IsFiltered(Name))
            {
                Session.SendPacket(new NameChangeUpdateComposer(Name, 4));
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool") && Name.ToLower().Contains("mod") ||
                Name.ToLower().Contains("adm") ||
                Name.ToLower().Contains("admin") ||
                Name.ToLower().Contains("m0d"))
            {
                Session.SendPacket(new NameChangeUpdateComposer(Name, 4));
                return;
            }

            if (!Name.ToLower().Contains("mod") && (Session.GetHabbo().Rank == 2 || Session.GetHabbo().Rank == 3))
            {
                Session.SendPacket(new NameChangeUpdateComposer(Name, 4));
                return;
            }

            if (Name.Length > 15)
            {
                Session.SendPacket(new NameChangeUpdateComposer(Name, 3));
                return;
            }

            if (Name.Length < 3)
            {
                Session.SendPacket(new NameChangeUpdateComposer(Name, 2));
                return;
            }

            if (InUse)
            {
                ICollection<string> Suggestions = new List<string>();
                for (var i = 100; i < 103; i++)
                {
                    Suggestions.Add(i.ToString());
                }

                Session.SendPacket(new NameChangeUpdateComposer(Name, 5, Suggestions));
                return;
            }

            Session.SendPacket(new NameChangeUpdateComposer(Name, 0));
        }
    }
}