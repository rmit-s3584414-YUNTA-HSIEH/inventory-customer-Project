using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ass2WithOAuth.Models;
using Microsoft.EntityFrameworkCore;
using Ass2WithOAuth.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using OrderHistory.Web.Helper;
using System;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ass2WithOAuth.Controllers
{
    public class CustomerController : Controller
    {

        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return RedirectToAction(nameof(OrderHistory));
        }



        public async Task<IActionResult> TheStoreInventory(int?id, string productName)
        {
            if (id == null||id>5)
            {
                return NotFound();
            }

            var query = _context.StoreInventory.Include(x => x.Store).Select(x => x);

            if (!string.IsNullOrWhiteSpace(productName))
            {
                // Adding a where to the query to filter the data.
                // Note for the first request productName is null thus the where is not always added.
                query = query.Where(x => x.Product.Name.Contains(productName));

                // Storing the search into ViewBag to populate the textbox with the same value for convenience.
                ViewBag.ProductName = productName;
            }
            
            query = query.Include(y => y.Product).Select(y => y);
            query = query.Where(x => x.StoreID == id);

            return View(await query.ToListAsync());
        }      

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int? quantity, int ProductID, int StoreID)
        {
            if (ProductID == 0 || StoreID == 0)
            {
                return NotFound();
            }

            var storeInventory = await _context.StoreInventory.Include(x => x.Store).Include(y => y.Product).Where(s => s.StoreID == StoreID).SingleOrDefaultAsync(p => p.ProductID == ProductID);
            if (quantity != null&& quantity != 0)
            {
                int key = 1;
                //get session keys
                var SessionKeys = HttpContext.Session.Keys;
                //generate session key
                while(SessionKeys.Contains(key.ToString()))
                {
                    key++;
                }
                //for checking session has contained same order item or not
                var checkCount = 0;

                OrderItem Orderitem = new OrderItem();
                //store new order item in session
                if (SessionKeys.Count()==0)
                {                    
                    Orderitem.ProductID = ProductID;
                    Orderitem.Product = storeInventory.Product;
                    Orderitem.Quantity = (int)quantity;
                    Orderitem.StoreID = StoreID;
                    HttpContext.Session.Set<OrderItem>(key.ToString(), Orderitem);

                    return RedirectToAction("TheStoreInventory", new { id = StoreID });
                }
                else
                {
                    foreach(var i in SessionKeys)
                    {

                        Orderitem = HttpContext.Session.Get<OrderItem>(i);
                        //update order item
                        if (Orderitem.StoreID ==StoreID && Orderitem.ProductID ==ProductID)
                        {
                            Orderitem.Quantity += (int)quantity;
                            HttpContext.Session.Set<OrderItem>(i, Orderitem);
                            break;
                        }
                        checkCount++;
                    }
                    //add new order item with same store
                    if(checkCount== SessionKeys.Count())
                    {
                        Orderitem.ProductID = ProductID;
                        Orderitem.Product = storeInventory.Product;
                        Orderitem.Quantity = (int)quantity;
                        Orderitem.StoreID = StoreID;
                        HttpContext.Session.Set<OrderItem>(key.ToString(), Orderitem);

                    }
                    return RedirectToAction("TheStoreInventory", new { id = StoreID });
                }
            }
            return View(storeInventory);
        }

        //remove the order item
        public IActionResult Remove(string id)
        {
            var keys = HttpContext.Session.Keys;

            if (id == null||!keys.Contains(id))
            {
                return NotFound();
            }
            HttpContext.Session.Remove(id);

            return RedirectToAction(nameof(Cart));
        }

        public IActionResult Cart()
        {

            return View();
        }

        public IActionResult CardValidation()
        {
            return View();
        }

        //after card validation, order will be stored in db and diplay for customer
        public async Task<IActionResult> OrderDetail()
        {
            var SessionKeys = HttpContext.Session.Keys;

            if (SessionKeys.Count()==0)
            {
                return NotFound();
            }

            Order Order = new Order();
            Order.CustomerName = User.Identity.Name;
            //add new order into db
            try
            {
                _context.Orders.Add(Order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {

            }            
            var orders = _context.Orders.ToList();
            var ordersID = new List<int>();
            //add order id into list for checking which order don't have any order items
            foreach(var o in orders)
            {
                ordersID.Add(o.OrderID);
            }

            var orderItems = _context.OrderItems.ToList();
            int emptyOrderID=0;
            //find the order that don't have any order items
            if(orderItems.Count()!=0)
            {
                foreach(var item in orderItems)
                {
                    if (ordersID.Contains(item.OrderID))
                    {
                        ordersID.Remove(item.OrderID);
                    }
                }
                emptyOrderID = ordersID.First();
            }
            else
            {
                emptyOrderID = ordersID.First();
            }

            OrderItem Orderitem;
            // add order items into db
            foreach (var i in SessionKeys)
            {
                Orderitem = HttpContext.Session.Get<OrderItem>(i);
                Orderitem.OrderID = emptyOrderID;
                Orderitem.Product= null;
                var storeInventory = _context.StoreInventory.Where(x => x.StoreID == Orderitem.StoreID).Where(y => y.ProductID == Orderitem.ProductID).SingleOrDefault();
                storeInventory.StockLevel -= Orderitem.Quantity;
                try
                { 
                    _context.OrderItems.Add(Orderitem);
                    _context.StoreInventory.Update(storeInventory);
                }
                catch (DbUpdateConcurrencyException)
                {

                }
            }
            //save to db
            await _context.SaveChangesAsync();

            //clear session
            HttpContext.Session.Clear();
            var orderDetail = _context.OrderItems.Include(s=>s.Store).Include(p=>p.Product).Where(x => x.OrderID == emptyOrderID).ToList();

            return View(orderDetail);
        }

        //get the cusotmer's order items by calling web api
        public async Task<IActionResult> OrderHistory()
        {
            //web api 
            var response = await OrderHistoryApi.InitializeClient().GetAsync($"api/OrderHistory/{User.Identity.Name}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            // Storing the response details recieved from web api.
            var result = response.Content.ReadAsStringAsync().Result;

            // Deserializing the response recieved from web api and storing into a list.
            var items = JsonConvert.DeserializeObject<List<OrderItem>>(result);

            foreach(var i in items)
            {
                var store = await _context.Stores.SingleAsync(x => x.StoreID == i.StoreID);
                var product = await _context.Products.SingleAsync(x => x.ProductID == i.ProductID);
                i.Store = store;
                i.Product =product;
            }
            items=items.OrderBy(x => x.OrderID).ToList();

            return View(items);
        }
    }
    //helper calss for storing object into session
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) :
                                  JsonConvert.DeserializeObject<T>(value);
        }
    }
}
