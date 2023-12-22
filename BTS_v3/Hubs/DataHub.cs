using Microsoft.AspNetCore.SignalR;

namespace BTS_v3.Hubs
{
    public class DataHub : Hub
    {
		public async Task SendSensorData(string itmp, decimal ihum, string otmp, decimal ohum)
		{
			await Clients.All.SendAsync("ReceiveSensorData", itmp, ihum, otmp, ohum);
		}
	}
}
