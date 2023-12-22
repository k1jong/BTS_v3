using BTS_v3.Hubs;
using BTS_v3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Diagnostics;
using System.Globalization;

namespace BTS_v3.Controllers
{
    public class HomeController : Controller
    {
		private readonly AppDbContext _context;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IHubContext<DataHub> _hubContext;
		public HomeController(AppDbContext _context, IHttpClientFactory httpClientFactory, IHubContext<DataHub> hubContext)
		{
			_hubContext = hubContext;
			this._context = _context;
			_httpClientFactory = httpClientFactory;
		}
		public IActionResult Index()
		{
			try
			{
				var innersensorData = _context.RaseberryPi
					.Where(x => x.deviceId == "inner")
				   .OrderBy(x => x.eventProcessedUtcTime)
				   .LastOrDefault();

				var outersensorData = _context.RaseberryPi
					.Where(x => x.deviceId == "outer")
				   .OrderBy(x => x.eventProcessedUtcTime)
				   .LastOrDefault();


				ViewBag.innersensorData = innersensorData;
				ViewBag.outersensorData = outersensorData;
				ViewBag.NavbarTitle = "대시보드";

				var maxDateTime = _context.Raw_WMAHUData.Max(x => x.Run_datetime);

				var latestData = _context.Raw_WMAHUData
					.Where(x => x.Run_datetime == maxDateTime)
					.ToList();

				//var combinedViewModel = new CombinedViewModel
				//{
				//	WMAHUData = latestData,
				//	Rasbts = new Rasbts() // Set RasbtsData here as needed
				//};

				//ViewBag.SelectedDate = maxDateTime.Substring(0, 10);
				//ViewBag.SelectedTime = maxDateTime.Substring(10);
				//ViewData["Title"] = $"{ViewBag.SelectedDate}";

				//return View(combinedViewModel);

				var innerData = _context.RaseberryPi
					.Where(r => r.deviceId == "inner")
					.OrderByDescending(r => r.eventProcessedUtcTime)
					.Select(r => new { r.eventProcessedUtcTime, r.temperature, r.humidity })
					.Take(10)
					.ToList();

				var outerData = _context.RaseberryPi
					.Where(r => r.deviceId == "outer")
					.OrderByDescending(r => r.eventProcessedUtcTime)
					.Select(r => new { r.eventProcessedUtcTime, r.temperature, r.humidity })
					.Take(10)
					.ToList();

				var result = new List<dynamic>();

				for (int i = 0; i < innerData.Count && i < outerData.Count; i++)
				{
					var tempDiff = innerData[i].temperature - outerData[i].temperature;
					var humidDiff = innerData[i].humidity - outerData[i].humidity;

					result.Add(new
					{
						eventProcessedUtcTime = innerData[i].eventProcessedUtcTime,
						TempDiff = tempDiff,
						HumidDiff = humidDiff
					});
				}

				ViewBag.result = result;

				return View(latestData);

			}
			catch (Exception ex)
			{
				ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
				return View(new CombinedViewModel());
			}
		}
		public IActionResult Login()
		{
			ViewBag.NavbarTitle = "로그인";
			return View();
		}
        public IActionResult Join()
        {
			ViewBag.NavbarTitle = "회원가입";
			return View();
        }
		public IActionResult Show()
		{
			var dataQueryOne = _context.Raw_WMAHUData
		  .Where(w => w.Run_datetime.Substring(4, 2) == "06" || w.Run_datetime.Substring(4, 2) == "09" || w.Run_datetime.Substring(4, 2) == "07" || w.Run_datetime.Substring(4, 2) == "08")
		  .GroupBy(w => w.Ahu_id)
		  .Select(g => new Wmchart
		  {
			  AhuId = g.Key,
			  AvgSetSupTempDiff = g.Average(w => w.Ahu_set_temp - w.Ahu_sup_temp),
			  AvgSetSupHumDiff = g.Average(w => w.Ahu_set_hum - w.Ahu_sup_hum),
			  AvgSupRetTempDiff = (g.Average(w => w.Ahu_sup_temp - w.Ahu_ret_temp) * -1),
			  AvgSupRetHumDiff = g.Average(w => w.Ahu_sup_hum - w.Ahu_ret_hum)
		  })
		  .OrderBy(w => w.AhuId)
		  .ToList();

			var dataQueryTwo = _context.Raw_WMAHUData
		 .Where(w => w.Run_datetime.Substring(4, 2) == "12" || w.Run_datetime.Substring(4, 2) == "01" || w.Run_datetime.Substring(4, 2) == "02")
		 .GroupBy(w => w.Ahu_id)
		 .Select(g => new Wmchart
		 {
			 AhuId = g.Key,
			 AvgSetSupTempDiff = g.Average(w => w.Ahu_set_temp - w.Ahu_sup_temp),
			 AvgSetSupHumDiff = g.Average(w => w.Ahu_set_hum - w.Ahu_sup_hum),
			 AvgSupRetTempDiff = g.Average(w => w.Ahu_sup_temp - w.Ahu_ret_temp),
			 AvgSupRetHumDiff = g.Average(w => w.Ahu_sup_hum - w.Ahu_ret_hum)
		 })
		 .OrderBy(w => w.AhuId)
		 .ToList();


			var dataQueryThree = _context.Raw_WMAHUData
	  .Where(w => w.Run_datetime.Substring(4, 2) == "03" || w.Run_datetime.Substring(4, 2) == "04" || w.Run_datetime.Substring(4, 2) == "05" || w.Run_datetime.Substring(4, 2) == "10" || w.Run_datetime.Substring(4, 2) == "11")
	  .GroupBy(w => w.Ahu_id)
	  .Select(g => new Wmchart
	  {
		  AhuId = g.Key,
		  AvgSetSupTempDiff = g.Average(w => w.Ahu_set_temp - w.Ahu_sup_temp),
		  AvgSetSupHumDiff = g.Average(w => w.Ahu_set_hum - w.Ahu_sup_hum),
		  AvgSupRetTempDiff = g.Average(w => w.Ahu_sup_temp - w.Ahu_ret_temp),
		  AvgSupRetHumDiff = g.Average(w => w.Ahu_sup_hum - w.Ahu_ret_hum)
	  })
	  .OrderBy(w => w.AhuId)
	  .ToList();

			ViewData["ChartOneData"] = dataQueryOne;
			ViewData["ChartTwoData"] = dataQueryTwo;
			ViewData["ChartThreeData"] = dataQueryThree;

			return View();
		}
		public IActionResult info_fare(string date1, string date2)
		{
			IEnumerable<SelectListItem> dates = _context.INFO_FARE
					   .AsEnumerable()
					   .GroupBy(x => x.FARE_DT.Substring(0, 4))
					   .Select(g => new SelectListItem
					   {
						   Value = g.Key,
						   Text = g.Key + "년"
					   })
					   .OrderByDescending(d => DateTime.ParseExact(d.Value, "yyyy", CultureInfo.InvariantCulture));
			ViewBag.Dates = dates;

			IEnumerable<Infofare> chartData1 = _context.INFO_FARE
				.Where(x => x.FARE_DT.StartsWith(date1))
				.ToList();

			IEnumerable<Infofare> chartData2 = _context.INFO_FARE
				.Where(x => x.FARE_DT.StartsWith(date2))
				.ToList();

			return View((chartData1, chartData2));
		}
		public IActionResult Privacy(string selectedDate, string selectedAhu, string selectedDataType)
		{
			ViewBag.NavbarTitle = "공조기 대시보드";
			DateTime currentDate = string.IsNullOrEmpty(selectedDate)
				? DateTime.Now
				: DateTime.ParseExact(selectedDate, "yyyyMMdd", CultureInfo.InvariantCulture);

			string formattedDate = currentDate.ToString("yyyyMMdd");

			ViewBag.AvailableAhUs = _context.Raw_WMAHUData
				.Select(x => x.Ahu_id)
				.Distinct()
				.OrderBy(x => x)
				.ToList();

			string selectedAhuId = string.IsNullOrEmpty(selectedAhu) ? "A00" : selectedAhu;

			ViewBag.AvailableDates = _context.Raw_WMAHUData
				.Where(x => x.Ahu_id == selectedAhuId)
				.Select(x => x.Run_datetime.Substring(0, 8))
				.Distinct()
				.OrderBy(x => x)
				.ToList();

			bool hasDataForSelectedDate = _context.Raw_WMAHUData
				.Any(x => x.Ahu_id == selectedAhuId && x.Run_datetime.StartsWith(formattedDate));

			if (!hasDataForSelectedDate)
			{
				ViewBag.ErrorMessage = "선택한 날짜에 대한 데이터가 없습니다.";
				return View(Enumerable.Empty<WMAHUData>());
			}

			var chartData = _context.Raw_WMAHUData
				.Where(x => x.Ahu_id == selectedAhuId && x.Run_datetime.Length >= 8 && x.Run_datetime.Substring(0, 8) == formattedDate)
				.ToList();

			var invSetFreqData = _context.Raw_WMINVData
				.Where(x => x.run_datetime.StartsWith(formattedDate))
				.ToList();

			var differenceData = GetDifferenceData(chartData, selectedDataType);

			ViewBag.InvSetFreqData = invSetFreqData;
			ViewBag.DifferenceData = differenceData;
			ViewBag.SelectedDate = currentDate.ToString("yyyy-MM-dd");
			ViewBag.SelectedAhu = selectedAhuId;
			ViewBag.SelectedDataType = selectedDataType;

			return View(chartData);
		}
        public IActionResult Chart_daily(string date)
        {
			ViewBag.NavbarTitle = "일간 전력 소비량";
			IEnumerable<SelectListItem> dates;

            dates = _context.Raw_KepcoDayLpData
                .AsEnumerable()
                .GroupBy(x => x.LpDate.Substring(0, 8))
                .Select(g => new SelectListItem
                {
                    Value = g.Key,
                    Text = DateTime.ParseExact(g.Key, "yyyyMMdd", CultureInfo.InvariantCulture)
                            .ToString("yyyy년 MM월 dd일")
                })
                .OrderByDescending(d => DateTime.ParseExact(d.Value, "yyyyMMdd", CultureInfo.InvariantCulture));

            ViewBag.Dates = dates;

            var chartData = _context.Raw_KepcoDayLpData
              .Where(x => x.LpDate.StartsWith(date))
              .GroupBy(x => x.LpDate.Substring(0, 12))
              .Select(g => new { Date = g.Key, TotalLpData = g.Sum(x => x.LpData) })
              .OrderBy(x => x.Date)
              .ToList();

            var liOverData = _context.Raw_KepcoDayLpData
                .Where(x => x.LpDate.StartsWith(date))
                .AsEnumerable()
                .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 12) })
                .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 0 && int.Parse(x.LpDate.Substring(8, 2)) <= 9) || int.Parse(x.LpDate.Substring(8, 2)) == 23 ? x.LpData : 0) })
                .OrderBy(x => x.Date)
                .Sum(x => x.TotalLpData);

            ViewBag.liOverData = liOverData;

            var midOverData = _context.Raw_KepcoDayLpData
               .Where(x => x.LpDate.StartsWith(date))
               .AsEnumerable()
               .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 12) })
               .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 9 && int.Parse(x.LpDate.Substring(8, 2)) <= 10) || (int.Parse(x.LpDate.Substring(8, 2)) >= 12 && int.Parse(x.LpDate.Substring(8, 2)) <= 13) || (int.Parse(x.LpDate.Substring(8, 2)) >= 17 && int.Parse(x.LpDate.Substring(8, 2)) <= 23) ? x.LpData : 0) })
               .OrderBy(x => x.Date)
               .Sum(x => x.TotalLpData);

            ViewBag.midOverData = midOverData;

            var HiOverData = _context.Raw_KepcoDayLpData
             .Where(x => x.LpDate.StartsWith(date))
             .AsEnumerable()
             .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 12) })
             .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 10 && int.Parse(x.LpDate.Substring(8, 2)) <= 12) || (int.Parse(x.LpDate.Substring(8, 2)) >= 13 && int.Parse(x.LpDate.Substring(8, 2)) <= 17) ? x.LpData : 0) })
             .OrderBy(x => x.Date)
             .Sum(x => x.TotalLpData);

            ViewBag.HiOverData = HiOverData;
			if (date != null)
			{
				int year = int.Parse(date.Substring(0, 4));
				int month = int.Parse(date.Substring(4, 2));
				int day = int.Parse(date.Substring(6, 2));

				string formattedDate = string.Format("{0}년 {1}월 {2}일", year, month, day);

				ViewBag.Date = formattedDate;
			}

			if (date != null)
			{
				DateTime previousDate = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture).AddDays(-1);
				string previousDateString = previousDate.ToString("yyyyMMdd");

				var previousChartData = _context.Raw_KepcoDayLpData
					.Where(x => x.LpDate.StartsWith(previousDateString))
					.GroupBy(x => x.LpDate.Substring(0, 12))
					.Select(g => new { Date = g.Key, TotalLpData = g.Sum(x => x.LpData) })
					.OrderBy(x => x.Date)
					.ToList();

				ViewBag.PreviousChartDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(previousChartData);


				var liOverData1 = _context.Raw_KepcoDayLpData
			 .Where(x => x.LpDate.StartsWith(previousDateString))
			 .AsEnumerable()
			 .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 12) })
			 .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 0 && int.Parse(x.LpDate.Substring(8, 2)) <= 9) || int.Parse(x.LpDate.Substring(8, 2)) == 23 ? x.LpData : 0) })
			 .OrderBy(x => x.Date)
			 .Sum(x => x.TotalLpData);

				ViewBag.liOverData1 = liOverData1;

				var midOverData1 = _context.Raw_KepcoDayLpData
				   .Where(x => x.LpDate.StartsWith(previousDateString))
				   .AsEnumerable()
				   .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 12) })
				   .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 9 && int.Parse(x.LpDate.Substring(8, 2)) <= 10) || (int.Parse(x.LpDate.Substring(8, 2)) >= 12 && int.Parse(x.LpDate.Substring(8, 2)) <= 13) || (int.Parse(x.LpDate.Substring(8, 2)) >= 17 && int.Parse(x.LpDate.Substring(8, 2)) <= 23) ? x.LpData : 0) })
				   .OrderBy(x => x.Date)
				   .Sum(x => x.TotalLpData);

				ViewBag.midOverData1 = midOverData1;

				var HiOverData1 = _context.Raw_KepcoDayLpData
				 .Where(x => x.LpDate.StartsWith(previousDateString))
				 .AsEnumerable()
				 .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 12) })
				 .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 10 && int.Parse(x.LpDate.Substring(8, 2)) <= 12) || (int.Parse(x.LpDate.Substring(8, 2)) >= 13 && int.Parse(x.LpDate.Substring(8, 2)) <= 17) ? x.LpData : 0) })
				 .OrderBy(x => x.Date)
				 .Sum(x => x.TotalLpData);

				ViewBag.HiOverData1 = HiOverData1;

			}

			return View(chartData);
        }
        public IActionResult Chart_Month(string date)
        {
            ViewBag.NavbarTitle = "월간 전력 소비량";
            IEnumerable<SelectListItem> dates;

            dates = _context.Raw_KepcoDayLpData
               .AsEnumerable()
               .GroupBy(x => x.LpDate.Substring(0, 6))
               .Select(g => new SelectListItem
               {
                   Value = g.Key,
                   Text = DateTime.ParseExact(g.Key, "yyyyMM", CultureInfo.InvariantCulture)
                        .ToString("yyyy년 MM월")
               })
               .OrderByDescending(d => DateTime.ParseExact(d.Value, "yyyyMM", CultureInfo.InvariantCulture));

            ViewBag.Dates = dates;

            var chartData = _context.Raw_KepcoDayLpData
              .Where(x => x.LpDate.StartsWith(date))
              .GroupBy(x => x.LpDate.Substring(0, 8))
              .Select(g => new { Date = g.Key, TotalLpData = g.Sum(x => x.LpData) })
              .OrderBy(x => x.Date)
              .ToList();

            var liOverData = _context.Raw_KepcoDayLpData
               .Where(x => x.LpDate.StartsWith(date))
               .AsEnumerable()
               .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 8) })
               .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 0 && int.Parse(x.LpDate.Substring(8, 2)) <= 9) || int.Parse(x.LpDate.Substring(8, 2)) == 23 ? x.LpData : 0) })
               .OrderBy(x => x.Date)
               .Sum(x => x.TotalLpData);

            ViewBag.liOverData = liOverData;

            var midOverData = _context.Raw_KepcoDayLpData
               .Where(x => x.LpDate.StartsWith(date))
               .AsEnumerable()
               .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 8) })
               .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 9 && int.Parse(x.LpDate.Substring(8, 2)) <= 10) || (int.Parse(x.LpDate.Substring(8, 2)) >= 12 && int.Parse(x.LpDate.Substring(8, 2)) <= 13) || (int.Parse(x.LpDate.Substring(8, 2)) >= 17 && int.Parse(x.LpDate.Substring(8, 2)) <= 23) ? x.LpData : 0) })
               .OrderBy(x => x.Date)
               .Sum(x => x.TotalLpData);

            ViewBag.midOverData = midOverData;

            var HiOverData = _context.Raw_KepcoDayLpData
             .Where(x => x.LpDate.StartsWith(date))
             .AsEnumerable()
             .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 8) })
             .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 10 && int.Parse(x.LpDate.Substring(8, 2)) <= 12) || (int.Parse(x.LpDate.Substring(8, 2)) >= 13 && int.Parse(x.LpDate.Substring(8, 2)) <= 17) ? x.LpData : 0) })
             .OrderBy(x => x.Date)
             .Sum(x => x.TotalLpData);

            ViewBag.HiOverData = HiOverData;

            if (date != null)
            {
                int year = int.Parse(date.Substring(0, 4));
                int month = int.Parse(date.Substring(4, 2));

                string formattedDate = string.Format("{0}년 {1}월", year, month);

                ViewBag.Date = formattedDate;
            }
            if (date != null)
            {
                DateTime previousDate = DateTime.ParseExact(date, "yyyyMM", CultureInfo.InvariantCulture).AddMonths(-1);
                string previousDateString = previousDate.ToString("yyyyMM");

                var previousChartData = _context.Raw_KepcoDayLpData
                   .Where(x => x.LpDate.StartsWith(previousDateString))
                   .GroupBy(x => x.LpDate.Substring(0, 8))
                   .Select(g => new { Date = g.Key, TotalLpData = g.Sum(x => x.LpData) })
                   .OrderBy(x => x.Date)
                   .ToList();

                ViewBag.PreviousChartDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(previousChartData);


                var liOverData1 = _context.Raw_KepcoDayLpData
              .Where(x => x.LpDate.StartsWith(previousDateString))
              .AsEnumerable()
              .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 8) })
              .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 0 && int.Parse(x.LpDate.Substring(8, 2)) <= 9) || int.Parse(x.LpDate.Substring(8, 2)) == 23 ? x.LpData : 0) })
              .OrderBy(x => x.Date)
              .Sum(x => x.TotalLpData);

                ViewBag.liOverData1 = liOverData1;

                var midOverData1 = _context.Raw_KepcoDayLpData
                   .Where(x => x.LpDate.StartsWith(previousDateString))
                   .AsEnumerable()
                   .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 8) })
                   .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 9 && int.Parse(x.LpDate.Substring(8, 2)) <= 10) || (int.Parse(x.LpDate.Substring(8, 2)) >= 12 && int.Parse(x.LpDate.Substring(8, 2)) <= 13) || (int.Parse(x.LpDate.Substring(8, 2)) >= 17 && int.Parse(x.LpDate.Substring(8, 2)) <= 23) ? x.LpData : 0) })
                   .OrderBy(x => x.Date)
                   .Sum(x => x.TotalLpData);

                ViewBag.midOverData1 = midOverData1;

                var HiOverData1 = _context.Raw_KepcoDayLpData
                 .Where(x => x.LpDate.StartsWith(previousDateString))
                 .AsEnumerable()
                 .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 8) })
                 .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 10 && int.Parse(x.LpDate.Substring(8, 2)) <= 12) || (int.Parse(x.LpDate.Substring(8, 2)) >= 13 && int.Parse(x.LpDate.Substring(8, 2)) <= 17) ? x.LpData : 0) })
                 .OrderBy(x => x.Date)
                 .Sum(x => x.TotalLpData);

                ViewBag.HiOverData1 = HiOverData1;

            }
            return View(chartData);
        }
        public IActionResult Chart_Year(string date)
        {
            ViewBag.NavbarTitle = "년간 전력 소비량";
            IEnumerable<SelectListItem> dates;

            dates = _context.Raw_KepcoDayLpData
               .AsEnumerable()
               .GroupBy(x => x.LpDate.Substring(0, 4))
               .Select(g => new SelectListItem
               {
                   Value = g.Key,
                   Text = DateTime.ParseExact(g.Key, "yyyy", CultureInfo.InvariantCulture)
                        .ToString("yyyy년")
               })
               .OrderByDescending(d => DateTime.ParseExact(d.Value, "yyyy", CultureInfo.InvariantCulture));

            ViewBag.Dates = dates;

            var chartData = _context.Raw_KepcoDayLpData
              .Where(x => x.LpDate.StartsWith(date))
              .GroupBy(x => x.LpDate.Substring(0, 6))
              .Select(g => new { Date = g.Key, TotalLpData = g.Sum(x => x.LpData) })
              .OrderBy(x => x.Date)
              .ToList();

            var liOverData = _context.Raw_KepcoDayLpData
               .Where(x => x.LpDate.StartsWith(date))
               .AsEnumerable()
               .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 6) })
               .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 0 && int.Parse(x.LpDate.Substring(8, 2)) <= 9) || int.Parse(x.LpDate.Substring(8, 2)) == 23 ? x.LpData : 0) })
               .OrderBy(x => x.Date)
               .Sum(x => x.TotalLpData);

            ViewBag.liOverData = liOverData;

            var midOverData = _context.Raw_KepcoDayLpData
               .Where(x => x.LpDate.StartsWith(date))
               .AsEnumerable()
               .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 6) })
               .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 9 && int.Parse(x.LpDate.Substring(8, 2)) <= 10) || (int.Parse(x.LpDate.Substring(8, 2)) >= 12 && int.Parse(x.LpDate.Substring(8, 2)) <= 13) || (int.Parse(x.LpDate.Substring(8, 2)) >= 17 && int.Parse(x.LpDate.Substring(8, 2)) <= 23) ? x.LpData : 0) })
               .OrderBy(x => x.Date)
               .Sum(x => x.TotalLpData);

            ViewBag.midOverData = midOverData;

            var HiOverData = _context.Raw_KepcoDayLpData
             .Where(x => x.LpDate.StartsWith(date))
             .AsEnumerable()
             .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 6) })
             .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 10 && int.Parse(x.LpDate.Substring(8, 2)) <= 12) || (int.Parse(x.LpDate.Substring(8, 2)) >= 13 && int.Parse(x.LpDate.Substring(8, 2)) <= 17) ? x.LpData : 0) })
             .OrderBy(x => x.Date)
             .Sum(x => x.TotalLpData);

            ViewBag.HiOverData = HiOverData;

            if (date != null)
            {
                int year = int.Parse(date.Substring(0, 4));

                string formattedDate = string.Format("{0}년", year);

                ViewBag.Date = formattedDate;
            }
            if (date != null)
            {
                DateTime previousDate = DateTime.ParseExact(date, "yyyy", CultureInfo.InvariantCulture).AddYears(-1);
                string previousDateString = previousDate.ToString("yyyy");

                var previousChartData = _context.Raw_KepcoDayLpData
                   .Where(x => x.LpDate.StartsWith(previousDateString))
                   .GroupBy(x => x.LpDate.Substring(0, 6))
                   .Select(g => new { Date = g.Key, TotalLpData = g.Sum(x => x.LpData) })
                   .OrderBy(x => x.Date)
                   .ToList();

                ViewBag.PreviousChartDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(previousChartData);


                var liOverData1 = _context.Raw_KepcoDayLpData
              .Where(x => x.LpDate.StartsWith(previousDateString))
              .AsEnumerable()
              .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 6) })
              .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 0 && int.Parse(x.LpDate.Substring(8, 2)) <= 9) || int.Parse(x.LpDate.Substring(8, 2)) == 23 ? x.LpData : 0) })
              .OrderBy(x => x.Date)
              .Sum(x => x.TotalLpData);

                ViewBag.liOverData1 = liOverData1;

                var midOverData1 = _context.Raw_KepcoDayLpData
                   .Where(x => x.LpDate.StartsWith(previousDateString))
                   .AsEnumerable()
                   .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 6) })
                   .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 9 && int.Parse(x.LpDate.Substring(8, 2)) <= 10) || (int.Parse(x.LpDate.Substring(8, 2)) >= 12 && int.Parse(x.LpDate.Substring(8, 2)) <= 13) || (int.Parse(x.LpDate.Substring(8, 2)) >= 17 && int.Parse(x.LpDate.Substring(8, 2)) <= 23) ? x.LpData : 0) })
                   .OrderBy(x => x.Date)
                   .Sum(x => x.TotalLpData);

                ViewBag.midOverData1 = midOverData1;

                var HiOverData1 = _context.Raw_KepcoDayLpData
                 .Where(x => x.LpDate.StartsWith(previousDateString))
                 .AsEnumerable()
                 .GroupBy(x => new { DateSubstring = x.LpDate.Substring(0, 6) })
                 .Select(g => new { Date = g.Key.DateSubstring, TotalLpData = g.Sum(x => (int.Parse(x.LpDate.Substring(8, 2)) >= 10 && int.Parse(x.LpDate.Substring(8, 2)) <= 12) || (int.Parse(x.LpDate.Substring(8, 2)) >= 13 && int.Parse(x.LpDate.Substring(8, 2)) <= 17) ? x.LpData : 0) })
                 .OrderBy(x => x.Date)
                 .Sum(x => x.TotalLpData);

                ViewBag.HiOverData1 = HiOverData1;

            }
            return View(chartData);
        }
        public IActionResult Lpdata()
		{
			ViewBag.NavbarTitle = "계절별 공조기 효율";

			var data = _context.Raw_WMAHUData
		 .Where(x => x.Ahu_id == "A00")
		 .OrderBy(x => x.Run_datetime)
		 .ToList();

			// Group data by day
			var groupedData = data.GroupBy(x => DateTime.ParseExact(x.Run_datetime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Month);

			var Winter = new List<(double Y, double X)>
			{ 
                //1월
                (0.412973, 8269.65),
				(0.33125, 20757.6),
				(0.165435, 21359.97),
				(0.100284, 21552.39),
				(0.09719, 20944.98),
				(0.032987, 18547.02),
				(0.092961, 17549.19),
				(0.175789, 20136.42),
				(0.030334, 20837.16),
				(-0.016793, 21513.15),
				(0.274148, 18491.94),
				(0.295896, 20755.62),
				(0.293308, 18355.59),
				(0.086238, 17957.97),
				(-0.053251, 21392.37),
				(-0.233049, 21829.77),
				(0.011174, 21221.19),
				(0.268372, 21043.53),
				(0.411143, 19938.15),
				(0.222917, 17545.86),
				(0.128536, 17065.35),
				(0.189805, 19172.79),
				(-0.086711, 19096.65),
				(-0.328251, 19989.63),
				(-0.456534, 20444.13),
				(-0.286079, 19180.71),
				(-0.212531, 17100.81),
				(-0.013825, 16950.06),
				(0.31774, 16968.24),
				(0.36894, 17139.87),
				(0.401073, 19571.76),
				(0.245202, 18705.33),
				(0.152241, 17921.7),
				(0.390593, 17524.62),
				(0.43024, 14933.88),
				(0.181692, 14716.8),
				(-0.052557, 18121.5),
				(-0.203819, 17990.91),
				(-0.337185, 16362.9),
				(-0.314899, 13798.35),
				(-0.218687, 13887.81),
				(-0.261617, 13761.18),
				(-0.366288, 13636.17),
				(0.049558, 16730.01),
				(0.321306, 17993.52),
				(0.334091, 18779.13),
				(0.183964, 19031.49),
				(0.239552, 17593.47),
				(0.415151, 14871.69),
				(0.230934, 14386.23),
				(0.227115, 16664.58),
				(0.304766, 18131.22),
				(0.157702, 18166.68),
				(-0.061648, 18185.85),
				(-0.283586, 18131.94),
				(-0.236616, 15885.9),
				(-0.390183, 15753.6),

			};

			var Summer = new List<(double X, double Y)>
			 {
                //6월
                (-0.928725, 18913.59),
				(-0.672537, 19472.4),
				(-0.382797, 19514.25),
				(-0.450568, 18278.64),
				(-0.563763, 16886.25),
				(-0.495865, 18043.65),
				(-0.730082, 19469.16),
				(-0.659659, 19924.29),
				(-0.354483, 20796.84),
				(-0.425852, 21587.13),
				(-0.694949, 20775.15),
				(-0.621307, 20068.29),
				(-0.613889, 19785.78),
				(-0.876926, 21478.59),
				(-0.530271, 21381.66),
				(-0.379104, 21581.64),
				(-0.395171, 21044.25),
				(-0.358112, 21572.01),
				(-0.338352, 19770.57),
				(-0.493845, 19690.83),
				(-0.601894, 21328.65),
				(-0.543908, 21656.07),
				(-0.573453, 21746.61),
				(-0.496559, 21560.76),
				(-0.579262, 21440.97),
				(-0.64315, 20729.79),
				(-0.649274, 20309.58),
				(-0.528314, 21526.92),
				(-0.344192, 22058.01),
				(-0.146844, 21866.4),
				//7월
				(5.193624, 22319.46),
				(5.013036, 21991.32),
				(4.993119, 20522.88),
				(5.437374, 20500.56),
				(5.65322, 21818.97),
				(5.792803, 22152.87),
				(5.551862, 21880.89),
				(5.077336, 22724.28),
				(4.912058, 22444.2),
				(4.854103, 21225.06),
				(4.897475, 21655.89),
				(4.930177, 23277.87),
				(4.855745, 24121.08),
				(4.809406, 23897.61),
				(4.594728, 23828.31),
				(4.32863, 10748.97),
				(4.094508, 22319.46),
				(4.482197, 21991.32),
				(4.714742, 20522.88),
				(4.894728, 20500.56),
				(4.547727, 21818.97),
				(4.61168, 22152.87),
				(4.337753, 21880.89),
				(4.323137, 22724.28),
				(4.539489, 22444.2),
				(4.831503, 21225.06),
				(4.807387, 21655.89),
				(4.82339, 23277.87),
				(4.940246, 24121.08),
				(4.869318, 23897.61),
				(4.583239, 23828.31),
				(4.77298, 24752.79),
				(4.796528, 24521.94),
				(4.730177, 24602.13),
				(4.846749, 24773.76),
				(4.865089, 24509.7),
				(4.66916, 23002.02),
				(4.68589, 22997.52),
				(4.822096, 24481.26),
				(4.907734, 24677.64),
				(4.709596, 23553),
				(4.75082, 23821.11),
				(4.724021, 23859.27),
				(4.466383, 22452.66),
				(4.399558, 22659.12),
				(4.437784, 23155.83),
				(4.749653, 24328.71),
				(4.790688, 23119.74),
				(5.069476, 21676.41),
				(5.101579, 21907.26),
				(5.008302, 21927.69),
				(4.814488, 21405.15),
				(5.105366, 23239.71),
				(5.008081, 23311.98),
				(4.575032, 21795.84),
				(4.326863, 21070.08),
				(4.350221, 21059.55),
				(3.986237, 18783),
				(3.837216, 18635.49),
				(3.94274, 20513.79),
				(3.962784, 20631.33),
				(3.972727, 20958.75),
			   (4.77298, 24752.79),
				(4.796528, 24521.94),
				(4.730177, 24602.13),
				(4.846749, 24773.76),
				(4.865089, 24509.7),
				(4.66916, 23002.02),
				(4.68589, 22997.52),
				(4.822096, 24481.26),
				(4.907734, 24677.64),
				(4.709596, 23553),
				(4.75082, 23821.11),
				(4.724021, 23859.27),
				(4.466383, 22452.66),
				(4.399558, 22659.12),
				(4.437784, 23155.83),
				(4.749653, 24328.71),
				(4.790688, 23119.74),
				(5.069476, 21676.41),
				(5.101579, 21907.26),
				(5.008302, 21927.69),
				(4.814488, 21405.15),
				(5.105366, 23239.71),
				(5.008081, 23311.98),
				(4.575032, 21795.84),
				(4.326863, 21070.08),
				(4.350221, 21059.55),
				(3.986237, 18783),
				(3.837216, 18635.49),
				(3.94274, 20513.79),
				(3.962784, 20631.33),
				(3.972727, 20958.75)
			 };

			var Spring = new List<(double X, double Y)>
			{
				(-0.318466, 14805.54),
				(-0.389267, 17128.53),
				(-0.497411, 17413.56),
				(-0.500632, 17217.9),
				(-0.203346, 16452.09),
				(0.016572, 14679.27),
				(-0.238889, 14709.6),
				(-0.386395, 16932.87),
				(-0.340436, 17435.25),
				(-0.537911, 17074.8),
				(-0.667487, 17457.66),
				(-0.542929, 16883.01),
				(-0.108397, 15041.25),
				(-0.514268, 14822.19),
				(-0.699622, 16726.14),
				(-0.85363, 17117.19),
				(-1.104072, 17128.89),
				(-0.824243, 16654.5),
				(-0.479261, 16201.98),
				(-0.432765, 15210.27),
				(-0.499937, 15141.96),
				(-0.625853, 17103.51),
				(-0.635448, 16783.74),
				(-0.685764, 17014.32),
				(-0.706282, 16896.15),
				(0.49113, 16697.34),
				(-0.509438, 15504.03),
				(-0.703851, 15968.79),
				(-0.765467, 17253.45),
				(-0.605366, 17583.57),
				(-0.685637, 17213.04),
				(-0.72159, 17571.42),
				(-0.642393, 16909.56),
				(-0.616162, 15966.72),
				(-0.708712, 11887.11),
				(-0.739204, 17461.35),
				(-0.766351, 16958.07),
				(-0.784344, 16888.59),
				(-0.782387, 17264.88),
				(-0.72344, 17165.88),
				(-0.728661, 15705),
				(-0.506566, 15431.04),
				(0.0869, 16672.05),
				(0.188131, 17223.66),
				(-0.628756, 17269.02),
				(-0.716788, 16953.21),
				(-0.702683, 16389.63),
				(-0.477557, 15472.98),
				(-0.529956, 15037.83),
				(-0.59536, 16418.61),
				(-0.728125, 16728.48),
				(-0.757165, 17121.51),
				(-0.631629, 16991.46),
				(-0.602683, 17119.26),
				(-0.469192, 16097.85),
				(-0.034217, 15700.68),
				(-0.106439, 17535.87),
				(-0.779987, 17171.28),
				(-0.683933, 17522.73),
				(-0.597254, 16854.84),
				(-0.684343, 16049.61),
				//5월
				(-0.558933, 14888.7),
				(-0.682702, 15034.68),
				(-0.674085, 16565.4),
				(-0.742614, 17493.93),
				(-0.667235, 16556.85),
				(-0.628377, 17547.21),
				(-0.281124, 16774.38),
				(-0.8232, 15451.56),
				(-0.774021, 15418.44),
				(-0.775473, 17135.73),
				(-0.731029, 16972.2),
				(-0.736521, 17445.33),
				(-0.816478, 18718.92),
				(-0.772948, 18028.44),
				(-0.72661, 17381.97),
				(-0.8268, 16551.45),
				(-0.636363, 17422.11),
				(-0.778914, 17585.37),
				(-0.829672, 16329.69),
				(-0.771086, 17416.62),
				(-0.548484, 17946.45),
				(-0.680776, 16059.51),
				(-0.533712, 15680.25),
				(-0.830556, 17585.91),
				(-0.682071, 17254.98),
				(-0.630978, 17499.42),
				(-0.614078, 17742.78),
				(-0.753662, 16958.88),
				(-0.365499, 15916.23),
				(-0.219918, 15685.11),
				(-0.705461, 18342.99),
			};

			var Fall = new List<(double X, double Y)>
			{
(-0.17077, 18913.14),
(-0.09959, 18009.54),
(-0.063762, 19753.02),
(-0.171812, 20968.74),
(-0.366446, 18306.81),
(-0.320171, 18776.43),
(-0.291067, 17818.74),
(-0.336963, 16648.29),
(-0.427273, 16696.53),
(-0.469729, 16001.01),
(-0.668308, 17366.13),
(-0.776357, 17605.8),
(-0.761459, 18281.52),
(-0.602809, 18366.93),
(-0.536932, 18182.79),
(-0.800505, 17892.9),
(-0.588384, 18963.45),
(-0.664615, 18970.83),
(-0.521243, 18524.34),
(-0.589488, 18042.21),
(-0.603472, 18005.13),
(-0.707702, 17034.75),
(-0.667267, 17888.94),
(-0.691951, 18591.39),
(-0.744318, 7135.65),
(-0.344003, 19120.5),
(-0.593213, 17917.65),
(-0.778504, 17892.72),
(-0.675884, 17863.56),
(-0.782765, 17927.91),
(-0.921527, 16373.25),
(-0.91171, 16269.3),
(-0.642677, 18607.05),
(-0.673517, 18297.81),
(-0.596591, 18127.35),
(-0.580714, 18104.13),
(-0.567519, 18089.73),
(-0.451452, 16875.63),
(-0.366666, 16898.67),
(-0.520391, 18349.65),
(-0.55565, 18659.25),
(-0.632007, 18798.66),
(-0.368907, 19543.77),
(-0.059766, 20073.42),
(-0.750866, 17373.33),
(-0.400536, 17036.64),
(-0.526579, 18758.07),
(-0.610259, 19608.39),
(-0.433523, 19480.05),
(-0.389804, 19170.09),
(-0.478093, 18894.15),
(-0.70868, 16746.48),
(-0.788724, 16636.23)
			};

			ViewBag.Spring = JsonConvert.SerializeObject(Spring);
			ViewBag.Winter = JsonConvert.SerializeObject(Winter);
			ViewBag.Summer = JsonConvert.SerializeObject(Summer);
			ViewBag.Fall = JsonConvert.SerializeObject(Fall);

		IEnumerable<Infofare> chartData1 = _context.INFO_FARE
		.Where(x => x.FARE_DT.StartsWith("2021"))
		.ToList();

			ViewBag.chartData1 = chartData1;

			return View(groupedData);
		}
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
		public IActionResult sarimadata()
		{
			ViewBag.NavbarTitle = "예측데이터";
			return View();
		}
		[HttpPost]
        public async Task<IActionResult> GetSensorData()
        {
            var innersensorData = await _context.RaseberryPi
				.Where(x =>x.deviceId == "inner")
                .OrderBy(x => x.eventProcessedUtcTime)
                .LastOrDefaultAsync(); // 가장 최근 데이터 가져오기

			var outersensorData = await _context.RaseberryPi
				.Where(x => x.deviceId == "outer")
				.OrderBy(x => x.eventProcessedUtcTime)
				.LastOrDefaultAsync(); // 가장 최근 데이터 가져오기

			await _hubContext.Clients.All.SendAsync("ReceiveSensorData", innersensorData.temperature, innersensorData.humidity, outersensorData.temperature, outersensorData.humidity);
			return Ok();
		}
		[HttpPost]
		public async Task<IActionResult> GetChartData()
		{
			try
			{
				// Retrieve the latest data for inner and outer devices from the database
				var innerData = await _context.RaseberryPi
					.Where(x => x.deviceId == "inner")
					.OrderByDescending(d => d.eventProcessedUtcTime)
					.Take(10)
					.ToListAsync();

				var outerData = await _context.RaseberryPi
					.Where(x => x.deviceId == "outer")
					.OrderByDescending(d => d.eventProcessedUtcTime)
					.Take(10)
					.ToListAsync();

				// If data for either inner or outer devices is not available, return an empty JSON array
				if (innerData.Count == 0 || outerData.Count == 0)
				{
					return Json(new List<dynamic>());
				}

				// Calculate the temperature and humidity differences between inner and outer devices
				var result = new List<dynamic>();

				for (int i = 0; i < innerData.Count && i < outerData.Count; i++)
				{
					var tempDiff = innerData[i].temperature - outerData[i].temperature;
					var humidDiff = innerData[i].humidity - outerData[i].humidity;

					result.Add(new
					{
						eventProcessedUtcTime = innerData[i].eventProcessedUtcTime,
						TempDiff = tempDiff,
						HumidDiff = humidDiff
					});
				}

				// Return the result as JSON
				return Json(result);
			}
			catch (Exception ex)
			{
				// Return a BadRequest response with an error message in case of an exception
				return BadRequest(new { error = ex.Message });
			}
		}

		private List<double> GetDifferenceData(List<WMAHUData> chartData, string selectedDataType)
		{
			return selectedDataType == "temperature"
				? chartData.Select(c => (double)(c.Ahu_ret_temp - c.Ahu_set_temp)).ToList()
				: chartData.Select(c => (double)(c.Ahu_ret_hum - c.Ahu_set_hum)).ToList();
		}
		public IActionResult ExportToExcel(string selectedDate, string selectedAhu, string selectedDataType)
		{
			string formattedDate = DateTime.ParseExact(selectedDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("yyyyMMdd");

			var chartData = _context.Raw_WMAHUData
				.Where(x => x.Ahu_id == selectedAhu && x.Run_datetime.Length >= 8 && x.Run_datetime.Substring(0, 8) == formattedDate)
				.ToList();

			var joinedData = (from invData in _context.Raw_WMINVData
							  join ahuData in _context.Raw_WMAHUData
							  on invData.run_datetime equals ahuData.Run_datetime
							  where invData.run_datetime.StartsWith(formattedDate) && ahuData.Ahu_id == selectedAhu
							  select new
							  {
								  invData.run_datetime,
								  invData.inv_set_freq,
								  ahuData.Ahu_ret_temp,
								  ahuData.Ahu_sup_temp,
								  ahuData.Ahu_set_temp,
								  ahuData.Ahu_ret_hum,
								  ahuData.Ahu_sup_hum,
								  ahuData.Ahu_set_hum
							  }).ToList();

			using (var package = new ExcelPackage())
			{
				// 첫 번째 시트 (Raw_WMAHUData 데이터)
				var worksheetChart = package.Workbook.Worksheets.Add("WMAHUData_Chart");

				worksheetChart.Cells[1, 1].Value = "실행 날짜";
				worksheetChart.Cells[1, 2].Value = selectedDataType == "temperature" ? "AHU 설정 온도" : "AHU 설정 습도";
				worksheetChart.Cells[1, 3].Value = selectedDataType == "temperature" ? "AHU 반환 온도" : "AHU 반환 습도";
				worksheetChart.Cells[1, 4].Value = selectedDataType == "temperature" ? "AHU 공급 온도" : "AHU 공급 습도";
				worksheetChart.Cells[1, 5].Value = selectedDataType == "temperature" ? "AHU 배출 온도" : "AHU 배출 습도";

				int rowChart = 2;
				foreach (var item in chartData)
				{
					worksheetChart.Cells[rowChart, 1].Value = item.Run_datetime;
					worksheetChart.Cells[rowChart, 2].Value = selectedDataType == "temperature" ? item.Ahu_set_temp : item.Ahu_set_hum;
					worksheetChart.Cells[rowChart, 3].Value = selectedDataType == "temperature" ? item.Ahu_ret_temp : item.Ahu_ret_hum;
					worksheetChart.Cells[rowChart, 4].Value = selectedDataType == "temperature" ? item.Ahu_sup_temp : item.Ahu_sup_hum;
					worksheetChart.Cells[rowChart, 5].Value = selectedDataType == "temperature" ? item.Ahu_out_temp : item.Ahu_out_hum;
					rowChart++;
				}

				// 두 번째 시트 (WMINVData 및 WMAHUData 데이터 조인)
				var worksheetInv = package.Workbook.Worksheets.Add("WMINVData_WMAHUData");

				worksheetInv.Cells[1, 1].Value = "실행 날짜";
				worksheetInv.Cells[1, 2].Value = "inv_set_freq";
				worksheetInv.Cells[1, 3].Value = selectedDataType == "temperature" ? "반환 온도 - 공급 온도 차이" : "반환 습도 - 공급 습도 차이";

				int rowInv = 2;
				foreach (var item in joinedData)
				{
					worksheetInv.Cells[rowInv, 1].Value = item.run_datetime;
					worksheetInv.Cells[rowInv, 2].Value = item.inv_set_freq;
					worksheetInv.Cells[rowInv, 3].Value = selectedDataType == "temperature" ? item.Ahu_ret_temp - item.Ahu_sup_temp : item.Ahu_ret_hum - item.Ahu_sup_hum;

					rowInv++;
				}

				// 세 번째 시트 (반환온도-설정온도 데이터)
				var worksheetTemperatureDifference = package.Workbook.Worksheets.Add("TemperatureDifference");

				worksheetTemperatureDifference.Cells[1, 1].Value = "실행 날짜";
				worksheetTemperatureDifference.Cells[1, 2].Value = selectedDataType == "temperature" ? "반환 온도 - 설정 온도 차이" : "반환 습도 - 설정 습도 차이";

				int rowTemperatureDifference = 2;
				foreach (var item in joinedData)
				{
					worksheetTemperatureDifference.Cells[rowTemperatureDifference, 1].Value = item.run_datetime;
					worksheetTemperatureDifference.Cells[rowTemperatureDifference, 2].Value = selectedDataType == "temperature" ? item.Ahu_ret_temp - item.Ahu_set_temp : item.Ahu_ret_hum - item.Ahu_set_hum;

					rowTemperatureDifference++;
				}

				// 파일 저장 및 다운로드
				var stream = new MemoryStream(package.GetAsByteArray());
				var fileName = $"WMAHUData_{selectedDate}_{selectedAhu}_{selectedDataType}.xlsx";

				return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
			}
		}
		[HttpPost]
		public async Task<IActionResult> GetChatGptResponse([FromBody] string userMessage)
		{
			var chatGPT = new ChatGPT(_httpClientFactory.CreateClient());
			var response = await chatGPT.GenerateLoremIpsum(userMessage);
			return Json(new { assistantResponse = response });
		}

	}
	public class ChatGPT
	{
		private readonly HttpClient _httpClient;

		public ChatGPT(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<string> GenerateLoremIpsum(string userMessage)
		{
			var apiKey = "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";
			_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

			// JSON content for the API call
			var jsonContent = new
			{
				prompt = userMessage,
				model = "text-davinci-003",
				max_tokens = 1000
			};

			try
			{
				// Make the API call
				var responseContent = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/completions", jsonContent);

				// Read the response as a string
				var resContext = await responseContent.Content.ReadAsStringAsync();

				// Deserialize the response into a dynamic object
				var data = JsonConvert.DeserializeObject<dynamic>(resContext);

				// Check if choices array is not null and not empty
				if (data.choices != null && data.choices.Count > 0)
				{
					return data.choices[0].text;
				}
				else
				{
					return "No response from ChatGPT.";
				}
			}
			catch (Exception ex)
			{
				return $"An error occurred: {ex.Message}";
			}
		}
	}
}