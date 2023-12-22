using Microsoft.AspNetCore.SignalR;

namespace BTS_v3.Hubs
{
    public class ChartHub : Hub
    {
        public async Task SendChartData(decimal temperature, decimal humidity)
        {
            await Clients.All.SendAsync("ReceiveChartData", temperature, humidity);
        }
    }
}
