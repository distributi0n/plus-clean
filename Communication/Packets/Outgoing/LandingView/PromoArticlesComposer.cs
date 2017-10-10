﻿namespace Plus.Communication.Packets.Outgoing.LandingView
{
    using System.Collections.Generic;
    using System.Linq;
    using HabboHotel.LandingView.Promotions;

    internal class PromoArticlesComposer : ServerPacket
    {
        public PromoArticlesComposer(ICollection<Promotion> LandingPromotions) : base(ServerPacketHeader
            .PromoArticlesMessageComposer)
        {
            WriteInteger(LandingPromotions.Count); //Count
            foreach (var Promotion in LandingPromotions.ToList())
            {
                WriteInteger(Promotion.Id); //ID
                WriteString(Promotion.Title); //Title
                WriteString(Promotion.Text); //Text
                WriteString(Promotion.ButtonText); //Button text
                WriteInteger(Promotion.ButtonType); //Link type 0 and 3
                WriteString(Promotion.ButtonLink); //Link to article
                WriteString(Promotion.ImageLink); //Image link
            }
        }
    }
}