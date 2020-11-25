using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TodoMvc.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public int? Order { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public bool Completed { get; set; }

        public override bool Equals(object obj)
        {
           var todo = obj as Todo;
           return (todo != null) && (Id == todo.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}