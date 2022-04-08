using System.ComponentModel.DataAnnotations;

namespace MvcApp.Models
{
    public class SessionDemoModel
    {
        public const string IntSessionItemName = "IntSessionItem";
        public const string StringSessionItemName = "StringSessionItem";

        [Display(Name = "Integer session item")]
        public int? IntSessionItem { get; set; }

        [Display(Name = "String session item")]
        public string StringSessionItem { get; set; }
    }

}
