using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Xml;

namespace xTrade
{
    public class InfData
    {
        public bool IsNull { get; private set; }

        public Dictionary<object, object> ReqStatus { get; private set; }
        public Dictionary<object, object> ReqStatusNew { get; private set; }
        public Dictionary<object, object> ReqPriority { get; private set; }
        public Dictionary<object, object> Currency { get; private set; }
        public Dictionary<object, object> Warehouse { get; private set; }
        public Dictionary<object, object> TypePayment { get; private set; }

        public Dictionary<object, object> Clients { get; private set; }

        public InfData()
        {
            IsNull = true;

            try
            {
                using (var appStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (appStorage.FileExists("Inf.xml"))
                    {
                        using (var file = appStorage.OpenFile("Inf.xml", FileMode.Open))
                        {
                            using (var reader = XmlReader.Create(file))
                            {
                                while (reader.Read())
                                {
                                    if (0 == String.CompareOrdinal(reader.Name, "ReqStatus"))
                                    {
                                        XmlReader readerSubtree = reader.ReadSubtree();

                                        ReqStatus = new Dictionary<object, object>();
                                        ReqStatusNew = new Dictionary<object, object>();

                                        while (readerSubtree.Read())
                                        {
                                            if (0 == String.CompareOrdinal(readerSubtree.Name, "RS"))
                                            {
                                                string id = readerSubtree.GetAttribute("ID");
                                                string rsn = readerSubtree.GetAttribute("RSN");
                                                string cn = readerSubtree.GetAttribute("CNew");

                                                if (!String.IsNullOrEmpty(id) || !String.IsNullOrEmpty(rsn) || !String.IsNullOrEmpty(cn))
                                                {
                                                    int idint = int.Parse(id);
                                                    bool cnew = bool.Parse(cn);

                                                    if (cnew)
                                                    {
                                                        ReqStatusNew.Add(idint, new SimpleData {ID = idint, ClName = rsn});
                                                    }

                                                    ReqStatus.Add(idint, new SimpleData { ID = idint, ClName = rsn });
                                                }
                                            }
                                        }
                                    }

                                    if (0 == String.CompareOrdinal(reader.Name, "ReqPriority"))
                                    {
                                        XmlReader readerSubtree = reader.ReadSubtree();

                                        ReqPriority = new Dictionary<object, object>();

                                        while (readerSubtree.Read())
                                        {
                                            if (0 == String.CompareOrdinal(readerSubtree.Name, "RP"))
                                            {
                                                string id = readerSubtree.GetAttribute("ID");
                                                string rpn = readerSubtree.GetAttribute("RPN");

                                                if (!String.IsNullOrEmpty(id) || !String.IsNullOrEmpty(rpn))
                                                {
                                                    int idint = int.Parse(id);

                                                    ReqPriority.Add(idint, new SimpleData { ID = idint, ClName = rpn });
                                                }
                                            }
                                        }
                                    }

                                    if (0 == String.CompareOrdinal(reader.Name, "Currency"))
                                    {
                                        XmlReader readerSubtree = reader.ReadSubtree();

                                        Currency = new Dictionary<object, object>();

                                        while (readerSubtree.Read())
                                        {
                                            if (0 == String.CompareOrdinal(readerSubtree.Name, "CR"))
                                            {
                                                string id = readerSubtree.GetAttribute("ID");
                                                string crn = readerSubtree.GetAttribute("CRN");

                                                if (!String.IsNullOrEmpty(id) || !String.IsNullOrEmpty(crn))
                                                {
                                                    int idint = int.Parse(id);

                                                    Currency.Add(idint, new SimpleData { ID = idint , ClName = crn });
                                                }
                                            }
                                        }
                                    }

                                    if (0 == String.CompareOrdinal(reader.Name, "Warehouse"))
                                    {
                                        XmlReader readerSubtree = reader.ReadSubtree();

                                        Warehouse = new Dictionary<object, object>();

                                        while (readerSubtree.Read())
                                        {
                                            if (0 == String.CompareOrdinal(readerSubtree.Name, "WR"))
                                            {
                                                string id = readerSubtree.GetAttribute("ID");
                                                string wrn = readerSubtree.GetAttribute("WRN");

                                                if (!String.IsNullOrEmpty(id) || !String.IsNullOrEmpty(wrn))
                                                {
                                                    int idint = int.Parse(id);

                                                    Warehouse.Add(idint, new SimpleData { ID = idint, ClName = wrn });
                                                }
                                            }
                                        }
                                    }

                                    if (0 == String.CompareOrdinal(reader.Name, "TypePayment"))
                                    {
                                        XmlReader readerSubtree = reader.ReadSubtree();

                                        TypePayment = new Dictionary<object, object>();

                                        while (readerSubtree.Read())
                                        {
                                            if (0 == String.CompareOrdinal(readerSubtree.Name, "TP"))
                                            {
                                                string id = readerSubtree.GetAttribute("ID");
                                                string tpn = readerSubtree.GetAttribute("TPN");

                                                if (!String.IsNullOrEmpty(id) || !String.IsNullOrEmpty(tpn))
                                                {
                                                    int idint = int.Parse(id);

                                                    TypePayment.Add(idint, new SimpleData { ID = idint, ClName = tpn });
                                                }
                                            }
                                        }
                                    }

                                    if (0 == String.CompareOrdinal(reader.Name, "Clients"))
                                    {
                                        XmlReader readerSubtree = reader.ReadSubtree();

                                        Clients = new Dictionary<object, object>();

                                        while (readerSubtree.Read())
                                        {
                                            if (0 == String.CompareOrdinal(readerSubtree.Name, "Cl"))
                                            {
                                                string clid = readerSubtree.GetAttribute("CLID");
                                                string cln = readerSubtree.GetAttribute("CLN");
                                                string clAddrr = readerSubtree.GetAttribute("CLAddrr");
                                                double clBlns = double.Parse(readerSubtree.GetAttribute("CLBlns"), new CultureInfo("en-US"));
                                                string clTel = readerSubtree.GetAttribute("CLTel");


                                                if (!String.IsNullOrEmpty(clid) || !String.IsNullOrEmpty(cln))
                                                {
                                                    var tmpcc = new ClientsClass {IDClient = int.Parse(clid),
                                                                                  ClientName = cln,
                                                                                  Address = clAddrr,
                                                                                  Balance = clBlns,
                                                                                  Telephone = clTel
                                                    };

                                                    XmlReader readerCp = readerSubtree.ReadSubtree();

                                                    while (readerCp.Read())
                                                    {
                                                        if (0 == String.CompareOrdinal(readerSubtree.Name, "CP"))
                                                        {
                                                            string cpid = readerSubtree.GetAttribute("CPID");
                                                            string cpNm = readerSubtree.GetAttribute("CPNm");
                                                            string cpAddrr = readerSubtree.GetAttribute("CPAddrr");
                                                            string cpTel = readerSubtree.GetAttribute("CPTel");

                                                            if (!String.IsNullOrEmpty(cpid) || !String.IsNullOrEmpty(cpNm))
                                                            {
                                                                tmpcc.PCl.Add(new ClientsPointsClass { ID = int.Parse(cpid),
                                                                                                       Name = cpNm,
                                                                                                       Address = cpAddrr,
                                                                                                       Tel = cpTel
                                                                });
                                                            }
                                                        }
                                                    }

                                                    Clients.Add(int.Parse(clid), tmpcc);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        IsNull = false;
                    }
                    else IsNull = true;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка загрузки файла!", MessageBoxButton.OK);
            }
        }
    }
}