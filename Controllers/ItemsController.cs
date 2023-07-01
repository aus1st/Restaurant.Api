using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Restaurant.Api.Models;

namespace Restaurant.Api.Controllers
{
    public class ItemsController : ApiController
    {
        private DBModel db = new DBModel();

        // GET: api/Items
        public IQueryable<Item> GetItems()
        {
            return db.Items;
        }




        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


    }
}