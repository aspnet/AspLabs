using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HelloMVC.Models
{
    public class ItemContext : DbContext
    {
        public ItemContext(DbContextOptions<ItemContext> options)
            : base(options)
        { }

        public DbSet<Item> Items { get; set; }
    }

    public class Item
    {
        public int ItemId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
