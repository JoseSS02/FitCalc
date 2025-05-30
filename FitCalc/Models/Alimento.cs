using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitCalc.Models
{
    public class Alimento
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public float Kcal { get; set; }
        public float Grasas { get; set; }
        public float Hidratos { get; set; }
        public float Proteinas { get; set; }
    }
}

