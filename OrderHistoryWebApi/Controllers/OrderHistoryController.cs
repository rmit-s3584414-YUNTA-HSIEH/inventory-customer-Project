using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrderHistoryWebApi.Models;
using OrderHistoryWebApi.Models.DataManager;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrderHistoryWebApi.Controllers
{
    [Route("api/[controller]")]
    public class OrderHistoryController : Controller
    {

        private readonly DataManager _repo;

        public OrderHistoryController(DataManager repo)
        {
            _repo = repo;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<OrderItem> Get()
        {
            return _repo.GetAll();
        }

        // GET api/<controller>/5
        [HttpGet("{customerName}")]
        public IEnumerable<OrderItem> Get(string customerName)
        {
            return _repo.Get(customerName);
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
