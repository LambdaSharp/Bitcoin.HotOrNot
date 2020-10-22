using System.Threading.Tasks;
using Bitcoin.HotOrNot.Common;
using LambdaSharp;
using LambdaSharp.Schedule;
using Newtonsoft.Json.Linq;

namespace Bitcoin.HotOrNot.PublishBitcoinPriceFunction {

    public sealed class Function : ALambdaScheduleFunction {

        //--- Methods ---
        public override async Task InitializeAsync(LambdaConfig config) { }

        public override async Task ProcessEventAsync(LambdaScheduleEvent schedule) {
            var response = await HttpClient.GetAsync("https://api.coindesk.com/v1/bpi/currentprice.json");
            if(response.IsSuccessStatusCode) {
                LogInfo("Fetched price from API");

                // read price from API response
                dynamic json = JObject.Parse(await response.Content.ReadAsStringAsync());
                var price = (double)json.bpi.USD.rate_float;

                // log and send CloudWatch event
                LogEvent(new BitcoinPriceEvent {
                    Price = price
                });
            } else {
                LogInfo("Unable to fetch price");
            }
        }
    }
}
