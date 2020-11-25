using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;

namespace SOFTTEK.SCMS.SRA.Controllers
{
    public class MaintenanceController : BaseApiController
    {
        [ActionName("Schedule")]
        public Task<IHttpActionResult> GetDeviceAvailableWorkOrders()
        {

            try
            {
                IHttpActionResult result = Json<string>(string.Empty);

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                IHttpActionResult error = Error(ex);
                return Task.FromResult(error);
            }
        }

        [ActionName("Schedule")]
        public Task<IHttpActionResult> PostAttendedWorkOrder([FromBody] object workOrder)
        {

            try
            {
                IHttpActionResult result = Json<string>(string.Empty);

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                IHttpActionResult error = Error(ex);
                return Task.FromResult(error);
            }
        }

        [ActionName("Schedule")]

        public Task<IHttpActionResult> PutAttendedWorkOrder(string id, [FromBody] object workOrder)
        {

            try
            {
                IHttpActionResult result = Json<string>(string.Empty);

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                IHttpActionResult error = Error(ex);
                return Task.FromResult(error);
            }
        }

        [ActionName("Advice")]

        public Task<IHttpActionResult> GetWorkOrderAdvices(string id)
        {

            try
            {
                IHttpActionResult result = Json<string>(string.Empty);

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                IHttpActionResult error = Error(ex);
                return Task.FromResult(error);
            }
        }

        [ActionName("Advice")]

        public Task<IHttpActionResult> PostWorkOrderAdvice(string id, [FromBody] object advice)
        {

            try
            {
                IHttpActionResult result = Json<string>(string.Empty);

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                IHttpActionResult error = Error(ex);
                return Task.FromResult(error);
            }
        }

        [ActionName("Advice")]

        public Task<IHttpActionResult> PutAdvice(string id, [FromBody] object advice)
        {
            try
            {

                IHttpActionResult result = Json<string>(string.Empty);

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                IHttpActionResult error = Error(ex);
                return Task.FromResult(error);
            }
        }

        [ActionName("Meassurement")]

        public Task<IHttpActionResult> GetWorkOrderMeassurements(string id)
        {
            try
            {

                IHttpActionResult result = Json<string>(string.Empty);

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                IHttpActionResult error = Error(ex);
                return Task.FromResult(error);
            }
        }

        [ActionName("Meassurement")]

        public Task<IHttpActionResult> PostWorkOrderMeassurement(string id, [FromBody] object meassurement)
        {

            try
            {
                IHttpActionResult result = Json<string>(string.Empty);

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                IHttpActionResult error = Error(ex);
                return Task.FromResult(error);
            }
        }

        [ActionName("Meassurement")]

        public Task<IHttpActionResult> PutMeassurement(string id, [FromBody] object meassurement)
        {

            try
            {
                IHttpActionResult result = Json<string>(string.Empty);

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                IHttpActionResult error = Error(ex);
                return Task.FromResult(error);
            }
        }

        [ActionName("Replacement")]

        public Task<IHttpActionResult> GetWorkOrderReplacements(string id)
        {

            try
            {
                IHttpActionResult result = Json<string>(string.Empty);

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                IHttpActionResult error = Error(ex);
                return Task.FromResult(error);
            }
        }

        [ActionName("Replacement")]

        public Task<IHttpActionResult> PostWorkOrderReplacement(string id, [FromBody] object replacement)
        {

            try
            {
                IHttpActionResult result = Json<string>(string.Empty);

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                IHttpActionResult error = Error(ex);
                return Task.FromResult(error);
            }
        }

        [ActionName("Replacement")]

        public Task<IHttpActionResult> PutReplacement(string id, [FromBody] object replacement)
        {

            try
            {
                IHttpActionResult result = Json<string>(string.Empty);

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                IHttpActionResult error = Error(ex);
                return Task.FromResult(error);
            }
        }

        [ActionName("Access")]
        public Task<IHttpActionResult> GetDeviceUsers()
        {

            try
            {
                IHttpActionResult result = Json<string>(string.Empty);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                IHttpActionResult error = Error(ex);
                return Task.FromResult(error);
            }
        }

    }
}
