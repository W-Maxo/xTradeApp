using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace xTrade
{
    public static class NewReqListClass
    {
        public static IEnumerable<ReqClass> GetNewReqList(InfData inf)
        {
            List<ReqClass> reqList = GetNewReqList();

            if (null != reqList)
            {
                foreach (var reqClass in reqList)
                {
                    if (reqClass != null) reqClass.FillToView(inf);
                }

                return reqList;
            }
            return null;
        }

        public static List<ReqClass> GetNewReqList()
        {
            using (var appStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (appStorage.FileExists("NewReq.xml"))
                {
                    var serializer = new XmlSerializer(typeof (XmlReqData));

                    XmlReqData reqData;
                    using (var file = appStorage.OpenFile("NewReq.xml", FileMode.Open))
                    {
                        using (var reader = XmlReader.Create(file))
                        {
                            reqData = (XmlReqData) serializer.Deserialize(reader);
                        }

                        file.Close();
                    }

                    return reqData.ReqList;
                }
                return  null;
            }
        }

        public static bool RemoveExisting(IEnumerable<ReqClass> list)
        {
            try
            {
                var reqData = new XmlReqData {ReqList = GetNewReqList()};

                var reqDataHt = reqData.ReqList.ToDictionary<ReqClass, object, object>(rd => rd.UnqStr.ToLower(), rd => rd);

                if (null != reqData.ReqList)
                {
                    foreach (var reqClass in list)
                    {
                        if (reqDataHt.ContainsKey(reqClass.UnqStr.ToLower()))
                        {
                            reqData.ReqList.Remove((ReqClass)reqDataHt[reqClass.UnqStr.ToLower()]);
                        }
                    }

                    Save(reqData);
                }
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public static bool AddItemToNewReqList(ReqClass item)
        {
            try
            {
                var reqData = new XmlReqData {ReqList = GetNewReqList() ?? new List<ReqClass>()};
                reqData.ReqList.Add(item);

                Save(reqData);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DeleteReq(string unqStr)
        {
            try
            {
                var comparer = StringComparer.OrdinalIgnoreCase;

                var reqData = new XmlReqData { ReqList = GetNewReqList() };

                foreach (var ri in reqData.ReqList)
                {
                    if (0 == comparer.Compare(ri.UnqStr, unqStr))
                    {
                        reqData.ReqList.Remove(ri);
                        break;
                    }
                }

                Save(reqData);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool UpdateItemInNewReqList(ReqClass item)
        {
            try
            {
                var comparer = StringComparer.OrdinalIgnoreCase;

                var reqData = new XmlReqData { ReqList = GetNewReqList() };

                foreach (var ri in reqData.ReqList)
                {
                    if (0 == comparer.Compare(ri.UnqStr, item.UnqStr))
                    {
                        reqData.ReqList.Remove(ri);
                        reqData.ReqList.Add(item);
                        break;
                    }
                }

                Save(reqData);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void Save(XmlReqData reqData)
        {
            using (var appStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var serializer = new XmlSerializer(typeof(XmlReqData));

                if (appStorage.FileExists("NewReq.xml"))
                    appStorage.DeleteFile("NewReq.xml");

                using (var file = appStorage.OpenFile("NewReq.xml", FileMode.CreateNew))
                {
                    TextWriter writer = new StreamWriter(file);

                    serializer.Serialize(writer, reqData);

                    writer.Close();
                    file.Close();
                }
            }
        }
    }
}
