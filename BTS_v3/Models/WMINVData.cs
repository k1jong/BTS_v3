using System.ComponentModel.DataAnnotations;

namespace BTS_v3.Models
{
    public class WMINVData
    {
        [Key]
        public long data_id { get; set; }
        public string inv_id { get; set; }
        public string run_datetime { get; set; }
        public decimal inv_A { get; set; }
        public decimal inv_set_freq { get; set; }
        public decimal inv_cur_freq { get; set; }
        public short inv_run_state { get; set; }
        public short inv_alram { get; set; }
    }
}
