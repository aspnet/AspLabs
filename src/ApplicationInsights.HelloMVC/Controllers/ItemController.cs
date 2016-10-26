using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloMVC.Models;
using Microsoft.AspNetCore.Mvc;
using static HelloMVC.Models.ItemContext;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace HelloMVC.Controllers
{
    public class ItemsController : Controller
    {
        private ItemContext _context;
        public ItemsController(ItemContext context)
        {
            _context = context;
            _context.Database.EnsureCreated();
        }

        public IActionResult Index()
        {
            return View(_context.Items.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(item);
        }
    }
}
