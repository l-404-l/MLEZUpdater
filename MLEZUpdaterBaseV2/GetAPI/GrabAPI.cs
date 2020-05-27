using Ionic.Zip;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLEZUpdaterBase
{
    public static class GrabAPI
    {
        public static GitHubClient Client = new GitHubClient(new ProductHeaderValue("MLEZUpdater"));
        
        public static async Task<byte[]> DownloadFilesContentAsync(string Author, string Repo, string FileName)
        {
            var Contents = await Client.Repository.Content.GetAllContents(Author, Repo, FileName);
            var File = Contents.First();
            var Data = await Client.Connection.GetRaw(new Uri(File.DownloadUrl), null);
            return Data.Body;
        }
        public static async Task<byte[]> DownloadFileAsync(string Author, string Repo, string FileName)
        {
            var Latest = await Client.Repository.Release.GetLatest(Author, Repo);
            var Asset = Latest.Assets.First(x => x.BrowserDownloadUrl.Contains(FileName));
            var Data = await Client.Connection.GetRaw(new Uri(Asset.BrowserDownloadUrl), null);
            return Data.Body;
        }
        public static async Task<byte[]> DownloadFileAsync(string Author, string Repo, Func<ReleaseAsset,bool> Check)
        {
            var Latest = await Client.Repository.Release.GetLatest(Author, Repo);
            var Asset = Latest.Assets.First(Check);
            var Data = await Client.Connection.GetRaw(new Uri(Asset.BrowserDownloadUrl), null);
            return Data.Body;
        }
        public static async Task<ZipFile> DownloadAndUnZip(string Author, string Repo, string FileName)
        {
            var bytes = await DownloadFileAsync(Author, Repo, FileName);
            var file = new MemoryStream(bytes);
            file.Seek(0, SeekOrigin.Begin);
            return ZipFile.Read(file);
        }
        public static async Task<ZipFile> DownloadAndUnZip(string Author, string Repo, Func<ReleaseAsset, bool> Check)
        {
            var bytes = await DownloadFileAsync(Author, Repo, Check);
            var file = new MemoryStream(bytes);
            file.Seek(0, SeekOrigin.Begin);
            return ZipFile.Read(file);
        }

        public static void DeleteAllFiles(string direc)
        {
            if (Directory.Exists(direc))
            {
                string[] files = Directory.GetFiles(direc);
                string[] dirs = Directory.GetDirectories(direc);

                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                foreach (string dir in dirs)
                {
                    DeleteAllFiles(dir);
                }

                Directory.Delete(direc, false);
            }
        }

        public static string FindMetaData()
        {
            foreach(var driec in Directory.GetDirectories(Directory.GetCurrentDirectory()))
            {
                if (driec.ToLower().Contains("_data") && Directory.Exists(driec +"/il2cpp_data"))
                {
                    return driec + "\\il2cpp_data\\Metadata\\global-metadata.dat";
                }
            }
            throw new Exception("MetaData not found");
            return "NOT FOUND";
        }

        public static async Task WaitForProcess(Process app)
        { 
            while(Process.GetProcesses().Any(x => x.Id == app.Id))
            {
                await Task.Delay(25);
            }
        }
    }
}
