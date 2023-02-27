using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharedModels.Models
{
    public class OrderConfirmation
    {
        [JsonPropertyName("site_id")]
        public int SiteId { get; set; }
        [JsonPropertyName("contact_id")]
        public int ContactId { get; set; }
        [JsonPropertyName("location_id")]
        public int LocationId { get; set; }
        [JsonPropertyName("order_number")]
        public int OrderNumber { get; set; }
        [JsonPropertyName("error_msg")]
        public string? ErrorMessage { get; set; }
    }
}
