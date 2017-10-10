﻿namespace Plus.Communication.Packets.Outgoing.Catalog
{
    using System.Collections.Generic;
    using HabboHotel.Catalog;
    using HabboHotel.GameClients;

    public class CatalogIndexComposer : ServerPacket
    {
        public CatalogIndexComposer(GameClient Session, ICollection<CatalogPage> Pages, int Sub = 0) : base(ServerPacketHeader
            .CatalogIndexMessageComposer)
        {
            WriteRootIndex(Session, Pages);
            foreach (var Parent in Pages)
            {
                if (Parent.ParentId != -1 ||
                    Parent.MinimumRank > Session.GetHabbo().Rank ||
                    Parent.MinimumVIP > Session.GetHabbo().VIPRank && Session.GetHabbo().Rank == 1)
                {
                    continue;
                }

                WritePage(Parent, CalcTreeSize(Session, Pages, Parent.Id));
                foreach (var child in Pages)
                {
                    if (child.ParentId != Parent.Id ||
                        child.MinimumRank > Session.GetHabbo().Rank ||
                        child.MinimumVIP > Session.GetHabbo().VIPRank && Session.GetHabbo().Rank == 1)
                    {
                        continue;
                    }

                    if (child.Enabled)
                    {
                        WritePage(child, CalcTreeSize(Session, Pages, child.Id));
                    }
                    else
                    {
                        WriteNodeIndex(child, CalcTreeSize(Session, Pages, child.Id));
                    }
                    foreach (var SubChild in Pages)
                    {
                        if (SubChild.ParentId != child.Id || SubChild.MinimumRank > Session.GetHabbo().Rank)
                        {
                            continue;
                        }

                        WritePage(SubChild, 0);
                    }
                }
            }

            WriteBoolean(false);
            WriteString("NORMAL");
        }

        public void WriteRootIndex(GameClient session, ICollection<CatalogPage> pages)
        {
            WriteBoolean(true);
            WriteInteger(0);
            WriteInteger(-1);
            WriteString("root");
            WriteString(string.Empty);
            WriteInteger(0);
            WriteInteger(CalcTreeSize(session, pages, -1));
        }

        public void WriteNodeIndex(CatalogPage page, int treeSize)
        {
            WriteBoolean(page.Visible);
            WriteInteger(page.Icon);
            WriteInteger(-1);
            WriteString(page.PageLink);
            WriteString(page.Caption);
            WriteInteger(0);
            WriteInteger(treeSize);
        }

        public void WritePage(CatalogPage page, int treeSize)
        {
            WriteBoolean(page.Visible);
            WriteInteger(page.Icon);
            WriteInteger(page.Id);
            WriteString(page.PageLink);
            WriteString(page.Caption);
            WriteInteger(page.ItemOffers.Count);
            foreach (var i in page.ItemOffers.Keys)
            {
                WriteInteger(i);
            }

            WriteInteger(treeSize);
        }

        public int CalcTreeSize(GameClient Session, ICollection<CatalogPage> Pages, int ParentId)
        {
            var i = 0;
            foreach (var Page in Pages)
            {
                if (Page.MinimumRank > Session.GetHabbo().Rank ||
                    Page.MinimumVIP > Session.GetHabbo().VIPRank && Session.GetHabbo().Rank == 1 ||
                    Page.ParentId != ParentId)
                {
                    continue;
                }

                if (Page.ParentId == ParentId)
                {
                    i++;
                }
            }

            return i;
        }
    }
}