using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;

namespace Funda.Web.Models
{
    public class RequestService : IDisposable
    {
        private Dictionary<string, int> makelaars = new Dictionary<string, int>();
        public Dictionary<string, int> Makelaars { get { return makelaars; } }
        private List<string> _data = new List<string>();
        public List<string> Data { get { return _data; } }

        private RetrieveData request;       
        public RequestService(string url,string dataType,string key,int pageSize)
        {
            request = new RetrieveData(url, dataType, key,pageSize);            
        }
        
        public RequestStatus  StoreData(params string[] parameters)
        {
            int TotalPages = 0;
            int currentPage = 1;
            string jsonStr;
            request.SetPage(currentPage++);
            request.SetParams(parameters);
            do
            {
                jsonStr = request.GetData();
                if (request.Status.errorCode != System.Net.HttpStatusCode.OK) return request.Status;
                _data.Add(jsonStr);
                if (TotalPages == 0)
                {
                    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                    dynamic dataObj = jsonSerializer.Deserialize<dynamic>(jsonStr);
                    TotalPages = (int)(dataObj["Paging"]["AantalPaginas"]);
                }
                request.SetPage(currentPage++);

            } while (currentPage <=  TotalPages);
            HttpContext.Current.Session["datalist"] = _data;
            HttpContext.Current.Session["parameters"] = request.Parameters;
            return request.Status;
        }

        public string ProcessData( params string[] parameters)
        {
            var data = (List<string>)HttpContext.Current.Session["datalist"];
            var param = (string[])HttpContext.Current.Session["parameters"];
            int i = 0;bool newParam = false;
            if (param != null)
            {
                foreach (string s in param)
                    if (s != parameters[i++])
                    {
                        newParam = true; break;
                    }
            }
            if (param == null || newParam)
            {
                var status = StoreData(parameters);
                if (status.errorCode != HttpStatusCode.OK) return $"{status.errorCode} : {status.message}";
                data = _data;
            }
            
            foreach (var str in data)
            {
                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                dynamic dataObj = jsonSerializer.Deserialize<dynamic>(str);
                foreach (var obj in dataObj["Objects"])
                {
                    string makelaarName = obj["MakelaarNaam"];
                    if (makelaars.ContainsKey(makelaarName))
                        makelaars[makelaarName]++;
                    else
                        makelaars.Add(makelaarName, 1);

                }
            }
            return "";
        }
        public void Dispose()
        {
            request.Dispose();
        }

    }
}