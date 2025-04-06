using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var fileName = @"c:\temp\output.csv";
List<string> lines = ["date,energy"];
var date = new DateOnly(2024, 01, 01);

try
{
    for (int i = 0; i <= 365; i++)
    {
        var currentDate = date.AddDays(i);
        var response = await MakeRequest(currentDate, config);
        if (response == null)
        {
            return;
        }

        for (int hour = 0; hour < 24; hour++)
        {
            lines.Add($"{currentDate.Year}-{currentDate.Month:D2}-{currentDate.Day:D2} {hour:D2}:00:00,{response.data[hour]}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}

foreach (var line in lines)
{
    Console.WriteLine();
}
File.AppendAllLines(fileName, lines);

static string CalculateSignature(string appSecret, string stringToSign)
{
    var appSecretBytes = Encoding.UTF8.GetBytes(appSecret);
    
    var sha256 = new HMACSHA256(appSecretBytes);
    var md5Result = sha256.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));

    return Convert.ToBase64String(md5Result);
}

static async Task<APSEnergyHourlyResponse?> MakeRequest(DateOnly date, IConfigurationRoot config)
{
    var appId = config["APSystems:AppId"]; // can be found at: https://apsystemsema.com/apsystems/web/setting/personalSetting/openAPIService
    var appSecret = config["APSystems:AppSecret"]; // can be found at: https://apsystemsema.com/apsystems/web/setting/personalSetting/openAPIService
    var sid = config["ApSystems:SID"]; // can be found at: https://apsystemsema.com/apsystems/web/setting/personalSetting/accountDetail

    // https://file.apsystemsema.com:8083/apsystems/resource/openapi/Apsystems_OpenAPI_User_Manual_End_User_EN.pdf
    // https://gathering.tweakers.net/forum/view_message/81231762
    var baseUrl = "https://api.apsystemsema.com:9282";
    var requestPath = $"/user/api/v2/systems/{sid}/energy?energy_level=hourly&date_range={date.Year}-{date.Month:D2}-{date.Day:D2}";

    var timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();
    var nonce = Guid.NewGuid().ToString("N");
    string signatureMethod = "HmacSHA256";
    string stringToSign = $"{timeStamp}/{nonce}/{appId}/energy/GET/{signatureMethod}";
    string signature = CalculateSignature(appSecret, stringToSign);

    using var client = new HttpClient();
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("X-CA-AppId", appId);
    client.DefaultRequestHeaders.Add("X-CA-Timestamp", timeStamp.ToString());
    client.DefaultRequestHeaders.Add("X-CA-Nonce", nonce);
    client.DefaultRequestHeaders.Add("X-CA-Signature-Method", signatureMethod);
    client.DefaultRequestHeaders.Add("X-CA-Signature", signature);

    var response = await client.GetAsync(baseUrl + requestPath);
    var content = await response.Content.ReadAsStringAsync();

    return JsonSerializer.Deserialize<APSEnergyHourlyResponse>(content);
}

class APSEnergyHourlyResponse
{
    public long code { get; set; }
    public IList<string> data { get; set; }
}