namespace BTS_v3.Models
{
    public class IndexViewModel
    {
        public List<InvDataViewModel> InvDataList { get; set; }
    }
    public class InvDataViewModel
    {
        public string InvId { get; set; }
        public decimal InvA { get; set; }
        public decimal CalculatedValue { get; internal set; }
	}
}
