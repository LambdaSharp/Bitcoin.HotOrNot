# LambdaSharp - Bitcoin Hot-or-Not App

## Lambda Function

```csharp
public override async Task ProcessEventAsync(LambdaScheduleEvent schedule) {

    // fetch Bitcoin price from API
    var response = await HttpClient.GetAsync("https://api.coindesk.com/v1/bpi/currentprice.json");
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

```csharp
public class BitcoinPriceEvent {

    //--- Properties ---
    public double Price { get; set; }
}
```

## Bitcoin API Payload

```json
{
    "time": {
        "updated": "Oct 19, 2020 03:26:00 UTC",
        "updatedISO": "2020-10-19T03:26:00+00:00",
        "updateduk": "Oct 19, 2020 at 04:26 BST"
    },
    "chartName": "Bitcoin",
    "bpi": {
        "USD": {
            "code": "USD",
            "symbol": "&#36;",
            "rate": "11,439.0650",
            "description": "United States Dollar",
            "rate_float": 11439.065
        }
    }
}
```

## Blazor Page
```html
@page "/"

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

    //--- Properties ---
    protected string BitcoinPrice { get; set; } = "(waiting for price)"
    protected int UpVotes { get; set; }
    protected int DownVotes { get; set; }

    //--- Methods ---
    protected void OnUpVoteClicked() {
        LogInfo("Up vote clicked!");
    }

    protected void OnDownVoteClicked() {
        LogInfo("Down vote clicked!");
    }
}
```
