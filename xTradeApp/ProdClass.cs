using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Xml;

namespace xTrade
{
    public class BProdClass
    {
        public int TvID { get; set; }
        public int CodeTv { get; set; }
        public int TypeID { get; set; }
        public string Name { get; set; }
        public int NimP { get; set; }
        public List<double> CostP { get; private set; }
        public int Remains { get; set; }

        public BProdClass()
        {
            CostP = new List<double>();
        }
    }

    public class ProdClass
    {
        private Dictionary<object, object> _listProd;

        public void LoadLinqTovars()
        {
            try
            {
                _listProd = new Dictionary<object, object>();

                using (var appStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (appStorage.FileExists("Tovars.xml"))
                    {
                        using (var file = appStorage.OpenFile("Tovars.xml", FileMode.Open))
                        {
                            using (var reader = XmlReader.Create(file))
                            {
                                while (reader.Read())
                                {
                                    if (0 != String.CompareOrdinal(reader.Name, "Gr")) continue;
                                    var nameAttr = reader.GetAttribute("GrNm");

                                    var tpreader = reader.ReadSubtree();

                                    while (tpreader.Read())
                                    {
                                        if (0 != String.CompareOrdinal(reader.Name, "Tp")) continue;

                                        if (String.IsNullOrEmpty(nameAttr)) continue;
                                        var readerSubtree = tpreader.ReadSubtree();

                                        while (readerSubtree.Read())
                                        {
                                            if (readerSubtree.Name != "Item") continue;

                                            var tmpprod = new BProdClass();

                                            var tmp = readerSubtree.GetAttribute("Desc");
                                            tmpprod.Name = tmp;

                                            tmp = readerSubtree.GetAttribute("Code");
                                            if (tmp != null) tmpprod.CodeTv = int.Parse(tmp);

                                            tmp = readerSubtree.GetAttribute("Rem");
                                            if (tmp != null) tmpprod.Remains = int.Parse(tmp);

                                            tmp = readerSubtree.GetAttribute("CntinP");
                                            if (tmp != null) tmpprod.NimP = int.Parse(tmp);

                                            tmp = readerSubtree.GetAttribute("TvID");
                                            if (tmp != null) tmpprod.TvID = int.Parse(tmp);

                                            _listProd.Add(tmpprod.TvID, tmpprod);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка загрузки файла!", MessageBoxButton.OK);
            }
        }

        public string GetProdNameByID(int ID)
        {
            if (_listProd.ContainsKey(ID))
            {
                var tmpprod = (BProdClass) _listProd[ID];
                return tmpprod.Name;
            }
            return "";
        }
    }
}
