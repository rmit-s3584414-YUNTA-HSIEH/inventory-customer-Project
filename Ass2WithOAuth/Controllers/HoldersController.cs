using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ass2WithOAuth.Models;
using Microsoft.EntityFrameworkCore;
using Ass2WithOAuth.Data;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ass2WithOAuth.Controllers
{
    [Authorize(Roles =Constants.CBDHolderRole+","+Constants.EastHolderRole + "," + Constants.WestHolderRole 
        + "," + Constants.SouthHolderRole + "," + Constants.NorthHolderRole)]
    public class HoldersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HoldersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return RedirectToAction(nameof(StoreInventory)); ;
        }


        public async Task<IActionResult> StoreInventory()
        {
            var StoreID = CheckStoreID();
            var query = _context.StoreInventory.Include(x => x.Store).Select(x => x);
            query = query.Include(y => y.Product).Select(y => y);
            query = query.Where(x => x.StoreID == StoreID);
          
            return View(await query.ToListAsync());
        }

        //display the prodcut that already in store and let holder set a new stock level
        public async Task <IActionResult> NewRequest(int?id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var StoreID = CheckStoreID();
            var Storeproduct = await _context.StoreInventory.Where(i=>i.StoreID==StoreID).Include(x => x.Product).Include(s=>s.Store).Select(y=>y).SingleOrDefaultAsync(p=>p.ProductID==id);
            


            if (Storeproduct == null)
            {
                return NotFound();
            }
            return View(Storeproduct);
        }

        //request a new stock level with product already in store
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewRequest(int ProductID, [Bind("StoreID,ProductID,StockLevel")] StoreInventory StoreInventory)
        {
            if (ProductID != StoreInventory.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                var newRequest = new StockRequest();
                newRequest.ProductID = ProductID;
                newRequest.StoreID = StoreInventory.StoreID;
                newRequest.Quantity = StoreInventory.StockLevel;
                try
                {
                    _context.StockRequests.Add(newRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {

                }
                return RedirectToAction(nameof(StoreInventory));
            }
            StoreInventory.Product = await _context.Products.SingleOrDefaultAsync(m => m.ProductID == ProductID);
            StoreInventory.Store = await _context.Stores.SingleOrDefaultAsync(s => s.StoreID == StoreInventory.StoreID);
            return View(StoreInventory);
        }

        //display the list of products that the store don't have 
        public IActionResult NewItemRequest()
        {
            var NewProducts = new List<Product>();
            var storeID =  CheckStoreID();
            var AllProducts = _context.Products.ToList();


            //add all product id into stores set
            foreach (var items in AllProducts)
            {
                NewProducts.Add(items);
            }

            var storeInventroy =   _context.StoreInventory.Include(x => x.Product).Include(y => y.Store).Select(z => z).Where(x => x.StoreID == storeID).ToList();

            //find out the products not in store inventory
            if (storeInventroy != null)
            {
                foreach (var items in storeInventroy)
                {
                    if (NewProducts.Contains(items.Product))
                    {
                        NewProducts.Remove(items.Product);
                    }
                }
            }

            return View (NewProducts);
        }



        public IActionResult AddNewItem(int? id)
        {
            
            if (id == null||id>10)
            {
                return NotFound();
            }
            var StoreID = CheckStoreID();
            
            var NewRequest = new StockRequest();
            NewRequest.StoreID = StoreID;
            NewRequest.Product = _context.Products.Single(x => x.ProductID == id);
            NewRequest.ProductID = (int)id;
            NewRequest.Quantity = 0;
            return View(NewRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewItem(int id, [Bind("ProductID,Quantity,StoreID")] StockRequest StockRequest)
        {
            if (id != StockRequest.ProductID)
            {
                return NotFound();
            }
            var StoreID = CheckStoreID();
            StockRequest.StoreID = StoreID;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(StockRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {

                }
                return RedirectToAction(nameof(NewItemRequest));
            }
            StockRequest.Product = await _context.Products.SingleOrDefaultAsync(m => m.ProductID == id);
            return View(StockRequest);
        }

        //helper method
        private int CheckStoreID()
        {
            int storeID = 0;
            if (User.IsInRole(Constants.CBDHolderRole))
            {             
                 storeID = 1;
            }
            if (User.IsInRole(Constants.NorthHolderRole))
            {
                 storeID = 2;
            }
            if (User.IsInRole(Constants.EastHolderRole))
            {
                 storeID = 3;
            }
            if (User.IsInRole(Constants.SouthHolderRole))
            {
                storeID = 4;
            }
            if (User.IsInRole(Constants.WestHolderRole))
            {
                storeID = 5;
            }
            return storeID;

        }

    }
}
