using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace Funda.Web.Models
{

    public class RetrieveData : IDisposable
    {
        private string _key;// = "ac1b0b1572524640a0ecc54de453ea9f";
        private string _baseUrl;
        private string _dataType;
        private int _page, _pageSize;
        private int _delay;
        public RequestStatus Status { get;set;}
        private string[] _parameters;       
        private HttpClient client;
        public RetrieveData(string baseUrl, string dataType, string key,int pageSize)
        {
            _dataType = dataType;
            _baseUrl = baseUrl;
            _key = key;
            _delay = 600;
            _pageSize = pageSize;
            Status = new RequestStatus();
            client = new HttpClient(){ BaseAddress = new Uri(_baseUrl)};
        }
        public void SetDelay(int delay) { _delay = delay; }
        public void SetKey(string key)  { _key = key; }        
        public void SetPage(int page)   { _page = page;  }
        public void SetParams(params string[] parameters) { _parameters = parameters; }
        public string BaseUrl { get { return _baseUrl; } }
        public String[] Parameters { get{ return _parameters; } }
        public override string ToString()
        {
            StringBuilder addr = new StringBuilder();
          
            if (_dataType.ToLower() == "json") addr.Append("json/");
            addr.Append(_key).Append("/").Append("?type=koop").Append("&zo=");            
            foreach (string s in _parameters) if(s.Length>0) addr.Append("/").Append(s);            
            addr.Append($"/&page={_page}&pagesize={_pageSize}");
            return addr.ToString();
        }
        public  string GetData()
        {
            if(!ISValidParameters())
            {
                Status.set(HttpStatusCode.BadRequest, " Invalid initial parameters...");return "";
            }
            Thread.Sleep(_delay);
            int repeatRequest =1;
            var resultStr = "";            
            Send_request:
            var responseTask = client.GetAsync(this.ToString());
            try
            {
                responseTask.Wait();
            }
            catch(Exception ex)
            {
                Status.set(HttpStatusCode.BadGateway, ex.Message); return "";               
            }                        
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readTask = result.Content.ReadAsStringAsync();
                readTask.Wait();                
                resultStr = readTask.Result;
                Status.set(result.StatusCode, "OK!");                
            }
            else
            {                
                Status.set(result.StatusCode, "Error Retrieving data from server...");
                int statusCode = (int)result.StatusCode;                
                if (statusCode / 100 == 5 ) // All server side errros
                {
                    if (repeatRequest < 4)
                    {
                        Thread.Sleep(_delay * (int)(Math.Pow(2, repeatRequest++)));
                        goto Send_request;
                    }
                    return "";
                }
                if(result.StatusCode==(HttpStatusCode)429 && repeatRequest++<10)
                {
                    Thread.Sleep(1000);goto Send_request;
                }
            }                
                return resultStr;                                       
        }
        private bool ISValidParameters()
        {
            return !( string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_key) || _parameters.Length == 0 || _pageSize==0);
        }
        public void Dispose()
        {
            client.Dispose();
        }
    }
}