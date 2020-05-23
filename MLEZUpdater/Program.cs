using Octokit;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MLEZUpdater
{
    class Program
    {
        static void Main(string[] args)
        {

            new Thread(async () => {

                await FetchUpdater();

            }).Start();
            Thread.Sleep(-1);
        }

        public static async Task FetchUpdater()
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Title = "MLEZUpdater - By: 404#0004 ";
            Console.WriteLine(new string('=', 70));
            Console.WriteLine("Grabbing loader updates..");

            var Client = new GitHubClient(new ProductHeaderValue("MLEZLoader"));
            var Latest = await Client.Repository.Release.GetLatest("l-404-l", "MLEZUpdater");
            var Asset = Latest.Assets.First(x => x.BrowserDownloadUrl.Contains("MLEZUpdaterBase.dll"));
            var Data = await Client.Connection.GetRaw(new Uri(Asset.BrowserDownloadUrl), null);
            Assembly value = null;
            try
            {
                value = Assembly.Load(Data.Body);

            } catch (Exception c)
            {
                Console.WriteLine("Failed to update loader.");
                Console.WriteLine("For support join our discord.");
                Console.WriteLine("Discord: https://discord.gg/PMmbwc2");
                Console.WriteLine();
                Console.WriteLine("Error:");
                Console.WriteLine(c);
            }
            Console.WriteLine(new string('=', 70));
            Console.ResetColor();
            try
            {
                if (value != null)
                {
                    var Task = (Task)value.GetType("MLEZUpdaterBase.Main").GetMethod("StartUpdating").Invoke(null, null);
                    await Task;
                }
            } catch(Exception c)
            {
                Console.WriteLine("Failed to complete loading.");
                Console.WriteLine("For support join our discord.");
                Console.WriteLine("Discord: https://discord.gg/PMmbwc2");
                Console.WriteLine();
                Console.WriteLine("Error:");
                Console.WriteLine(c);
            }
        }
    }
}
