using System.ComponentModel.DataAnnotations;

namespace BTS_v3.Models
{
    public class WMAHUData
    {
        [Key]
        public long Data_id { get; set; }
        public string Ahu_id { get; set; }
        public string Run_datetime { get; set; }
        public decimal Ahu_set_temp { get; set; }
        public decimal Ahu_set_hum { get; set; }
        public decimal Ahu_ret_temp { get; set; }
        public decimal Ahu_ret_hum { get; set; }
        public decimal Ahu_sup_temp { get; set; }
        public decimal Ahu_sup_hum { get; set; }
        public decimal Ahu_out_temp { get; set; }
        public decimal Ahu_out_hum { get; set; }
        public short Ahu_comp1_run { get; set; }
        public short Ahu_comp2_run { get; set; }
        public short Ahu_warm_run { get; set; }
        public short Ahu_addhum_run { get; set; }
        public decimal Ahu_cool_diff { get; set; }
        public decimal Ahu_warm_diff { get; set; }
        public decimal Ahu_addhum_diff { get; set; }
        public decimal Ahu_rmvhum_diff { get; set; }
    }
}
