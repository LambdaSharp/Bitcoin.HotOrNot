# LambdaSharp - Bitcoin Hot-or-Not App

## Prerequisites

1. Sign-up for AWS Account: https://aws.amazon.com/
1. Create AWS credentials file: [See Instructions](https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-files.html)
1. Install .NET Core SDK 3.1.401+ (any platform Windows, macOS, or Linux)
1. Install an IDE, such as [Visual Studio Code](https://code.visualstudio.com/)

## Step 1: Getting Started

Install the LambdaSharp CLI:
```bash
dotnet tool install -g LambdaSharp.Tool
```

Create a LambdaSharp deployment tier:
```
lash init --quick-start
```

Clone Git repository from GitHub:
```bash
git clone git@github.com:LambdaSharp/Bitcoin.HotOrNot.git
```

Switch into the cloned folder:
```bash
cd Bitcoin.HotOrNot
```

## Step 2: Create Lambda function to fetch Bitcoin price

Create a new LambdaSharp module, which describes the required serverless infrastructure:
```bash
lash new module Bitcoin.HotOrNot
```

Add a Lambda function configured for an [EventBridge Schedule Rule](https://docs.aws.amazon.com/eventbridge/latest/userguide/scheduled-events.html) to the module:
```bash
lash new function --type schedule PublishBitcoinPriceFunction
```

Update Lambda function to fetch the Bitcoin price from the [CoinDesk API](https://api.coindesk.com/v1/bpi/currentprice.json) and publish it to CloudWatch EvenBridge event bus.

Define a C# class for the CloudWatch event and use [LogEvent(...)](https://lambdasharp.net/sdk/LambdaSharp.ALambdaFunction.html#LambdaSharp_ALambdaFunction_LogEvent__1___0_System_Collections_Generic_IEnumerable_System_String__) method to emit it to CloudWatch EventBridge. _(Hint: consider putting this class in a location so it can be shared with the frontend code later.)_
```csharp
public class BitcoinPriceEvent {

    //--- Properties ---
    public double Price { get; set; }
}
```

Deploy module with Lambda function:
```bash
lash deploy
```

Log into the [AWS Console](https://aws.amazon.com/console/) and confirm the event shows up in the CloudWatch log group for the `PublishBitcoinPriceFunction` Lambda function.

<details><summary>Sample JSON Response</summary>

```json
{
    "time": {
        "updated": "Oct 19, 2020 03:26:00 UTC",
        "updatedISO": "2020-10-19T03:26:00+00:00",
        "updateduk": "Oct 19, 2020 at 04:26 BST"
    },
    "disclaimer": "This data was produced from the CoinDesk Bitcoin Price Index (USD). Non-USD currency data converted using hourly conversion rate from openexchangerates.org",
    "chartName": "Bitcoin",
    "bpi": {
        "USD": {
            "code": "USD",
            "symbol": "&#36;",
            "rate": "11,439.0650",
            "description": "United States Dollar",
            "rate_float": 11439.065
        },
        "GBP": {
            "code": "GBP",
            "symbol": "&pound;",
            "rate": "8,843.9415",
            "description": "British Pound Sterling",
            "rate_float": 8843.9415
        },
        "EUR": {
            "code": "EUR",
            "symbol": "&euro;",
            "rate": "9,764.7405",
            "description": "Euro",
            "rate_float": 9764.7405
        }
    }
}
```
</details>


## Step 3: Create Blazor WebAssembly app to show Bitcoin price

Add a Blazor WebAssembly project to the module:
```bash
lash new app VoteApp
```

Update app project by editing _Index.razor_ page with the UI code below.

<details><summary>Sample Blazorise UI Layout</summary>

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
            <Button Size="ButtonSize.Large" Color="Color.Primary" Clicked="@OnUpVoteClicked" Disabled="@IsDisabled">
                <Icon Name="IconName.ThumbsUp" /><br/>(@UpVotes)
            </Button>
            <Button Size="ButtonSize.Large" Color="Color.Primary" Clicked="@OnDownVoteClicked" Disabled="@IsDisabled">
                <Icon Name="IconName.ThumbsDown" /><br/>(@DownVotes)
            </Button>
        </CardBody>
    </Card>
</Container>

@code {

    //--- Properties ---
    protected string BitcoinPrice { get; set; } = "(waiting for price)"
    protected bool IsDisabled { get; set; } = true;
    protected int UpVotes { get; set; }
    protected int DownVotes { get; set; }
}
```
</details>

Redeploy the module:
```bash
lash deploy
```

## Step 4: Forward CloudWatch events to the Blazor app

Update the `Module.yml` file to forward CloudWatch events to the LambdaSharp App EventBus for the `VoteApp`.

<details><summary>LambdaSharp App Event Source</summary>

```yaml
  - App: VoteApp
    Sources:
      - EventBus: default
        Pattern:
          Source:
            - Bitcoin.HotOrNot::PublishBitcoinPriceFunction
            - Bitcoin.HotOrNot::VoteApp
```
</details>

Define a `BitcoinVoteEvent` class to trigger when the up/down vote buttons are clicked and use `LogEvent(...)` to emit the vote. _(Hint: use `Info.AppInstanceId` as the voter ID since it's unique per app instance.)_
```csharp
public class BitcoinVoteEvent {

    //--- Properties ---
    public string VoterId { get; set; }
    public bool Vote { get; set; }
}
```

Update the code in _Index.razor_ to subscribe to the events and bind them to their respective methods (`OnBitcoinPriceUpdated(BitcoinPriceEvent)` and `OnBitcoinVote(BitcoinVoteEvent)`):
```csharp
protected override void OnInitialized() {
    LogInfo("Initialzing component...");
    base.OnInitialized();

    EventBus.SubscribeTo<BitcoinPriceEvent>("Bitcoin.HotOrNot::PublishBitcoinPriceFunction", OnBitcoinPriceUpdated);
    EventBus.SubscribeTo<BitcoinVoteEvent>("Bitcoin.HotOrNot::VoteApp", OnBitcoinVote);

    LogInfo("Component initialized");
}
```

Make sure to invoke `StateHasChanged()` method to indicate to Blazor the UI needs to be redrawn.

## Step 5: Deploy final Blazor app

Deploy the final code and enjoy!
```bash
lash deploy
```