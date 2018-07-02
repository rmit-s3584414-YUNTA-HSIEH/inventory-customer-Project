
using OrderHistoryWebApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderHistoryWebApi.Models.DataManager
{
    


    public class DataManager
    {
        private readonly OrderHistoryContext _context;


        public DataManager(OrderHistoryContext context)
        {
            _context = context;
        }

        public IEnumerable<OrderItem> GetAll()
        {
            return _context.OrderItems.ToList();
        }

        //get all order item from the same customer
        public IEnumerable<OrderItem> Get(String CustomerName)
        {
            var orderID = new List<int>();
            var orders = _context.Orders.Where(x => x.CustomerName == CustomerName).ToList();

            foreach(var o in orders)
            {
                orderID.Add(o.OrderID);
            }

            var orderItems = _context.OrderItems.ToList();
            foreach(var item in orderItems)
            {
                if(!orderID.Contains(item.OrderID))
                {
                    orderItems.Remove(item);
                }
            }
            return orderItems;
        }


    }
}
