using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitCalc.Models
{
    public class Tip
    {
        public int Id { get; set; }

        public string Categoria { get; set; } = string.Empty;

        public string Consejo { get; set; } = string.Empty;
    }
}
