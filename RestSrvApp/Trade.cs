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

    // ���O�t�@�C���ɏ����o��
    app.Logger.LogInformation("ExitEntry");

    app.Logger.LogInformation(body);
    // JSON�������JObject�ɕϊ�
    var json = Newtonsoft.Json.Linq.JObject.Parse(body);

    // "side"�v���p�e�B�̒l���擾
    string side = json["side"]!.ToString();
    app.Logger.LogInformation($"side:  = {side}");

    // "futureCode"�̒l���擾
    string futureCode = json["futureCode"]!.ToString();
    app.Logger.LogInformation($"futureCode:  = {futureCode}");

    // "derivMonth"�̒l���擾
    int derivMonth = (int)json["derivMonth"]!.ToObject<int>(); ;
    app.Logger.LogInformation($"derivMonth:  = {derivMonth}");

    // "qty"�̒l���擾 �w�肪�Ȃ�������֐�����擾
    int qty = (int)json["qty"]!.ToObject<int>();
    app.Logger.LogInformation($"qty:  = {qty}");

    string symbol = "";

    try
    {
        Monitor.Enter(app);

        // �g�[�N���擾
        string token = GenerateToken.GetToken();

        // �����R�[�h�擾
        dynamic SymbolFutureNameResponseObject = Kabusapi_Symbolname_Future.SymbolNameFuture(
            token: token, FutureCode: futureCode, DerivMonth: derivMonth);

        // �V���{��
        symbol = SymbolFutureNameResponseObject.Symbol;

        // ��������Ԃ�
        int exchange = GetExChange();

        // �|�W�V�������������猈��
        Exit(token, symbol, exchange, side);


        // �G���g��
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

    // ���O�t�@�C���ɏ����o��
    app.Logger.LogInformation("ExitOnly");

    app.Logger.LogInformation(body);
    // JSON�������JObject�ɕϊ�
    var json = Newtonsoft.Json.Linq.JObject.Parse(body);

    // "side"�v���p�e�B�̒l���擾
    string side = json["side"]!.ToString();
    app.Logger.LogInformation($"side:  = {side}");

    // "futureCode"�̒l���擾
    string futureCode = json["futureCode"]!.ToString();
    app.Logger.LogInformation($"futureCode:  = {futureCode}");

    // "derivMonth"�̒l���擾
    int derivMonth = (int)json["derivMonth"]!.ToObject<int>(); ;
    app.Logger.LogInformation($"derivMonth:  = {derivMonth}");

    // "qty"�̒l���擾 �w�肪�Ȃ�������0
    int qty = json["qty"]?.ToObject<int>() ?? 0;
    app.Logger.LogInformation($"qty:  = {qty}");    

    try
    {
        Monitor.Enter(app);

        // �g�[�N���擾
        string token = GenerateToken.GetToken();

        // �����R�[�h�擾
        dynamic SymbolFutureNameResponseObject = Kabusapi_Symbolname_Future.SymbolNameFuture(
            token: token, FutureCode: futureCode, DerivMonth: derivMonth);

        // ��������Ԃ�
        int exchange = GetExChange();

        // �|�W�V�������������猈��
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
    // ��������Ԃ�

    // ���ݎ������擾
    DateTime currentTime = DateTime.Now;

    // ���ݎ������ǂ̎��ԑтɊY�����邩����
    int ExChangeTemp = 24; // ���
    if (currentTime.TimeOfDay >= new TimeSpan(8, 0, 0) && currentTime.TimeOfDay <= new TimeSpan(15, 15, 0))
    {
        ExChangeTemp = 23; // ����
    }

    return ExChangeTemp;

};

void Exit(string token, string symbol, int exchange, string side)
{
    var opositSide = (side == "1") ? "2" : "1";

    // �c���Ɖ�
    List<dynamic> PositionsResponseObjectList = Kabusapi_Positions.Positions(
        token, product: "3", symbol, opositSide, addinfo: "false");

    // �c���ɔ���W�V���������邩 
    int totalPosition = PositionsResponseObjectList.Sum(item => item.LeavesQty);

    // �|�W�V����������Ό���
    if (totalPosition > 0)
    {
        
        // ����

        var exitObj = new
        {
            Password = Constants.LoginPassword,
            Symbol = symbol,
            Exchange = exchange, // �s��R�[�h(���� or ���)
            TradeType = 2, // �ԍ�
            TimeInForce = 2, // FAK
            Qty = totalPosition,
            Side = side, // // ���� or ����
            ClosePositionOrder = 0, // ���t�i�Â����j�A���v�i�������j
            FrontOrderType = 120, // ���s
            Price = 0,
            ExpireDay = 0
        };

        dynamic SendOrderFutureResponseExit = Kabusapi_Sendorder_Future_new.SendOrderFuture(token, exitObj);

    }
}

void Entry(string token, string symbol, int exchange, string side, int qty)
{

    // �G���g��
    var obj = new
    {
        Password = Constants.LoginPassword,
        Symbol = symbol,
        Exchange = exchange, // �s��R�[�h(���� or ���),
        TradeType = 1,
        TimeInForce = 2,
        Side = side, // ���� or ����
        Qty = qty,
        FrontOrderType = 120, // ���s
        Price = 0,
        ExpireDay = 0
    };

    dynamic SendOrderFutureResponseObject = Kabusapi_Sendorder_Future_new.SendOrderFuture(token, obj);
}

app.Run();
