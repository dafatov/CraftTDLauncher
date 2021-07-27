using System.Windows;
using System.Net;
using System;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading;
using System.Diagnostics;

namespace CrafTDLauncher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window

    {
        const String PUBLIC_KEY = "https://yadi.sk/d/TVvwAJcij9AwBA";
        const String PUBLIC_KEY_LAUNCHER = "https://yadi.sk/d/dXtISOzIilaUyg";
        const String URL_BASE = "https://cloud-api.yandex.net:443/v1/disk/public/resources?public_key=";
        const String URL_PATH = "&path=";
        const String LAUNCHER_NAME = "CrafTDLauncher";
        const String GAME_NAME = "CrafTD";
        const String LAUNCHER_VERSION = "0.0.0.3-Alpha";
        String PATH_BASE = "D://";
        WebClient fileClient;
        WebClient jsonClient;
        WebClient updateClient;
        int abort;
        long allBytesReceived;
        long currentSize;
        int index;

        List<RootObject> filesToDownload;
        RootObject launcher;


        public MainWindow()
        {
            InitializeComponent();
            init();
            checkUpdate();
            /*byte[] tmp = File.ReadAllBytes("Update.runtimeconfig.json");
            string s = "{";
            
            for (int i = 0; i < tmp.Length; i++)
            {
                s += tmp[i] + ",";
            }
            s += "}";
            Console.WriteLine(s);
            Console.WriteLine("Ready");*/
        }

        private void checkUpdate()
        {
            launcher = http_loader_string(URL_BASE + PUBLIC_KEY_LAUNCHER);
            if (checkError())
            {
                string version = updateClient.DownloadString(launcher._embedded.items[1].file);
                if (!version.Equals(LAUNCHER_VERSION))
                {
                    Button_Download.IsEnabled = false;
                    Button_Dir.IsEnabled = false;
                    updateClient.DownloadFileAsync(new System.Uri(launcher._embedded.items[0].file), "CrafTDLauncher.exe.part");
                }
            }
        }

        private void init()
        {
            File.Delete("/Update.dll");
            File.Delete("/Update.exe");
            File.Delete("/Update.runtimeconfig.json");


            this.Title = LAUNCHER_NAME + " v." + LAUNCHER_VERSION;

            fileClient = new WebClient();
            jsonClient = new WebClient();
            updateClient = new WebClient();
            fileClient.DownloadProgressChanged += client_DownloadProgressChanged;
            fileClient.DownloadFileCompleted += client_DownloadProgressCompleted;
            updateClient.DownloadFileCompleted += update_DownloadProgressCompleted;
            filesToDownload = new List<RootObject>();
            PATH_BASE = Environment.CurrentDirectory;
            setToolTip(Button_Dir, PATH_BASE);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            downloadAllFiles();
        }

        private bool checkError()
        {
            if (abort != 0)
            {
                switch (abort)
                {
                    case -1:
                        System.Windows.Forms.MessageBox.Show("Error: Url has bad format");
                        break;
                    case 429:
                        System.Windows.Forms.MessageBox.Show("Error: Can't fixed. The daily dose was exceeded!");
                        break;
                    case 500:
                        System.Windows.Forms.MessageBox.Show("Error: Server hasn't theese files, contact admin...");
                        break;
                    default:
                        System.Windows.Forms.MessageBox.Show("Error: Unknown error, contact admin... (" + abort + ")");
                        break;
                }
                return false;
            } else
            {
                return true;
            }
        }

        private void downloadAllFiles()
        {
            long allSize = 0;
            allBytesReceived = 0;
            index = 0;

            progressBar_All.Value = 0;
            progressBar.Value = 0;

            progressBar.Maximum = 1;
            abort = 0;
            TextBlock_Status.Text = "0%";
            findAllFiles(URL_BASE + PUBLIC_KEY + URL_PATH + "/");
            if (checkError())
            {
                Console.WriteLine("\n\n");
                foreach (RootObject file in filesToDownload)
                {
                    allSize += file.size;
                }
                progressBar_All.Maximum = allSize;
                /*foreach (RootObject file in filesToDownload)
                {*/
                if (filesToDownload.Count == 0)
                {
                    progressBar_All.Value = progressBar_All.Maximum;
                    TextBlock_Status.Text = "100%";
                }
                else
                {
                    currentSize = filesToDownload[index].size;
                    Button_Download.IsEnabled = false;
                    Button_Dir.IsEnabled = false;
                    client_DownoloadStart(filesToDownload[index].file, PATH_BASE + filesToDownload[index].path);
                }
                /*}*/
            }
        }

        private void findAllFiles(String url)
        {
            RootObject current = http_loader_string(url);
            
            if (abort != 0)
            {
                return;
            }
            
            for (int i = 0; i < current._embedded.items.Count; i++)
            {
                RootObject item = current._embedded.items[i];

                if (item.type.Equals("file"))
                {
                    Console.WriteLine(File.Exists(PATH_BASE + item.path) + " : " + File.GetLastWriteTimeUtc(PATH_BASE + item.path) + " : " + item.modified);
                    if (!File.Exists(PATH_BASE + item.path) && !File.GetLastWriteTimeUtc(PATH_BASE + item.path).Equals(item.modified)) {
                        filesToDownload.Add(item);
                    }
                } else if (item.type.Equals("dir"))
                {
                    findAllFiles(URL_BASE + PUBLIC_KEY + URL_PATH + item.path);
                } else
                {
                    throw new Exception("Error type of item");
                }
            }
        }

        private RootObject http_loader_string(String url)
        {
            try
            {
                Thread.Sleep(100);
                String tmp = jsonClient.DownloadString(url);

                RootObject template = JsonConvert.DeserializeObject<RootObject>(tmp);

                return template;

            } catch (UriFormatException u)
            {
                Console.WriteLine(u.ToString());
                abort = -1;
            } catch (WebException w)
            {
                abort = int.Parse(w.Message.Substring(w.Message.IndexOf('(') + 1, w.Message.IndexOf(')') - w.Message.IndexOf('(') - 1));
                Console.WriteLine("\n\n"+w.Message+"\n\n");
            }
            return null;
        }

        private void client_DownoloadStart(String URL, String Path)
        {
            String tmp = Path.Substring(0, Path.LastIndexOf('/'));
            //Console.WriteLine(tmp);
            if (!Directory.Exists(tmp))
            {
                Directory.CreateDirectory(tmp);
            }
            try
            {
                fileClient.DownloadFileAsync(new System.Uri(URL), Path);
            }
            catch (UriFormatException u)
            {
                Console.WriteLine(u.ToString());
            }
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = (double)e.BytesReceived/e.TotalBytesToReceive;
            progressBar_All.Value = e.BytesReceived + allBytesReceived;
            TextBlock_Status.Text = Math.Round(progressBar_All.Value / progressBar_All.Maximum * 100).ToString()+"%";
        }

        private void client_DownloadProgressCompleted(object sender, AsyncCompletedEventArgs e)
        {
            progressBar.Value = 0;
            allBytesReceived += currentSize;

            //Console.WriteLine(DateTime.Parse(filesToDownload[index].modified));
            File.SetLastWriteTimeUtc(PATH_BASE + filesToDownload[index].path, DateTime.Parse(filesToDownload[index].modified));

            index++;
            if (index == filesToDownload.Count)
            {
                Button_Download.IsEnabled = true;
                Button_Dir.IsEnabled = true;
                return;
            }

            currentSize = filesToDownload[index].size;
            client_DownoloadStart(filesToDownload[index].file, PATH_BASE + filesToDownload[index].path);
        }

        private void Button_Click_Dir(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.RootFolder = Environment.SpecialFolder.MyComputer;
            if (folder.ShowDialog().Equals(System.Windows.Forms.DialogResult.OK))
            {
                PATH_BASE = folder.SelectedPath + "/" + GAME_NAME;
                setToolTip(Button_Dir, folder.SelectedPath);
            }
        }

        private void setToolTip(System.Windows.Controls.Button button, String aString)
        {
            System.Windows.Controls.ToolTip toolTip = new System.Windows.Controls.ToolTip();
            StackPanel toolTipPanel = new StackPanel();
            toolTipPanel.Children.Add(new TextBlock { Text = "Download directory (Folder will created):", FontSize = 10 });
            toolTipPanel.Children.Add(new TextBlock { Text = aString, FontSize = 16 });
            toolTip.Content = toolTipPanel;
            button.ToolTip = toolTip;
        }

        private void update_DownloadProgressCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try { 
            File.SetLastWriteTimeUtc("CrafTDLauncher.exe.part", DateTime.Parse(launcher.modified));
            Console.WriteLine("\n\n"+DateTime.Parse(launcher.modified)+"\n\n");
            File.WriteAllBytes("Update.exe", Data.Update_exe);
            File.WriteAllBytes("Update.dll", Data.Update_dll);
            File.WriteAllBytes("Update.runtimeconfig.json", Data.Update_runtimeconfig_json);

            
           
                Process.Start("Update.exe", Process.GetCurrentProcess().ProcessName);
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception) { }
        }
    }
}
