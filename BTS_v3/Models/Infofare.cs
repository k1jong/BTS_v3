using System.ComponentModel.DataAnnotations;

namespace BTS_v3.Models
{
    public class Infofare
    {
        [Key]
        public int FARE_ID { get; set; }
        public string FARE_DT { get; set; }
        public decimal FARE_KW { get; set; }
        public int FARE_KW_FARE { get; set; }
        public int Fare_Base_Fare { get; set; }
        public int Fare_Total_Fare { get; set; }
        public int Fare_Kikum { get; set; }
        public int Fare_Cur_Month_Fare { get; set; }
        public string Fare_Desc { get; set; }
        public string Cre_User { get; set; }
        public DateTime Cre_Dt { get; set; }
        public string Mod_User { get; set; }
        public DateTime Mod_Dt { get; set; }
	}
}
