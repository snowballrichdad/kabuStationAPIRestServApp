using CSharp_sample;
using System.Text;
using Microsoft.Extensions.Logging;
using Serilog;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Reflection.PortableExecutable;
using LiteDB;
using System.Diagnostics;
using TaskScheduler;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);


builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureHttpsDefaults(listenOptions =>
    {
        listenOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
    });
});
var app = builder.Build();

app.UseCertificateForwarding();

app.UseAuthentication();

app.MapPost("/ExitEntry", async (HttpContext context) => {
    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
    var body = await reader.ReadToEndAsync();

    // ログファイルに書き出す
    app.Logger.LogInformation("ExitEntry");

    app.Logger.LogInformation(body);
    // JSON文字列をJObjectに変換
    var json = Newtonsoft.Json.Linq.JObject.Parse(body);

    // "side"プロパティの値を取得
    string side = json["side"]!.ToString();
    app.Logger.LogInformation($"side:  = {side}");

    // "futureCode"の値を取得
    string futureCode = json["futureCode"]!.ToString();
    app.Logger.LogInformation($"futureCode:  = {futureCode}");

    // "derivMonth"の値を取得
    int derivMonth = (int)json["derivMonth"]!.ToObject<int>(); ;
    app.Logger.LogInformation($"derivMonth:  = {derivMonth}");

    // "qty"の値を取得 指定がなかったら関数から取得
    int qty = (int)json["qty"]!.ToObject<int>();
    app.Logger.LogInformation($"qty:  = {qty}");

    string symbol = "";

    try
    {
        Monitor.Enter(app);

        // トークン取得
        string token = GenerateToken.GetToken();

        // 銘柄コード取得
        dynamic SymbolFutureNameResponseObject = Kabusapi_Symbolname_Future.SymbolNameFuture(
            token: token, FutureCode: futureCode, DerivMonth: derivMonth);

        // シンボル
        symbol = SymbolFutureNameResponseObject.Symbol;

        // 日中か夜間か
        int exchange = GetExChange();

        // ポジションがあったら決済
        Exit(token, symbol, exchange, side);


        // エントリ
        Entry(token, symbol, exchange, side, qty);

        entried = true;

    }
    catch (HttpRequestException e)
    {
        app.Logger.LogInformation("{a} {b}", e, e.Message);
    }
    catch (Exception ex)
    {
        app.Logger.LogInformation("{a} {b}", ex, ex.Message);
        ;
    }
    finally
    {
        Monitor.Exit(app);
    }

    return "Hello World!";

});

app.MapPost("/ExitOnly", async (HttpContext context) => {
    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
    var body = await reader.ReadToEndAsync();

    // ログファイルに書き出す
    app.Logger.LogInformation("ExitOnly");

    app.Logger.LogInformation(body);
    // JSON文字列をJObjectに変換
    var json = Newtonsoft.Json.Linq.JObject.Parse(body);

    // "side"プロパティの値を取得
    string side = json["side"]!.ToString();
    app.Logger.LogInformation($"side:  = {side}");

    // "futureCode"の値を取得
    string futureCode = json["futureCode"]!.ToString();
    app.Logger.LogInformation($"futureCode:  = {futureCode}");

    // "derivMonth"の値を取得
    int derivMonth = (int)json["derivMonth"]!.ToObject<int>(); ;
    app.Logger.LogInformation($"derivMonth:  = {derivMonth}");

    // "qty"の値を取得 指定がなかったら0
    int qty = json["qty"]?.ToObject<int>() ?? 0;
    app.Logger.LogInformation($"qty:  = {qty}");    

    try
    {
        Monitor.Enter(app);

        // トークン取得
        string token = GenerateToken.GetToken();

        // 銘柄コード取得
        dynamic SymbolFutureNameResponseObject = Kabusapi_Symbolname_Future.SymbolNameFuture(
            token: token, FutureCode: futureCode, DerivMonth: derivMonth);

        // 日中か夜間か
        int exchange = GetExChange();

        // ポジションがあったら決済
        Exit(token, SymbolFutureNameResponseObject.Symbol, exchange, side, qty);

    }
    catch (HttpRequestException e)
    {
        app.Logger.LogInformation("{a} {b}", e, e.Message);
    }
    catch (Exception ex)
    {
        app.Logger.LogInformation("{a} {b}", ex, ex.Message);
        ;
    }
    finally
    {
        Monitor.Exit(app);
    }

    return "Hello World!";

});

int GetExChange ()
{
    // 日中か夜間か

    // 現在時刻を取得
    DateTime currentTime = DateTime.Now;

    // 現在時刻がどの時間帯に該当するか判定
    int ExChangeTemp = 24; // 夜間
    if (currentTime.TimeOfDay >= new TimeSpan(8, 0, 0) && currentTime.TimeOfDay <= new TimeSpan(15, 15, 0))
    {
        ExChangeTemp = 23; // 日中
    }

    return ExChangeTemp;

};

void Exit(string token, string symbol, int exchange, string side)
{
    var opositSide = (side == "1") ? "2" : "1";

    // 残高照会
    List<dynamic> PositionsResponseObjectList = Kabusapi_Positions.Positions(
        token, product: "3", symbol, opositSide, addinfo: "false");

    // 残高に売りジションがあるか 
    int totalPosition = PositionsResponseObjectList.Sum(item => item.LeavesQty);

    // ポジションがあれば決済
    if (totalPosition > 0)
    {
        
        // 決済

        var exitObj = new
        {
            Password = Constants.LoginPassword,
            Symbol = symbol,
            Exchange = exchange, // 市場コード(日中 or 夜間)
            TradeType = 2, // 返済
            TimeInForce = 2, // FAK
            Qty = totalPosition,
            Side = side, // // 売り or 買い
            ClosePositionOrder = 0, // 日付（古い順）、損益（高い順）
            FrontOrderType = 120, // 成行
            Price = 0,
            ExpireDay = 0
        };

        dynamic SendOrderFutureResponseExit = Kabusapi_Sendorder_Future_new.SendOrderFuture(token, exitObj);

    }
}

void Entry(string token, string symbol, int exchange, string side, int qty)
{

    // エントリ
    var obj = new
    {
        Password = Constants.LoginPassword,
        Symbol = symbol,
        Exchange = exchange, // 市場コード(日中 or 夜間),
        TradeType = 1,
        TimeInForce = 2,
        Side = side, // 売り or 買い
        Qty = qty,
        FrontOrderType = 120, // 成行
        Price = 0,
        ExpireDay = 0
    };

    dynamic SendOrderFutureResponseObject = Kabusapi_Sendorder_Future_new.SendOrderFuture(token, obj);
}

app.Run();
