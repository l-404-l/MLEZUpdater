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
            Console.WriteLine(new string('=', 70));
            Console.WriteLine("Grabbing loader updates..");

            var Client = new GitHubClient(new ProductHeaderValue("MLEZLoader"));
            var Latest = await Client.Repository.Release.GetLatest("l-404-l", "MLEZUpdater");
            var Asset = Latest.Assets.First(x => x.BrowserDownloadUrl.Contains("MLEZUpdaterBase.dll"));
            var Data = await Client.Connection.GetRaw(new Uri(Asset.BrowserDownloadUrl), null);

            try
            {
                Assembly.Load(Data.Body);

            } catch
            {
                Console.WriteLine("Failed to update loader.");
                Console.WriteLine("For support join our discord.");
                Console.WriteLine("Discord: https://discord.gg/PMmbwc2");
            }
            Console.WriteLine(new string('=', 70));
            Console.ResetColor();
            try
            {
                await MLEZUpdaterBase.Main.StartUpdating();
            } catch(Exception c)
            {
                Console.WriteLine("Failed to start loader.");
                Console.WriteLine("For support join our discord.");
                Console.WriteLine("Discord: https://discord.gg/PMmbwc2");
            }
        }
    }
}
