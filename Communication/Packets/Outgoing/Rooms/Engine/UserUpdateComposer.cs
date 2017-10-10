namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HabboHotel.Rooms;

    internal class UserUpdateComposer : ServerPacket
    {
        public UserUpdateComposer(ICollection<RoomUser> RoomUsers) : base(ServerPacketHeader.UserUpdateMessageComposer)
        {
            WriteInteger(RoomUsers.Count);
            foreach (var User in RoomUsers.ToList())
            {
                WriteInteger(User.VirtualId);
                WriteInteger(User.X);
                WriteInteger(User.Y);
                WriteString(User.Z.ToString("0.00"));
                WriteInteger(User.RotHead);
                WriteInteger(User.RotBody);
                var StatusComposer = new StringBuilder();
                StatusComposer.Append("/");
                foreach (var Status in User.Statusses.ToList())
                {
                    StatusComposer.Append(Status.Key);
                    if (!string.IsNullOrEmpty(Status.Value))
                    {
                        StatusComposer.Append(" ");
                        StatusComposer.Append(Status.Value);
                    }
                    StatusComposer.Append("/");
                }

                StatusComposer.Append("/");
                WriteString(StatusComposer.ToString());
            }
        }
    }
}