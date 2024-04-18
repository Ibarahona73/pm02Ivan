using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pm02Ivan.Models
{
    public class FireProd
    {
        public string? Key { get; set; }        
        public int? Id_nota{ get; set; }
        public DateTime Fecha {  get; set; }
        public string? Desc { get; set; }
        public string? Foto { get; set; }
        public string? Audio { get; set; }
    }
}
