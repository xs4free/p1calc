using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var appId = config["APSystems:AppId"];
var appSecret = config["APSystems:AppSecret"];
var sid = config["ApSystems:SID"];

// https://file.apsystemsema.com:8083/apsystems/resource/openapi/Apsystems_OpenAPI_User_Manual_End_User_EN.pdf
var baseUrl = "https://api.apsystemsema.com:9282";

var requestPath = $"/user/api/v2/systems/details/{sid}";
var response = await MakeRequest(appId, appSecret, baseUrl, requestPath, "GET");

Console.WriteLine(response);

static string CalculateSignature(string appSecret, string stringToSign)
{
    var appSecretBytes = Encoding.UTF8.GetBytes(appSecret);
    
    var sha256 = new HMACSHA256(appSecretBytes);
    var md5Result = sha256.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));

    return Convert.ToBase64String(md5Result);
}

static async Task<string> MakeRequest(string appId, string appSecret, string baseUrl, string requestPath, string httpMethod)
{
    var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
    var nonce = Guid.NewGuid().ToString("N");
    string signatureMethod = "HmacSHA256";
    string stringToSign = timeStamp + "/" + nonce + "/" + appId + "/" + requestPath + "/" + httpMethod + "/" + signatureMethod;
    string signature = CalculateSignature(appSecret, stringToSign);

    using var client = new HttpClient();
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("X-CA-AppId", appId);
    client.DefaultRequestHeaders.Add("X-CA-Timestamp", timeStamp);
    client.DefaultRequestHeaders.Add("X-CA-Nonce", nonce);
    client.DefaultRequestHeaders.Add("X-CA-Signature-Method", signatureMethod);
    client.DefaultRequestHeaders.Add("X-CA-Signature", signature);

    var response = await client.GetAsync(baseUrl + requestPath);

    return await response.Content.ReadAsStringAsync();
}