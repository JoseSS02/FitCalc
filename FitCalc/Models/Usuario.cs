using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitCalc.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public int? CaloriasNecesarias { get; set; }
        public int? ProteinasNecesarias { get; set; }
        public int? GrasasNecesarias { get; set; }
        public int? HidratosNecesarios { get; set; }

    }
}
