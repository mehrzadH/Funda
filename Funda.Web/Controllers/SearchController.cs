using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using System.Net.Http;
using Funda.Web.Models;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Threading;

namespace Funda.Web.Controllers
{
    public class SearchController : Controller
    {
        
        public ActionResult Index(bool tuin=false)
        {

           
            Dictionary<string, int> makelaars = new Dictionary<string, int>();
           
            using (RequestService requestSvc = new RequestService(@"http://partnerapi.funda.nl/feeds/Aanbod.svc/", "json", "ac1b0b1572524640a0ecc54de453ea9f", 25))
            {
                ViewBag.Error = requestSvc.ProcessData("amsterdam",tuin? "tuin":"");                
                makelaars = requestSvc.Makelaars;
                ViewBag.properties = makelaars.Sum(x => x.Value);
            }
            
            return View(makelaars.OrderByDescending(x => x.Value).Take(10).ToDictionary(x=>x.Key,x=>x.Value));
        }

       
    }
       
        
}
