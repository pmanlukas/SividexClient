using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;

namespace VideoIndexerClient
{
    class Program
    {
        //Defines an Upload Call for the VideoIndexer API
        struct VideoIndexerUpload
        {
            public string ocpKey, name, privacy, filePath, fileName;

            public VideoIndexerUpload(string key, string name, string privacy, string path, string nameFile)
            {
                this.ocpKey = key;
                this.name = name;
                this.privacy = privacy;
                this.filePath = path;
                this.fileName = nameFile;
            }
        }

        //Can be used to store a breakdown from the VideoIndexer API
        struct Breakdown
        {
            public string id;
            public HttpResponseMessage response;
            public string contents;

            public Breakdown(string id)
            {
                this.id = id;
                this.response = new HttpResponseMessage();
                this.contents = "";
            }
        }

        static void Main(string[] args)
        {
            bool isRunning = true;


            VideoIndexerUpload upload = new VideoIndexerUpload("{apiKey}",
                "{NameUpload}",
                "Private", "{pathToFile}", "{NameFile}");


            while (isRunning)
            {
                Console.WriteLine();
                Console.WriteLine("Enter q to finish, u for upload,b for breakdown or enter w for a widget url!");
                var input = Console.ReadLine();
                if (input == "q")
                {
                    isRunning = false;
                }
                else if (input == "u")
                {
                    Console.WriteLine("Hit enter to start Upload!");
                    Console.ReadKey();

                    Console.WriteLine("Upload started!");
                    MakeRequest(upload).Wait();
                }
                else if (input == "b")
                {
                    var idBreakdown = GetIdValue();
                    GetBreakdown(idBreakdown).Wait();
                    Console.ReadKey();
                }
                else if (input == "w")
                {
                    var idWidget = GetIdValue();
                    GetBreakdownUrl(idWidget).Wait();
                    Console.ReadKey();
                }
            }
        }

        //Used to send a Post Request to VideoInderAPI and returns id of breakdown
        static async Task MakeRequest(VideoIndexerUpload uploadRequest)
        {
            //Define Request body


            using (var client = new HttpClient())
            {
                var content = new MultipartFormDataContent();

                content.Add(new StreamContent(File.Open(uploadRequest.filePath, FileMode.Open)), uploadRequest.name,
                    uploadRequest.fileName);


                client.Timeout = TimeSpan.FromMinutes(30);

                var queryString = HttpUtility.ParseQueryString(string.Empty);

                //Define Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", uploadRequest.ocpKey);


                //Define Request parameters
                queryString["name"] = uploadRequest.name;
                queryString["privacy"] = uploadRequest.privacy;
                var uri = $"https://videobreakdown.azure-api.net/Breakdowns/Api/Partner/Breakdowns?{queryString}";

                HttpResponseMessage response;

                try
                {
                    response = await client.PostAsync(uri, content);
                    Console.WriteLine(response.ToString());
                    Console.WriteLine("Parsing response body...");
                    Console.WriteLine("The Id is: ");
                    await ParseJson(response);
                }
                catch (Exception e)
                {
                    Console.WriteLine("There was an Error with your Upload!");
                    Console.WriteLine(e);
                }
            }
        }

        //Used to send a Get Request to VideoInderAPI and returns breakdown json
        static async Task GetBreakdown(string id)
        {
            using (var client = new HttpClient())
            {
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "{apiKey}");

                // Request parameters
                queryString["language"] = "English";
                var uri = $"https://videobreakdown.azure-api.net/Breakdowns/Api/Partner/Breakdowns/{id}?{queryString}";

                var response = await client.GetAsync(uri);
                Console.WriteLine(response.ToString());
                await ParseJson(response);
            }
        }

        static async Task<string> ParseJson(HttpResponseMessage res)
        {
            var contents = await res.Content.ReadAsStringAsync();
            Console.Write(contents.ToString());
            return contents;
        }

        static async Task GetBreakdownUrl(string id)
        {
            using (var client = new HttpClient())
            {
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "{apiKey}");

                // Request parameters
                //queryString["widgetType"] = "{string}";
                //queryString["allowEdit"] = "true";
                var uri =
                    $"https://videobreakdown.azure-api.net/Breakdowns/Api/Partner/Breakdowns/{id}/InsightsWidgetUrl?" +
                    queryString;

                var response = await client.GetAsync(uri);

                Console.WriteLine(response.ToString());
                await ParseJson(response);
            }
        }

        static string GetIdValue()
        {
            Console.WriteLine("Enter Breakdown");
            var id = Console.ReadLine();
            return id;
        }
    }
}