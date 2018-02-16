using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using AutoJunkApi;
using static System.IO.File;

namespace AutoJunk
{
    public static class Constants
    {
        public const string FolderName = "autojunk_plugins";
    }

    internal class Program
    {

        private static string _vers = "0.5.9_DEVBUILD 27 (16.2.2018)";
        private static string _versCpp = "0.5.9_DEVBUILD 27 (16.2.2018)";
        private static string _versCsh = "0.5.9_DEVBUILD 27 (16.2.2018)";
        private static string _buildNumber = "0.5.9_dev27";
        private static readonly Random Random = new Random();
        private static bool _logging;

        private static Dictionary<string, IAutoJunkPlugin> _Plugins;

        private static void Main()
        {
            Console.Title = "AutoJunk Loading please wait.";
            Console.WriteLine(_vers);
            var plugins = LoadPlugins(Constants.FolderName);
            if (plugins != null)
            {
                var mThreaddz = new Thread(delegate()
                {
                    try
                    {
                        _Plugins = new Dictionary<string, IAutoJunkPlugin>();
                        foreach (var item in plugins)
                        {
                            Console.Title = "AutoJunk Loading plugins.";
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(">>> Loading engine: " + item.Name + " <<<");
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Gray;
                            _Plugins.Add(item.Name, item);
                            if (item.Name.Contains("AutoJunkApi")) continue;
                            if (_Plugins.ContainsKey(item.Name))
                            {
                                IAutoJunkPlugin plugin = _Plugins[item.Name];
                                int count_pluginfile = GetPluginFile(plugin.FileEnding).Count();
                                if (count_pluginfile != 0)
                                {
                                    foreach (var files in GetPluginFile(plugin.FileEnding))
                                    {
                                        try
                                        {
                                            plugin.Engine(files);
                                            Console.Title = "AutoJunk Loading plugins.";
                                        }
                                        catch (Exception e)
                                        {
                                            if (plugin.CatchException)
                                            {
                                                Console.BackgroundColor = ConsoleColor.Black;
                                                Console.ForegroundColor = ConsoleColor.Yellow;
                                                Console.WriteLine(">>> " + item.Name);
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine(">>> Exception : " + e + " <<<");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        plugin.Engine("File_Not_Found");
                                        Console.Title = "AutoJunk Loading plugins.";
                                    }
                                    catch (Exception e)
                                    {
                                        if (plugin.CatchException)
                                        {
                                            Console.BackgroundColor = ConsoleColor.Black;
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.WriteLine(">>> " + item.Name);
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine(">>> Exception : " + e + " <<<");
                                        }
                                    }
                                }
                            }

                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(">>> Engine: " + item.Name + " is loaded. <<<");
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(">>> Engine loading error: " + e + " <<<");
                    }
                });
                mThreaddz.Start();

                int maximum_wait = 601;
                var mthreaddz_stopped = "No";


                while (maximum_wait > 0)
                {
                    if (mThreaddz.ThreadState == ThreadState.Unstarted)
                    {
                        break;
                    }

                    if (mThreaddz.ThreadState == ThreadState.Stopped)
                    {
                        break;
                    }

                    maximum_wait = maximum_wait - 1;

                    if (maximum_wait == 1)
                    {
                        mThreaddz.Abort();
                        mthreaddz_stopped = "TimeLimit";

                        break;
                    }

                    if (maximum_wait > 1)
                    {
                        Thread.Sleep(1000);
                    }
                }

                if (mthreaddz_stopped == "TimeLimit")
                {
                    Console.Clear();
                    Console.WriteLine("");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Loading engines was stopped bescause of time limit (10 minutes).");
                    Console.WriteLine("");
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            var updateOr = GetUpdate("http://mangekyoukraken.mygamesonline.org/version/new/build.html");

            if (updateOr.Contains("Error"))
            {
                MessageBox.Show("Failed to check for update!", "Error Update", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else if (!updateOr.Contains("Actuall"))
            {
                var mThread = new Thread(delegate()
                {
                    Form update = new update();
                    update.ShowDialog();
                });
                mThread.Start();
                Thread.Sleep(1000);
            }

            var errsr = false;
            _logging = true;

            var count = GetCpp().Count;
            var count2 = GetCsharp().Count;

            BigInteger times = 0;

            Console.Title = "AutoJunk Code Adder [C++ C#] Version: " + _vers;

            Console.WriteLine(
                "Do you want to make log files? (This is not recommended if you want to add lots of junk code): (1 for yes 2 for no): ");
            try
            {
                BigInteger readedLog = int.Parse(Console.ReadLine());
                if (readedLog == 2)
                {
                    _logging = false;
                }
                else if (readedLog >= 3)
                {
                    Console.WriteLine("Logging was turned on.");
                    _logging = true;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("You dont write any number. Logging was turned on.");
                if (_logging == false)
                    _logging = true;
            }

            if (_logging == true)
                WriteLog("---------------LOG START " + "TIME: " + DateTime.Now.ToLongTimeString() + "----------------");

            if (count == 0 && count2 == 0)
            {
                errsr = true;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error! I dont find any .cpp and .cs files!");
                if (_logging == true)
                    WriteLog("Error! I dont find any .cpp and .cs files!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Loading C++ (Mangekyou.Sharingan Engine) Version: " + _versCpp);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Loading C# (Mangekyou.Sharingan Engine) Version: " + _versCsh);
                Console.ForegroundColor = ConsoleColor.Cyan;
                if (count2 == 1)
                    Console.WriteLine("I found " + count2 + " C# file.");
                if (count2 >= 2)
                    Console.WriteLine("I found " + count2 + " C# files.");
                if (count == 1)
                    Console.WriteLine("I found " + count + " C++ file.");
                if (count >= 2)
                    Console.WriteLine("I found " + count + " C++ files.");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("How many times you want to add junk code (to one file)?:");
                Console.ForegroundColor = ConsoleColor.White;
                try
                {
                    times = BigInteger.Parse(Console.ReadLine());
                }
                catch (Exception)
                {
                    Console.WriteLine("You dont write any number.");
                    if (_logging == true)
                        WriteLog("You dont write any number.");
                    errsr = true;
                }

                if (count >= 1)
                {
                    WriteJunkCppNew(times);
                }
                if (count2 >= 1)
                {
                    WriteJunkCsharpNew(times);
                }
            }


            if (errsr == true)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Error] Adding wasnt successful. Click Enter, to exit.");
                if (_logging == true)
                    WriteLog("ERROR");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Adding successful. Click Enter, to exit.");
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                "WARNING! Is recomended to turn off this program before turning on steam games with VAC.");
            if (_logging == true)
                WriteLog("---------------LOG END----------------");
            Console.ReadLine();
        }

        private static void WriteJunkCppNew(BigInteger times)
        {
        //here is nothing, bescause i making new engine. I update it, once it is completed.
        }
        private static void WriteJunkCsharpNew(BigInteger times)
        {
        //here is nothing, bescause i making new engine. I update it, once it is completed.
        }

        private static string GetUpdate(string url)
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead(url))
                using (var textReader = new StreamReader(stream, Encoding.UTF8, true))
                {
                    var versionUpdate = HttpUtility.HtmlDecode(textReader.ReadToEnd());
                    return !versionUpdate.Equals(_buildNumber) ? versionUpdate : "Actuall";
                }
            }
            catch (Exception)
            {
                return "Error";
            }
        }


        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuwxyz";
            var blaba = new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
            return blaba;
        }

        private static string _RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var blaba = new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
            return blaba;
        }

        private static List<string> GetCpp()
        {
            return Directory.GetFiles(Directory.GetCurrentDirectory(), "*.cpp", SearchOption.AllDirectories).ToList();
        }

        private static List<string> GetCsharp()
        {
            return Directory.GetFiles(Directory.GetCurrentDirectory(), "*.cs", SearchOption.AllDirectories).ToList();
        }

        private static IEnumerable<string> GetPluginFile(string file)
        {
            return Directory.GetFiles(Directory.GetCurrentDirectory(), "*." + file, SearchOption.AllDirectories)
                .ToList();
        }

        private static void WriteLog(string logMessage)
        {
            using (var w = AppendText("Log-" + DateTime.Today.ToString("dd-MM-yyyy") + "." + "txt"))
            {
                w.WriteLine("[" + DateTime.Now.ToLongTimeString() + "]" + " " + logMessage);
            }
        }

        private static ICollection<IAutoJunkPlugin> LoadPlugins(string path)
        {
            if(Directory.Exists(path))
            {
                var dllFileNames = Directory.GetFiles(path, "*.dll");

                ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
                foreach(string dllFile in dllFileNames)
                {
                    AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                    Assembly assembly = Assembly.Load(an);
                    assemblies.Add(assembly);
                }

                var pluginType = typeof(IAutoJunkPlugin);
                ICollection<Type> pluginTypes = new List<Type>();
                foreach(var assembly in assemblies)
                {
                    if (assembly == null) continue;
                    var types = assembly.GetTypes();

                    foreach(var type in types)
                    {
                        if(type.IsInterface || type.IsAbstract)
                        {
                            continue;
                        }
                        else
                        {
                            if(type.GetInterface(pluginType.FullName) != null)
                            {
                                pluginTypes.Add(type);
                            }
                        }
                    }
                }

                ICollection<IAutoJunkPlugin> plugins = new List<IAutoJunkPlugin>(pluginTypes.Count);
                foreach(var type in pluginTypes)
                {
                    var plugin = (IAutoJunkPlugin)Activator.CreateInstance(type);
                    plugins.Add(plugin);
                }

                return plugins;
            }

            return null;
        }
    }
}
