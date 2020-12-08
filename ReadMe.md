# LambdaSharp - Bitcoin Hot-or-Not App

## Lambda Function

```csharp
        public override async Task ProcessEventAsync(LambdaScheduleEvent schedule) {

            // fetch Bitcoin price from API
            var response = await HttpClient.GetAsync("https://api.coindesk.com/v1/bpi/currentprice.json");

            /* Sample JSON Response:
            * {
            *     "bpi": {
            *         "USD": {
            *             "rate_float": 18833.915
            *         }
            *     }
            * }
            */

            if(response.IsSuccessStatusCode) {
                LogInfo("Fetched price from API");

                // extract Bitcoin price from response
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
```


## Event Data-Type

```csharp
public class BitcoinPriceEvent {

    //--- Properties ---
    public double Price { get; set; }
}
```

## Blazor Page

```html
@page "/"

@using Bitcoin.HotOrNot.Common

@inherits ALambdaComponent

@inject LambdaSharp.App.LambdaSharpEventBusClient EventBus

<Container Fluid="true">
    <Card Margin="Margin.Is4.FromTop">
        <CardHeader Padding="Padding.Is1.FromBottom">
            <Heading Size="HeadingSize.Is4">Bitcoin Price</Heading>
        </CardHeader>

        <CardBody Padding="Padding.Is0.FromBottom">
            <CardTitle Size="5">@BitcoinPrice</CardTitle>
        </CardBody>

        <CardBody>
            <Button Size="ButtonSize.Large" Color="Color.Primary" Clicked="@OnUpVoteClicked">
                <Icon Name="IconName.ThumbsUp" /><br/>(@UpVotes)
            </Button>
            <Button Size="ButtonSize.Large" Color="Color.Primary" Clicked="@OnDownVoteClicked">
                <Icon Name="IconName.ThumbsDown" /><br/>(@DownVotes)
            </Button>
        </CardBody>
    </Card>
</Container>

@code {

    //--- Fields ---
    private Dictionary<string, bool> _votes = new Dictionary<string, bool>();

    //--- Properties ---
    protected string BitcoinPrice { get; set; } = "(waiting for price)";
    protected int UpVotes { get; set; }
    protected int DownVotes { get; set; }

    //--- Methods ---
    protected void OnUpVoteClicked() {
        LogInfo("Up vote clicked!");

        // TODO: emit up-vote event
    }

    protected void OnDownVoteClicked() {
        LogInfo("Down vote clicked!");

        // TODO: emit down-vote event
    }

    protected override void OnInitialized() {
        base.OnInitialized();

        // TODO: initialize CloudWatch event subscriptions
    }
}
```
