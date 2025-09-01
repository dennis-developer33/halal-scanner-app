using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalalScanner.Models
{
    public class HaramIngredient
    {
        public string Name { get; set; }
        public string Reason { get; set; }
        public List<string> AlternativeNames { get; set; } = new List<string>();
    }

    public class HalalCheckResult
    {
        public bool IsHalal { get; set; }
        public List<HaramIngredient> FoundHaramIngredients { get; set; } = new List<HaramIngredient>();
        public string Summary { get; set; }
    }
}
