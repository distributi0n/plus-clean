namespace Plus.Communication.Packets.Outgoing.Campaigns
{
    using System;
    using System.Collections.Generic;

    internal class CampaignCalendarDataComposer : ServerPacket
    {
        public CampaignCalendarDataComposer(List<int> OpenedBoxes, List<int> LateBoxes) : base(ServerPacketHeader
            .CampaignCalendarDataMessageComposer)
        {
            WriteString("xmas15"); //Set the campaign.
            WriteString(""); //No idea.
            WriteInteger(DateTime.Now.Day - 1); //Start
            WriteInteger(25); //End?

            //Opened boxes
            WriteInteger(OpenedBoxes.Count);
            foreach (var Day in OpenedBoxes)
            {
                WriteInteger(Day);
            }

            //Late boxes?
            WriteInteger(LateBoxes.Count);
            foreach (var Day in LateBoxes)
            {
                WriteInteger(Day);
            }
        }
    }
}