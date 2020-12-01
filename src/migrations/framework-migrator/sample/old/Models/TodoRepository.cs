using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TodoMvc.Data
{
    public class TodoRepository
    {
        public static List<Models.Todo> Todos = new List<Models.Todo>
        {
           new Models.Todo
           {
               Id = MaxId++,
               Title = "Brush teeth",
           },
           new Models.Todo
           {
               Id = MaxId++,
               Title = "Comb hair",
           },
           new Models.Todo
           {
               Id = MaxId++,
               Title = "Eat breakfast",
           },
        };

        public static int MaxId = 0;

        public IEnumerable<Models.Todo> GetAll()
        {
            return Todos;
        }

        public Models.Todo Get(int id)
        {
            return Todos.Where(t => t.Id == id).FirstOrDefault();
        }

        public Models.Todo Save(Models.Todo item)
        {
            if (item.Id == 0)
            {
                item.Id = ++MaxId;
            }

            int index = Todos.IndexOf(item);
            if (index != -1)
            {
                Todos[index] = item;
            }
            else
            {
                Todos.Add(item);
            }

            return item;
        }

        public void Delete()
        {
            Todos.Clear();
        }

        public void Delete(int id)
        {
            Todos.RemoveAll(t => t.Id == id);
        }
    }
}