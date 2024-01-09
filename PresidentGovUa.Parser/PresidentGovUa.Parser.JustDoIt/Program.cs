using System.Formats.Asn1;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Text;
using CsvHelper;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using PresidentGovUa.Parser.Models;

string urlsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files/urls.csv");

var list = new List<UrlItem>();
using (var reader = new StreamReader(urlsPath))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    list = csv.GetRecords<UrlItem>().ToList();
}

Console.WriteLine($"Articles count: {list.Count()}");

var pages = new List<PageItem>();
using (IWebDriver driver = new EdgeDriver())
{
    try
    {
        foreach (var item in list)
        {
            driver.Navigate().GoToUrl(item.Origin);
            var title = driver.FindElement(By.XPath("//h1[@itemprop='name']")).Text;
            var date = driver.FindElements(By.XPath("//p[@class='date']")).Last().Text;
            var content = driver.FindElement(By.XPath("//div[@class='article_content']")).Text;

            pages.Add(new PageItem
            {
                Title = title,
                Date = date,
                Content = content
            });
        }
    }
    finally
    {
        driver.Quit();
    }
}



for (int i = 0; i < pages.Count; i++)
{
    var before = pages[i].Content.Length;
    var after = 0;

    pages[i].Content = pages[i].Content.Replace("!", "! ");
    pages[i].Content = pages[i].Content.Replace(".", ". ");


    while (before != after)
    {
        before = pages[i].Content.Length;
        pages[i].Content = pages[i].Content.Replace("  ", "");
        after = pages[i].Content.Length;
        Console.WriteLine($"before: {before} / after: {after}");
    }

    before = pages[i].Content.Length;
    after = 0;

    while (before != after)
    {
        before = pages[i].Content.Length;
        pages[i].Content = pages[i].Content.Replace("\n", "");
        after = pages[i].Content.Length;
        Console.WriteLine($"before: {before} / after: {after}");
    }

    before = pages[i].Content.Length;
    after = 0;

    while (before != after)
    {
        before = pages[i].Content.Length;
        pages[i].Content = pages[i].Content.Replace("\r", "");
        after = pages[i].Content.Length;
        Console.WriteLine($"before: {before} / after: {after}");
    }

    
}

var writer = new StreamWriter($"{DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss")}.json", false, Encoding.UTF8);

var options = new JsonSerializerOptions
{
    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
    WriteIndented = true
};

var jsonString = JsonSerializer.Serialize<IEnumerable<PageItem>>(pages, options);
writer.Write(jsonString);
writer.Flush();
writer.Close();

Console.WriteLine("DONE");
