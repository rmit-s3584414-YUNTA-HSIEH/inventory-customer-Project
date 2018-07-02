using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ass2WithOAuth.Models;
using Microsoft.EntityFrameworkCore;
using Ass2WithOAuth.Data;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ass2WithOAuth.Controllers
{
    [Authorize(Roles =Constants.OwnerRole)]
    public class OwnerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OwnerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(OwnerInventory));
        }
      
        public async Task<IActionResult> OwnerInventory(string productName)
        {
            // Eager loading the Product table - join between OwnerInventory and the Product table.
            var query = _context.OwnerInventory.Include(x => x.Product).Select(x => x);

            if (!string.IsNullOrWhiteSpace(productName))
            {
                // Adding a where to the query to filter the data.
                // Note for the first request productName is null thus the where is not always added.
                query = query.Where(x => x.Product.Name.Contains(productName));

                // Storing the search into ViewBag to populate the textbox with the same value for convenience.
                ViewBag.ProductName = productName;
            }

            // Adding an order by to the query for the Product ID.
            query = query.OrderBy(x => x.Product.ProductID);

            // Passing a List<OwnerInventory> model object to the View.
            return View(await query.ToListAsync());
        }
        public async Task<IActionResult> SetOwnerStock(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Ownerproduct = await _context.OwnerInventory.Include(x=>x.Product).SingleOrDefaultAsync(m => m.ProductID == id);
            if (Ownerproduct == null)
            {
                return NotFound();
            }
            return View(Ownerproduct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetOwnerStock(int id, [Bind("ProductID,StockLevel")] OwnerInventory OwnerInventory)
        {
            if (id != OwnerInventory.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(OwnerInventory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {

                }
                return RedirectToAction(nameof(OwnerInventory));
            }
            OwnerInventory.Product = await _context.Products.SingleOrDefaultAsync(m => m.ProductID == id);
            return View(OwnerInventory);
        }



        public async Task<IActionResult> StockRequest()
        {
           
            var request = _context.StockRequests.Include(x => x.Product).Select(x => x);
            request = request.Include(y => y.Store).Select(y => y);
            //adding an order by id
            request = request.OrderBy(x => x.StockRequestID);

            var ownerStock = _context.OwnerInventory.ToList();
            ViewBag.ownerStock = ownerStock;

            return View(await request.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StockRequest(int StockRequestID)
        {
            //get the request via stock request ID
            var request = await _context.StockRequests.SingleOrDefaultAsync(r => r.StockRequestID == StockRequestID);
            //get woner inventory with specific product id for later comparison
            var ownerStock = await _context.OwnerInventory.SingleAsync(o => o.ProductID == request.ProductID);

            var storeInventory = await _context.StoreInventory.FindAsync(request.StoreID,request.ProductID);
            
            //update data to database
            //store already has the product
            if (storeInventory!=null)
            {
                storeInventory.StockLevel += request.Quantity;
                ownerStock.StockLevel -= request.Quantity;
                try
                {
                    _context.StoreInventory.Update(storeInventory);
                    _context.OwnerInventory.Update(ownerStock);
                    _context.StockRequests.Remove(request);
                }
                catch(DbUpdateConcurrencyException)
                {
                }  
                await _context.SaveChangesAsync();
            }
            //add new the new product into store
            if (storeInventory == null)
            {
                StoreInventory newProductItem = new StoreInventory();
                newProductItem.StoreID = request.StoreID;
                newProductItem.ProductID = request.ProductID;
                newProductItem.StockLevel = request.Quantity;
                ownerStock.StockLevel -= request.Quantity;
                try
                {
                    _context.StoreInventory.Add(newProductItem);
                    _context.OwnerInventory.Update(ownerStock);
                    _context.StockRequests.Remove(request);
                }
                catch (DbUpdateConcurrencyException)
                {
                }
                await _context.SaveChangesAsync();
            } 
            return RedirectToAction(nameof(StockRequest));
        }

    }
}

