namespace Plus.Communication.Packets.Outgoing.Sound
{
    using System.Collections.Generic;

    internal class SoundSettingsComposer : ServerPacket
    {
        public SoundSettingsComposer(ICollection<int> ClientVolumes, bool ChatPreference, bool InvitesStatus,
            bool FocusPreference, int FriendBarState)
            : base(ServerPacketHeader.SoundSettingsMessageComposer)
        {
            foreach (var VolumeValue in ClientVolumes)
            {
                WriteInteger(VolumeValue);
            }

            WriteBoolean(ChatPreference);
            WriteBoolean(InvitesStatus);
            WriteBoolean(FocusPreference);
            WriteInteger(FriendBarState);
            WriteInteger(0);
            WriteInteger(0);
        }
    }
}