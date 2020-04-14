using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VTT_Translate
{
    class Program
    {
        static void Main(string[] args)
        {
            const string language = "fr"; // french
            const string path = @"C:\projects\VTT-Translate\files\";
            var inputFiles = Directory.GetFiles(path);
            foreach (var inputFile in inputFiles)
            {
                var outputFile = inputFile.Replace(".vtt", "-" + language + ".vtt");
                var strInput = File.ReadAllLines(inputFile);
                var fsOut = new StreamWriter(outputFile);
                var progress = 0;
                foreach (var line in strInput)
                {
                    progress++;
                    if (line == "WEBVTT" || line == "")
                    {
                        fsOut.WriteLine(line);
                        continue;
                    }
                    if (Regex.IsMatch(line, @"^\d+$"))
                    {
                        fsOut.WriteLine(line);
                        continue;
                    };
                    if (Regex.IsMatch(line, @"^\d\d:.*$"))
                    {
                        fsOut.WriteLine(line);
                        continue;
                    }
                    var translated = FromString(line, language);
                    fsOut.WriteLine(translated);
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("File:" + inputFile);
                    Console.WriteLine("Progress:" + progress + "/" + strInput.Length);
                }
                fsOut.Flush();
                fsOut.Close();
            }
            Console.WriteLine("Complete");
            Console.ReadLine();

        }



        private static string FromString(string english, string to)
        {
            const string host = "https://api.cognitive.microsofttranslator.com";
            var route = "/translate?api-version=3.0&from=en&to=" + to;
            var subscriptionKey = ConfigurationManager.AppSettings["subscription"];
            var body = new object[] { new { Text = english } };
            var requestBody = JsonConvert.SerializeObject(body);
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(host + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                var response = client.SendAsync(request).Result;
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var jResponse = JArray.Parse(jsonResponse);
                foreach (var translation in jResponse[0]["translations"])
                {
                    var strText = translation["text"].ToString();
                    return strText;
                }
            }
            return null;
        }
    }
}
