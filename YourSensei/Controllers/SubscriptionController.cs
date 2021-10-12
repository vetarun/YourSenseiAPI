using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.ApplicationServices;
using System.Web.Http;
using YourSensei.Data;
using YourSensei.Service;
using YourSensei.ViewModel;

namespace YourSensei.Controllers
{
    //[Authorize]
    [RoutePrefix("Subscription")]
    public class SubscriptionController : ApiController
    {
        private readonly ISubscriptionService _service;

        public SubscriptionController(SubscriptionService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("GetSubscriptionPlans")]
        public async Task<IHttpActionResult> GetSubscriptionPlans(int id)
        {
            var result = await _service.GetSubscriptionPlans(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetSubscriptions")]
        public async Task<IHttpActionResult> GetSubscriptions()
        {
            var result = await _service.GetSubscriptions();
            return Ok(result);
        }
        
        [HttpPost]
        [Route("AddUpdateSubscriptionPlans")]
        public async Task<IHttpActionResult> AddUpdateSubscriptionPlans(SubscriptionPlanViewModel obj)
        {
            var result = await _service.addUpdateSubscriptionPlan(obj);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetFeaturesAllowed")]
        public async Task<IHttpActionResult> GetFeaturesAllowed()
        {
            var result = await _service.GetFeaturesAllowed();
            return Ok(result);
        }

        [HttpPost]
        [Route("SubscribePlanToUser")]
        public async Task<IHttpActionResult> SubscribePlanToUser(string userdetailid, string companyid, int planID, int noOfDays)
        {
            var result = await _service.SubscribePlanToUser(userdetailid, companyid, planID, noOfDays);
            return Ok(result);
        }

        [HttpPost]
        [Route("SavePayPalTransaction")]
        public IHttpActionResult SavePayPalTransaction(SubscriptionTransaction obj)
        {
            var result =  _service.SavePayPalTransaction(obj);
            return Ok(result);
        }
    }
}
