using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Xml;

namespace xTrade
{
    public static class ReqListClass
    {
        public static IEnumerable<ReqClass> GetReqListClass(InfData inf, string fileName)
        {
            var newList = new List<ReqClass>();

            try
            {
                using (var appStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!appStorage.FileExists(fileName)) return null;

                    using (var file = appStorage.OpenFile(fileName, FileMode.Open))
                    {
                        using (var reader = XmlReader.Create(file))
                        {
                            while (reader.Read())
                            {
                                if (0 != String.CompareOrdinal(reader.Name, "Request")) continue;
                                var tmpReq = new ReqClass();

                                string attr = reader.GetAttribute("IDClient");
                                tmpReq.IDClient = !String.IsNullOrEmpty(attr) ? int.Parse(attr) : -1;

                                attr = reader.GetAttribute("DateCreation");
                                tmpReq.DateCreation = !String.IsNullOrEmpty(attr)
                                                          ? DateTime.Parse(attr, new CultureInfo("en-US"))
                                                          : DateTime.MinValue;

                                attr = reader.GetAttribute("DateDelivery");
                                tmpReq.DateDelivery = !String.IsNullOrEmpty(attr)
                                                          ? DateTime.Parse(attr, new CultureInfo("en-US"))
                                                          : DateTime.MinValue;

                                tmpReq.Category = tmpReq.DateCreation.Date;

                                attr = reader.GetAttribute("UserID");
                                tmpReq.UserID = !String.IsNullOrEmpty(attr) ? int.Parse(attr) : -1;

                                attr = reader.GetAttribute("Discount");
                                tmpReq.Discount = !String.IsNullOrEmpty(attr) ? double.Parse(attr) : -1;

                                attr = reader.GetAttribute("IDClientPoint");
                                tmpReq.IDClientPoint = !String.IsNullOrEmpty(attr) ? int.Parse(attr) : -1;

                                attr = reader.GetAttribute("TPaymentID");
                                tmpReq.PaymentID = !String.IsNullOrEmpty(attr) ? int.Parse(attr) : -1;

                                attr = reader.GetAttribute("PriorityID");
                                tmpReq.PriorityID = !String.IsNullOrEmpty(attr) ? int.Parse(attr) : -1;

                                attr = reader.GetAttribute("ReqStatusID");
                                tmpReq.ReqStatusID = !String.IsNullOrEmpty(attr) ? int.Parse(attr) : -1;

                                attr = reader.GetAttribute("Number");
                                tmpReq.Number = !String.IsNullOrEmpty(attr) ? attr : string.Empty;

                                attr = reader.GetAttribute("CurrencyID");
                                tmpReq.CurrencyID = !String.IsNullOrEmpty(attr) ? int.Parse(attr) : -1;

                                attr = reader.GetAttribute("Note");
                                tmpReq.Note = !String.IsNullOrEmpty(attr) ? attr : string.Empty;

                                attr = reader.GetAttribute("WarehouseID");
                                tmpReq.WarehouseID = !String.IsNullOrEmpty(attr) ? int.Parse(attr) : -1;

                                attr = reader.GetAttribute("UnqStr");
                                tmpReq.UnqStr = !String.IsNullOrEmpty(attr) ? attr : string.Empty;

                                attr = reader.GetAttribute("ReqTvID");
                                tmpReq.ReqTvID = !String.IsNullOrEmpty(attr) ? int.Parse(attr) : -1;

                                var tmpcc = (ClientsClass) inf.Clients[tmpReq.IDClient];

                                tmpReq.Title = tmpcc.ClientName;

                                if (!String.IsNullOrEmpty(attr))
                                {
                                    XmlReader readerSubtree = reader.ReadSubtree();

                                    while (readerSubtree.Read())
                                    {
                                        if (readerSubtree.Name == "Item")
                                        {
                                            var tmpReqItem = new ReqItem();

                                            string itemsAttr = readerSubtree.GetAttribute("TvID");
                                            tmpReqItem.TvID = !String.IsNullOrEmpty(itemsAttr)
                                                                  ? int.Parse(itemsAttr)
                                                                  : -1;

                                            itemsAttr = readerSubtree.GetAttribute("CodeTv");
                                            tmpReqItem.CodeTv = !String.IsNullOrEmpty(itemsAttr)
                                                                    ? int.Parse(itemsAttr)
                                                                    : -1;

                                            itemsAttr = readerSubtree.GetAttribute("Count");
                                            tmpReqItem.Count = !String.IsNullOrEmpty(itemsAttr)
                                                                   ? int.Parse(itemsAttr)
                                                                   : -1;

                                            itemsAttr = readerSubtree.GetAttribute("CostTv");
                                            tmpReqItem.CostTv = !String.IsNullOrEmpty(itemsAttr)
                                                                    ? double.Parse(itemsAttr)
                                                                    : -1;

                                            itemsAttr = readerSubtree.GetAttribute("CurrencyID");
                                            tmpReqItem.CurrencyID = !String.IsNullOrEmpty(itemsAttr)
                                                                        ? int.Parse(itemsAttr)
                                                                        : -1;

                                            tmpReq.ReqItemList.Add(tmpReqItem);
                                        }
                                    }
                                }

                                tmpReq.Description = tmpReq.GetFullSum().ToString(CultureInfo.InvariantCulture) + " " +
                                                     inf.Currency[tmpReq.CurrencyID];
                                newList.Add(tmpReq);
                            }
                        }
                    }

                }
                return newList;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, string.Format("Ошибка загрузки файла! ({0})", fileName), MessageBoxButton.OK);
                return null;
            }
        }

    }
}
