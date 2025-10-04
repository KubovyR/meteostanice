using System.Xml;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

class WeatherControl
{
    string url;
    HttpClient client = new HttpClient();
    DateTime lastUpdateTime;
    TimeSpan resetInterval = TimeSpan.FromMinutes(60);
    bool cancel = false;
    string dbFileLoc;
    string connStr;

    public WeatherControl(string url)
    {
        this.url = url;

        dbFileLoc = Directory.GetCurrentDirectory() + "\\mydb.mdf";
        connStr = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={dbFileLoc};Integrated Security=True;Connect Timeout=30";
    }

    public async Task Do()
    {
        XmlDocument document = new XmlDocument();

        while (!cancel)
        {
            do
            {
                if ((DateTime.Now - lastUpdateTime) >= resetInterval)
                {
                    try
                    {
                        using HttpResponseMessage response = await client.GetAsync(this.url);
                        response.EnsureSuccessStatusCode();

                        var xml = await response.Content.ReadAsStringAsync();
                        document.LoadXml(File.ReadAllText(xml));

                        string json = JsonConvert.SerializeXmlNode(document, Newtonsoft.Json.Formatting.Indented, true);

                        NewItem(DateTime.Now, json, true);

                    }
                    catch (Exception ex)
                    {
                        switch (ex.InnerException)
                        {
                            case HttpRequestException:
                                NewItem(DateTime.Now, "XML SERVER UNAVAILABLE", false);
                                break;
                            default:
                                Console.WriteLine("ERROR:\n" + ex.InnerException + "\n");
                                cancel = true;
                                break;
                        }
                    }
                    lastUpdateTime = DateTime.Now;
                }
            } while (!Console.KeyAvailable);
            if (Console.ReadKey().Key == ConsoleKey.Escape) cancel = true;
        }
    }

    private void NewItem(DateTime downTime, string json, bool ok)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand("INSERT INTO weather (downtime, json, ok) VALUES (@date,@json,@ok)", conn))
            {
                cmd.Parameters.Add("@date", System.Data.SqlDbType.DateTime).Value = downTime;
                cmd.Parameters.Add("@json", System.Data.SqlDbType.Text).Value = json;
                cmd.Parameters.Add("@ok", System.Data.SqlDbType.Bit).Value = ok;
                cmd.ExecuteNonQuery();
            }
        }
    }

    static void Main(string[] args)
    {
        var wController = new WeatherControl(args[0]);
        wController.Do();
        while (!wController.cancel) System.Threading.Thread.Sleep(500);
    }

}