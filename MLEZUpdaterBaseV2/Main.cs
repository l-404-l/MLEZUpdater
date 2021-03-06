﻿using Ionic.Zip;
using Newtonsoft.Json;
using SevenZip;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MLEZUpdaterBase
{
    public class Main
    {

        public static async Task StartUpdating()
        {
            var ODirc = Directory.GetCurrentDirectory();
            var metadata = GrabAPI.FindMetaData();

            GrabAPI.DeleteAllFiles(ODirc + "\\UnHollower");
            if (File.Exists(ODirc + "/winmm.dll"))
                File.Delete(ODirc + "/winmm.dll");
            GrabAPI.DeleteAllFiles(ODirc + "\\MelonLoader");
            GrabAPI.DeleteAllFiles(ODirc + "\\IL2CPPDumper");
            GrabAPI.DeleteAllFiles(ODirc + "\\ExtraFiles");

            Directory.CreateDirectory(ODirc + "/UnHollower/UnityDepends");
            Directory.CreateDirectory(ODirc + "/IL2CPPDumper");
            Directory.CreateDirectory(ODirc + "/ExtraFiles");

            Console.ResetColor();
            Console.WriteLine(new string('=', 70));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Finding Game Version..");
            var find = File.ReadAllLines(ODirc + "/GameAssembly.dll");
            var version = "";
            var depends = new byte[0];
            var foundDepends = false;
            foreach (var t in find)
            {
                if (t.Contains("\\Unity\\") && t.Contains("\\Editor\\Data"))
                {
                    var thesplit = t.Substring(t.IndexOf("\\Unity\\")).Split('\\');
                    foreach(var strngs in thesplit)
                    {
                        if (strngs.Contains("20"))
                        {
                            version = strngs;
                            break;
                        }
                    }
                    break;
                }
            }
            if (!string.IsNullOrEmpty(version))
            {
                Console.WriteLine($"Found Version: {version}");
                Console.Title = Console.Title + $" Version: {version}";
            }
            else
            {
                Console.WriteLine($"Version Not Found.");
                Console.Title += $" Version: Unknown";
            }
            var Contents = await GrabAPI.Client.Repository.Content.GetAllContents("HerpDerpinstine", "MelonLoader", "BaseLibs/Unity Dependencies");
            foreach (var entires in Contents)
            {
                if (entires.Name.Contains(version))
                {
                    var found = await GrabAPI.Client.Connection.GetRaw(new Uri(entires.DownloadUrl), null);
                    depends = found.Body;
                }
            }
            if (depends.Length > 0)
            {
                var dependzip = new MemoryStream(depends);
                dependzip.Seek(0, SeekOrigin.Begin);
                ZipFile.Read(dependzip).ExtractAll(ODirc + "/UnHollower/UnityDepends", ExtractExistingFileAction.OverwriteSilently);
                foundDepends = true;
            }


            Console.ResetColor();
            Console.WriteLine(new string('=', 70));
            await Task.Delay(2000);

            Console.WriteLine(new string('=', 70));
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("MelonLoader Downloading..");

            var MelonLoader = await GrabAPI.DownloadAndUnZip("HerpDerpinstine", "MelonLoader", "MelonLoader.zip");
            MelonLoader.ExtractAll(ODirc, ExtractExistingFileAction.OverwriteSilently);
            
            Console.WriteLine("Extracted MelonLoader!");
            Console.ResetColor();
            Console.WriteLine(new string('=', 70));

            await Task.Delay(2000);
            Console.Clear();

            Console.ResetColor();
            Console.WriteLine(new string('=', 70));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Grabbing IL2CPP Dumper..");
            Directory.SetCurrentDirectory(ODirc + "\\IL2CPPDumper");
            var Il2CPPDumper = await GrabAPI.DownloadAndUnZip("Perfare", "Il2CppDumper", x => !x.BrowserDownloadUrl.Contains("-netcore-") && x.BrowserDownloadUrl.Contains("Il2CppDumper"));
            Il2CPPDumper.ExtractAll(Directory.GetCurrentDirectory(), ExtractExistingFileAction.OverwriteSilently);

            File.WriteAllText("config.json", JsonConvert.SerializeObject(new il2cppdumpConfig()));
            var pros = new ProcessStartInfo(Directory.GetCurrentDirectory() + "\\Il2CppDumper.exe");
            pros.ArgumentList.Add($"{ODirc}\\GameAssembly.dll");
            pros.ArgumentList.Add($"{metadata}");
            var il2cppdp = Process.Start(pros);
            await GrabAPI.WaitForProcess(il2cppdp);

            Console.ResetColor();
            Console.WriteLine(new string('=', 70));
            Thread.Sleep(2000);
            Console.Clear();

            Console.ResetColor();
            Console.WriteLine(new string('=', 70));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Grabbing Unhollower..");
            Directory.SetCurrentDirectory(ODirc + "\\UnHollower");

            if (!File.Exists("7z.dll"))
                File.WriteAllBytes("7z.dll", Properties.Resources._7z);
            SevenZipBase.SetLibraryPath(Directory.GetCurrentDirectory() + "/7z.dll");

            var rlist = await GrabAPI.Client.Repository.Release.GetAll("knah", "Il2CppAssemblyUnhollower");
            var reallist = rlist.OrderByDescending(x => x.PublishedAt).ToList();
            Console.WriteLine("Select Version:");
            foreach (var vrs in reallist)
            {
                Console.WriteLine(reallist.FindIndex(x => x == vrs) + ": Name: " + vrs.Name);
            }
            int value = 1;
            try
            {
                value = Convert.ToInt32(Console.ReadLine());
            } catch
            {
                Console.WriteLine("Invalid Number Defaulting to 1");
            }
            if (value > reallist.Count)
                value = 0;
            var item = reallist[value];
            foreach (var itm in item.Assets)
                Console.WriteLine(itm.BrowserDownloadUrl);
            var URL = item.Assets.First(x => x.BrowserDownloadUrl.Contains("Il2CppAssemblyUnhollower"));
            Console.WriteLine("Grabbing Unhollower..");
            var UnHollower = await GrabAPI.Client.Connection.GetRaw(new Uri(URL.BrowserDownloadUrl), null);
            var memeory = new MemoryStream(UnHollower.Body);
            memeory.Seek(0, SeekOrigin.Begin);
            using (var temp = new SevenZipExtractor(memeory))
            {
                for (int i = 0; i < temp.ArchiveFileData.Count; i++)
                {
                    temp.ExtractFiles(Directory.GetCurrentDirectory(), temp.ArchiveFileData[i].Index);
                }
            }
            
            var pros2 = new ProcessStartInfo(Directory.GetCurrentDirectory() + "\\AssemblyUnhollower.exe");
            pros2.ArgumentList.Add($"--input={ODirc + "\\IL2CPPDumper\\DummyDll"}");
            pros2.ArgumentList.Add($"--output={ODirc + "\\MelonLoader\\Managed"}");
            pros2.ArgumentList.Add($"--mscorlib={ODirc + "\\MelonLoader\\Managed\\mscorlib.dll"}");
            if (foundDepends)
                pros2.ArgumentList.Add($"--unity={ODirc + "/UnHollower/UnityDepends"}");
            var unhollowp = System.Diagnostics.Process.Start(pros2);
            await GrabAPI.WaitForProcess(unhollowp);
            
            
            Console.ResetColor();
            Console.WriteLine(new string('=', 70));
            await Task.Delay(2000);
            Console.Clear();
            Console.ResetColor();
            Console.WriteLine(new string('=', 70));


            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Fixing your files now..");
            var ExtraFiles = await GrabAPI.DownloadFilesContentAsync("l-404-l", "MLEZUpdater", "ExtraFiles");
            if (ExtraFiles.Length > 0)
            {
                var mem = new MemoryStream(ExtraFiles);
                ZipFile.Read(mem).ExtractAll(ODirc + "/ExtraFiles", ExtractExistingFileAction.OverwriteSilently);
                if (Directory.Exists(ODirc + "/ExtraFiles"))
                {
                    Console.WriteLine("Loading Extras..");
                    foreach (var file in Directory.GetFiles(ODirc + "/ExtraFiles"))
                    {
                        File.Copy(file, ODirc + "/MelonLoader/Managed/" + Path.GetFileName(file), true);
                    }
                }
            }
            Directory.SetCurrentDirectory(ODirc);
            GrabAPI.DeleteAllFiles(ODirc + "\\ExtraFiles");
            GrabAPI.DeleteAllFiles(ODirc + "\\UnHollower");
            GrabAPI.DeleteAllFiles(ODirc + "\\IL2CPPDumper");
            Console.WriteLine("All Done!!");


            Console.ResetColor();
            Console.WriteLine(new string('=', 70));
            await Task.Delay(2000);
        }


    }
}
