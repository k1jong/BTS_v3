using System.ComponentModel.DataAnnotations;

namespace BTS_v3.Models
{
    public class Rasbts
    {
        [Key]
        public string eventProcessedUtcTime { get; set; }
        public string deviceId { get; set; }
        public decimal temperature { get; set; }
        public decimal humidity { get; set; }
    }
}
