using System.ComponentModel.DataAnnotations;

namespace BTS_v3.Models
{
    public class KepcoDayLpData
    {
        [Key]
        public int LpID { get; set; }
        public string LpDate { get; set; }
        public decimal LpData { get; set; }
    }
}
