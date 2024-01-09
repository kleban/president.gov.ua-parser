
using CsvHelper;
using CsvHelper.Configuration;
using PresidentGovUa.Parser.Models;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

string filesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files");

var list_1 = Directory.GetFiles(filesPath, "*.json").Select(x=> x.Substring(x.LastIndexOf(@"\")+1, x.Length-x.LastIndexOf(@"\")-1));

var list_2 = list_1.Select(x=> x.Replace(".json", ""));

var list_3 = list_2.Select(x => DateTime.ParseExact(x.Replace(".json", ""), "dd-MM-yyyy-HH-mm-ss", new CultureInfo("en")));

var filePath = Path.Combine(filesPath, $"{list_3.Max().ToString("dd-MM-yyyy-HH-mm-ss")}.json");

var options = new JsonSerializerOptions
{
    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
    WriteIndented = true
};

var data = JsonSerializer.Deserialize<List<PageItem>>(
    new StreamReader(filePath).ReadToEnd());

var data2 = data.Select(x => new PageItem2
{
    Content = x.Content,
    Title = x.Title,
    Time = x.Date.Split("-")[1].Trim(),
    Date = DateTime.Parse(string.Join(" ", x.Date.Split("-")[0].Trim().Split(" ").ToList().Take(3)), new CultureInfo("uk-UA")).ToString("dd.MM.yyyy")
}).ToList();
/*
var replaceCodes = new List<KeyValuePair<string, string>>
{
    new KeyValuePair<string, string>("\u2013", "-"),
    new KeyValuePair<string, string>("\u2014", "-"),
    new KeyValuePair<string, string>("\u2015", "-"),
    new KeyValuePair<string, string>("\u2019", "'"),
    new KeyValuePair<string, string>("\u2019", "'"),

}*/

var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    ShouldQuote = args => true
};

using (var writer = new StreamWriter(Path.Combine(filesPath, "pg_ua.csv"), append: false, encoding: Encoding.UTF8))
using (var csv = new CsvWriter(writer, config))
{
    csv.WriteRecords(data2);    
}






