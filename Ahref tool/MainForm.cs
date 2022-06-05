using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;
using Ahref_tool.Models;
using Ahref_tool.Services;
using Newtonsoft.Json;

namespace Ahref_tool
{
    public partial class MainForm : MetroForm
    {
        public bool LogToUi = true;
        public bool LogToFile = true;
        private readonly string _path = Application.StartupPath;
        public HttpCaller HttpCaller = new HttpCaller();

        private readonly AhrefService _ahrefService = new AhrefService();
        private readonly SeoService _seoService = new SeoService();
        private readonly WhoIsService _whoIsService = new WhoIsService();
        private readonly GoogleService _googleService = new GoogleService();
        private readonly ExcelService _excelService = new ExcelService();
        public MainForm()
        {
            InitializeComponent();
            Reporter.OnLog += OnLog;
            Reporter.OnError += OnError;
            Reporter.OnProgress += OnProgress;
        }

        private void OnProgress(object sender, (int nbr, int total, string message) e)
        {
            Display($"{e.message} {e.nbr} / {e.total}");
            SetProgress(e.nbr * 100 / e.total);
        }

        private void OnError(object sender, string e)
        {
            ErrorLog(e);
        }

        private void OnLog(object sender, string e)
        {
            Display(e);
            NormalLog(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists("domains"))
            {
                Directory.CreateDirectory("domains");
            }

            ServicePointManager.DefaultConnectionLimit = 65000;
            Directory.CreateDirectory("data");
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Utility.CreateDb();
            Utility.LoadConfig();
            inputI.Text = _path + @"\input.xlsx";
            Utility.InitCntrl(this);
        }
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), @"Unhandled Thread Exception");
        }
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show((e.ExceptionObject as Exception)?.ToString(), @"Unhandled UI Exception");
        }
        #region UIFunctions
        public delegate void WriteToLogD(string s, Color c);
        public void WriteToLog(string s, Color c)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new WriteToLogD(WriteToLog), s, c);
                    return;
                }
                if (LogToUi)
                {
                    if (DebugT.Lines.Length > 5000)
                    {
                        DebugT.Text = "";
                    }
                    DebugT.SelectionStart = DebugT.Text.Length;
                    DebugT.SelectionColor = c;
                    DebugT.AppendText(DateTime.Now.ToString(Utility.SimpleDateFormat) + " : " + s + Environment.NewLine);
                }
                Console.WriteLine(DateTime.Now.ToString(Utility.SimpleDateFormat) + @" : " + s);
                if (LogToFile)
                {
                    File.AppendAllText(_path + "/data/log.txt", DateTime.Now.ToString(Utility.SimpleDateFormat) + @" : " + s + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public void NormalLog(string s)
        {
            WriteToLog(s, Color.Black);
        }
        public void ErrorLog(string s)
        {
            WriteToLog(s, Color.Red);
        }
        public void SuccessLog(string s)
        {
            WriteToLog(s, Color.Green);
        }
        public void CommandLog(string s)
        {
            WriteToLog(s, Color.Blue);
        }

        public delegate void SetProgressD(int x);
        public void SetProgress(int x)
        {
            if (InvokeRequired)
            {
                Invoke(new SetProgressD(SetProgress), x);
                return;
            }
            if ((x <= 100))
            {
                ProgressB.Value = x;
            }
        }
        public delegate void DisplayD(string s);
        public void Display(string s)
        {
            if (InvokeRequired)
            {
                Invoke(new DisplayD(Display), s);
                return;
            }
            displayT.Text = s;
        }

        #endregion
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _googleService._driver?.Quit();
            Utility.Config = new Dictionary<string, string>();
            Utility.SaveCntrl(this);
            Utility.SaveConfig();
            _googleService._driver?.Quit();
        }
        private async void startB_Click(object sender, EventArgs e)
        {
            LogToUi = logToUII.Checked;
            LogToFile = logToFileI.Checked;
            //we spin it in a new worker thread
            //Task.Run(MainWork);
            //we run mainWork on the UI thread
            //await MainWork();

        }
        private void loadInputB_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog { Filter = @"txt|*.txt", InitialDirectory = _path };
            if (o.ShowDialog() == DialogResult.OK)
            {
                inputI.Text = o.FileName;
            }
        }
        private void openInputB_Click_1(object sender, EventArgs e)
        {
            try
            {
                Process.Start(inputI.Text);
            }
            catch (Exception ex)
            {
                ErrorLog(ex.ToString());
            }
        }
        private void openOutputB_Click_1(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                ErrorLog(ex.ToString());
            }
        }
        private void loadOutputB_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = @"xlsx file|*.xlsx",
                Title = @"Select the output location"
            };
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
            }
        }

        private void LoadDomains()
        {
            Singleton.Domains = new List<Domain>();
            var inputs = File.ReadAllLines(inputI.Text).ToList();
            foreach (var input in inputs)
            {
                Singleton.Domains.Add(new Domain { Name = input });
            }
        }

        private async void startB_Click_1(object sender, EventArgs e)
        {
            try
            {
                await MainWork();
            }
            catch (Exception exception)
            {
                Display($"Critical error, stopping the main work : {exception.Message}");
                ErrorLog(exception.ToString());
            }
        }
        private async Task MainWork()
        {
            SuccessLog("Work Started");
            LoadDomains();
            _googleService.Init();

            try
            {
                Reporter.Log($"Start log in to Ahref");
                await _ahrefService.LogIn(userI.Text, passI.Text);
                Reporter.Log($"log in to Ahref succeeded");
            }
            catch (Exception e)
            {
                throw new Exception($"Error login : {e.Message}");
            }

           
            await _ahrefService.DownloadAhrefCsv();
            var googleTask = _googleService.PopulateGoogleResult();
            var whoIsTask = _whoIsService.GetRegisteredDate();
            var seoTask = _seoService.PopulateTfAndCfData();
            var t2 = _ahrefService.PopulateOrganicTrafficCost();
            var t3 = _ahrefService.PopulateOrganicTraffic();
            var t4 = _ahrefService.PopulateReferringTraffic();
            var t5 = _ahrefService.PopulateUr();
            var t6 = _ahrefService.PopulateBackLinks();
            var t7 = _ahrefService.PopulateReferringDomains();
            var t8 = _seoService.PopulateDaStatus();

            await Task.WhenAll(googleTask, whoIsTask, seoTask, t2, t3, t4, t5, t6,t7,t8);
            _googleService.Dispose();
            //var json = JsonConvert.SerializeObject(Singleton.Domains);

            //File.WriteAllText("json.txt", json);

            _excelService.Export($@"domains\Ahref.com {DateTime.Now:dd_MMM_yyyy_HH_mm}.xlsx");

            SuccessLog("Work Completed");
        }
    }
}
