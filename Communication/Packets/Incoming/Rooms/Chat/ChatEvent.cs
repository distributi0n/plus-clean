namespace Plus.Communication.Packets.Incoming.Rooms.Chat
{
    using System;
    using HabboHotel.GameClients;
    using HabboHotel.Moderation;
    using HabboHotel.Quests;
    using HabboHotel.Rooms.Chat.Logs;
    using HabboHotel.Rooms.Chat.Styles;
    using Outgoing.Moderation;
    using Outgoing.Rooms.Chat;
    using Utilities;

    public class ChatEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().InRoom)
            {
                return;
            }

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            var Message = StringCharFilter.Escape(Packet.PopString());
            if (Message.Length > 100)
            {
                Message = Message.Substring(0, 100);
            }
            var Colour = Packet.PopInt();
            ChatStyle Style = null;
            if (!PlusEnvironment.GetGame().GetChatManager().GetChatStyles().TryGetStyle(Colour, out Style) ||
                Style.RequiredRight.Length > 0 && !Session.GetHabbo().GetPermissions().HasRight(Style.RequiredRight))
            {
                Colour = 0;
            }
            User.UnIdle();
            if (PlusEnvironment.GetUnixTimestamp() < Session.GetHabbo().FloodTime && Session.GetHabbo().FloodTime != 0)
            {
                return;
            }

            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendPacket(new MutedComposer(Session.GetHabbo().TimeMuted));
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("room_ignore_mute") && Room.CheckMute(Session))
            {
                Session.SendWhisper("Oops, you're currently muted.");
                return;
            }

            User.LastBubble = Session.GetHabbo().CustomBubbleId == 0 ? Colour : Session.GetHabbo().CustomBubbleId;
            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                int MuteTime;
                if (User.IncrementAndCheckFlood(out MuteTime))
                {
                    Session.SendPacket(new FloodControlComposer(MuteTime));
                    return;
                }
            }

            PlusEnvironment.GetGame()
                .GetChatManager()
                .GetLogs()
                .StoreChatlog(new ChatlogEntry(Session.GetHabbo().Id, Room.Id, Message, UnixTimestamp.GetNow(),
                    Session.GetHabbo(), Room));
            if (Message.StartsWith(":", StringComparison.CurrentCulture) &&
                PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Session, Message))
            {
                return;
            }

            if (PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckBannedWords(Message))
            {
                Session.GetHabbo().BannedPhraseCount++;
                if (Session.GetHabbo().BannedPhraseCount >=
                    Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("room.chat.filter.banned_phrases.chances")))
                {
                    PlusEnvironment.GetGame()
                        .GetModerationManager()
                        .BanUser("System",
                            ModerationBanType.USERNAME,
                            Session.GetHabbo().Username,
                            "Spamming banned phrases (" + Message + ")",
                            PlusEnvironment.GetUnixTimestamp() + 78892200);
                    Session.Disconnect();
                    return;
                }

                Session.SendPacket(new ChatComposer(User.VirtualId, Message, 0, Colour));
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
            {
                Message = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Message);
            }
            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_CHAT);
            User.OnChat(User.LastBubble, Message, false);
        }
    }
}