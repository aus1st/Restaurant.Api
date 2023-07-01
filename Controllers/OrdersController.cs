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
    public class OrdersController : ApiController
    {
        private DBModel db = new DBModel();

        // GET: api/Orders
        public async Task<IHttpActionResult> GetOrders()
        {
            var result = (from b in db.Orders
                          join c in db.Customers on b.CustomerId equals c.CustomerId
                          select new
                          {
                              b.OrderId,
                              b.OrderNo,
                              Customer = c.CustomerName,
                              b.PMethod,
                              b.GTotal
                          }).ToListAsync();

            return Ok(await result);
            //return db.Orders;
        }

        // GET: api/Orders/5
        [ResponseType(typeof(Order))]
        public async Task<IHttpActionResult> GetOrder(int id)
        {
            Order formData = await db.Orders.FindAsync(id);
            if (formData == null)
            {
                return NotFound();
            }

            var orderItems = await (from o in db.OrderItems
                                   join i in db.Items on o.ItemId equals i.ItemId
                                    where o.OrderId == id
                                    select new
                                    {
                                  o.OrderId,
                                  o.OrderItemId,
                                  i.ItemId,
                                  i.ItemName,
                                  o.Qty,
                                  Price = i.price,
                                  Total = o.Qty * i.price
                              }).ToListAsync();


            return Ok(new {formData, orderItems });
        }

        // PUT: api/Orders/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutOrder(int id, Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != order.OrderId)
            {
                return BadRequest();
            }

            db.Entry(order).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Orders
        [ResponseType(typeof(Order))]
        public async Task<IHttpActionResult> PostOrder(Order order)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (order.OrderId == 0)
                {
                    db.Orders.Add(order);
                }
                else
                {
                    var missingId = order.OrderItems.Where(x => x.OrderId == 0).ToList();
                    missingId.ForEach(m =>
                    {
                        m.OrderId = order.OrderId;
                    });
                    db.Entry(order).State = EntityState.Modified;
                }


                foreach (var item in order.OrderItems)
                {
                    if (item.OrderItemId == 0)
                        db.OrderItems.Add(item);
                    else
                        db.Entry(item).State = EntityState.Modified;
                }

                //delete items
                if (order.DeletedEntries != null)
                {
                    foreach (var d in order.DeletedEntries.Split(',').Where(x => x != ""))
                    {
                        OrderItem oI = await db.OrderItems.FindAsync(Convert.ToInt32(d));
                        if (oI != null)
                        {
                            db.OrderItems.Remove(oI);
                        }
                    }

                }

                await db.SaveChangesAsync();

                return CreatedAtRoute("DefaultApi", new { id = order.OrderId }, order);
            }
            catch (Exception ex)
            {

                return InternalServerError(ex);
            }
        }

        // DELETE: api/Orders/5
        [ResponseType(typeof(Order))]
        public async Task<IHttpActionResult> DeleteOrder(int id)
        {
            Order order = await db.Orders.Include(i=>i.OrderItems)
                .SingleOrDefaultAsync(o=>o.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            foreach (var item in order.OrderItems.ToList())
            {
                db.OrderItems.Remove(item);
            }


            db.Orders.Remove(order);
            await db.SaveChangesAsync();

            return Ok(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrderExists(int id)
        {
            return db.Orders.Count(e => e.OrderId == id) > 0;
        }
    }
}