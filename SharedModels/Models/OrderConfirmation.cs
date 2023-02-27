using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models
{
    public class OrderConfirmation
    {
        public int SiteId { get; set; }
        public int LocationId { get; set; }
        public int OrderNumber { get; set; }
        public string ErrorMessage { get; set; }
    }
}
