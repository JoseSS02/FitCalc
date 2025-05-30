using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitCalc.Models
{
    public class Macronutrientes
    {
        public string Usuario { get; set; } = string.Empty;
        public string Calorias { get; set; } = string.Empty;
        public float Grasas { get; set; }
        public float Hidratos { get; set; }
        public float Proteinas { get; set; }
    }
}
