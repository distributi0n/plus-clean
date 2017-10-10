namespace Plus.HabboHotel.Catalog
{
    using System.Collections.Generic;

    public class CatalogPage
    {
        public CatalogPage(int Id,
            int ParentId,
            string Enabled,
            string Caption,
            string PageLink,
            int Icon,
            int MinRank,
            int MinVIP,
            string Visible,
            string Template,
            string PageStrings1,
            string PageStrings2,
            Dictionary<int, CatalogItem> Items,
            ref Dictionary<int, int> flatOffers)
        {
            this.Id = Id;
            this.ParentId = ParentId;
            this.Enabled = Enabled.ToLower() == "1" ? true : false;
            this.Caption = Caption;
            this.PageLink = PageLink;
            this.Icon = Icon;
            MinimumRank = MinRank;
            MinimumVIP = MinVIP;
            this.Visible = Visible.ToLower() == "1" ? true : false;
            this.Template = Template;
            foreach (var Str in PageStrings1.Split('|'))
            {
                if (this.PageStrings1 == null)
                {
                    this.PageStrings1 = new List<string>();
                }
                this.PageStrings1.Add(Str);
            }
            foreach (var Str in PageStrings2.Split('|'))
            {
                if (this.PageStrings2 == null)
                {
                    this.PageStrings2 = new List<string>();
                }
                this.PageStrings2.Add(Str);
            }

            this.Items = Items;
            ItemOffers = new Dictionary<int, CatalogItem>();
            foreach (var i in flatOffers.Keys)
            {
                if (flatOffers[i] == Id)
                {
                    foreach (var item in this.Items.Values)
                    {
                        if (item.OfferId == i)
                        {
                            if (!ItemOffers.ContainsKey(i))
                            {
                                ItemOffers.Add(i, item);
                            }
                        }
                    }
                }
            }
        }

        public int Id { get; set; }

        public int ParentId { get; set; }

        public bool Enabled { get; set; }

        public string Caption { get; set; }

        public string PageLink { get; set; }

        public int Icon { get; set; }

        public int MinimumRank { get; set; }

        public int MinimumVIP { get; set; }

        public bool Visible { get; set; }

        public string Template { get; set; }

        public List<string> PageStrings1 { get; }

        public List<string> PageStrings2 { get; }

        public Dictionary<int, CatalogItem> Items { get; }

        public Dictionary<int, CatalogItem> ItemOffers { get; }

        public CatalogItem GetItem(int pId)
        {
            if (Items.ContainsKey(pId))
            {
                return Items[pId];
            }

            return null;
        }
    }
}