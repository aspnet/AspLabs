using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;

namespace TodoMvc.Controllers
{
    public class TodoController : ApiController
    {
        private readonly Data.TodoRepository repo;

        public TodoController()
        {
            repo = new Data.TodoRepository();
        }

        // GET: Todo
        [HttpGet]
        public IEnumerable<Models.Todo> Get()
        {
            var items = repo.GetAll();

            foreach (var t in items)
            {
                t.Url = String.Format("{0}/{1}", Request.RequestUri.ToString(), t.Id.ToString());
            }

            return items;
        }

        // GET: Todo/5
        public Models.Todo Get(int id)
        {
            var t = repo.Get(id);
            if (t != null)
            {
                t.Url = Request.RequestUri.ToString();
            }
            return t;
        }

        // POST: Todo
        public HttpResponseMessage Post(Models.Todo item)
        {
            var t = repo.Save(item);
            t.Url = String.Format("{0}/{1}", Request.RequestUri.ToString(), t.Id.ToString());

            var response = Request.CreateResponse<Models.Todo>(HttpStatusCode.OK, t);
            response.Headers.Location = new Uri(Request.RequestUri, "todo/" + t.Id.ToString());

            return response;
        }

        // PATCH: Todo/5
        public HttpResponseMessage Patch(int id, Models.Todo item)
        {
            item.Id = id;
            var t = repo.Save(item);
            t.Url = Request.RequestUri.ToString();

            var response = Request.CreateResponse<Models.Todo>(HttpStatusCode.OK, t);
            response.Headers.Location = new Uri(Request.RequestUri, t.Id.ToString());

            return response;
        }

        // DELETE: Todo
        public void Delete()
        {
            repo.Delete();
        }

        // DELETE: Todo/5
        public void Delete(int id)
        {
            repo.Delete(id);
        }
    }
}
