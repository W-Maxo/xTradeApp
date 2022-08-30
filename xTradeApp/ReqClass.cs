using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

namespace xTrade
{
    [XmlRoot("Req")]
    public class ReqClass
    {
        [XmlIgnore]
        public string Title { get; set; }

        [XmlIgnore]
        public string Description { get; set; }

        [XmlIgnore]
        public DateTime Category { get; set; }

        [XmlAttribute("ReqTvID")]
        public int ReqTvID { get; set; }

        [XmlAttribute("IDCl")]
        public int IDClient { get; set; }

        [XmlAttribute("DateCr")]
        public DateTime DateCreation { get; set; }

        [XmlAttribute("DateDel")]
        public DateTime DateDelivery { get; set; }

        [XmlAttribute("UrID")]
        public int UserID { get; set; }

        [XmlAttribute("Disc")]
        public double Discount { get; set; }

        [XmlAttribute("IDClP")]
        public int IDClientPoint { get; set; }

        [XmlAttribute("PaymentID")]
        public int PaymentID { get; set; }

        [XmlAttribute("PrID")]
        public int PriorityID { get; set; }

        [XmlAttribute("ReqStID")]
        public int ReqStatusID { get; set; }

        [XmlAttribute("Num")]
        public string Number { get; set; }

        [XmlAttribute("CurrID")]
        public int CurrencyID { get; set; }

        [XmlAttribute("Note")]
        public string Note { get; set; }

        [XmlAttribute("WID")]
        public int WarehouseID { get; set; }

        [XmlAttribute("Pst")]
        public bool Posted { get; set; }

        [XmlAttribute("UnqStr")]
        public string UnqStr { get; set; }

        [XmlIgnore]
        public bool Btnen
        {
            get { return !Posted; }
        }

        public double GetFullSum ()
        {
            double res = 0;

            foreach (var ril in ReqItemList)
            {
                res += ril.CostTv * ril.Count;
            }

            return res;
        }

        public void FillToView(InfData inf)
        {
            if (!inf.IsNull)
            {
                var tmpcc = (ClientsClass) inf.Clients[IDClient];
                Title = tmpcc.ClientName;

                Category = DateCreation.Date;

                Description = GetFullSum().ToString(CultureInfo.InvariantCulture) + " " +
                              inf.Currency[CurrencyID];
            }
        }
        [XmlArray("ReqItemList")]
        public List<ReqItem> ReqItemList { get; set; }

        public ReqClass()
        {
            ReqItemList = new List<ReqItem>();
        }
    }
}

