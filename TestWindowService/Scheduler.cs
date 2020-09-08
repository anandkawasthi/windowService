using Common;
using ExportToExcel;
using FileReader;
using FileWriter;
using ReadTextFileCSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Timers;

namespace TestWindowService
{
    public partial class Scheduler : ServiceBase
    {
        private Timer timer1 = null;
        private static string CSVExport;
        private static string sourceLocation;
        private static string targetLocation;
        private static string completedFileLocation;
        private static string errorFileLocation;

        public Scheduler()
        {
            InitializeComponent();
            timer1 = new Timer();
            double inter = GetNextInterval();
            timer1.Interval = inter;
            timer1.Elapsed += new ElapsedEventHandler(timer1_Tick);

        }

        protected override void OnStart(string[] args)
        {
            CSVExport = ConfigurationManager.AppSettings["exportCsv"];
            sourceLocation = ConfigurationManager.AppSettings["sourceFilePath"];
            targetLocation = ConfigurationManager.AppSettings["targetFilePath"];
            completedFileLocation = ConfigurationManager.AppSettings["completedFilePath"];
            errorFileLocation = ConfigurationManager.AppSettings["completedFilePath"];


            timer1.AutoReset = true;
            timer1.Enabled = true;
            LoggerService.WriteErrorLog("service started");
        }

        private void timer1_Tick(object sender, ElapsedEventArgs e)
        {
            string[] filePaths = Directory.GetFiles(sourceLocation);
            string errorDestination = "";

            foreach (var file in filePaths)
            {
                try
                {
                    var filename = Path.GetFileName(file);
                    string destination = completedFileLocation + "//" + filename;
                    errorDestination = errorFileLocation + "//" + filename;

                    if (!File.Exists(destination))
                    {
                        ProcessFiles(targetLocation, file, filename);

                        File.Move(file, destination);
                    }
                    else
                    {
                        LoggerService.WriteErrorLog("Ths file already exist completed or destination folder. " + filename);
                        File.Move(file, errorDestination);


                    }
                }
                catch (Exception ex)
                {
                    LoggerService.WriteErrorLog(ex);
                    File.Move(file, errorDestination);
                }
            }

            timer1.Stop();
            System.Threading.Thread.Sleep(1000000);
            SetTimer();

            LoggerService.WriteErrorLog("File reading and exporting is completed");
        }
        private double GetNextInterval()
        {
            var timeString = ConfigurationManager.AppSettings["StartTime"];
            DateTime t = DateTime.Parse(timeString);
            TimeSpan ts = new TimeSpan();
            ts = t - System.DateTime.Now;
            if (ts.TotalMilliseconds < 0)
            {
                ts = t.AddDays(1) - DateTime.Now;
            }
            return ts.TotalMilliseconds;
        }

        private void SetTimer()
        {
            double inter = (double)GetNextInterval();
            timer1.Interval = inter;
            timer1.Start();
        }
        private static void ProcessFiles(string targetLocation, string file, string fileName)
        {
            FirstLabelRules.readLineStart = false;
            FirstLabelRules.readLineEnd = false;
            SecondLabelRules.LineItemStart = false;
            SecondLabelRules.LineItemEnd = false;
            var finalLinesData = new List<LineItem>();

            LoggerService.WriteErrorLog("Start Reading Text File-" + fileName);

            var FirstDraft = TextReaderHelper.FirstLabelParsing(file);

            LoggerService.WriteErrorLog("FirstDraft completed -" + fileName);

            var ParentEobBlock = FirstDraft[0];

            FirstDraft.Remove(ParentEobBlock);

            LoggerService.WriteErrorLog("reading header eob data -" + fileName);

            var headerEobData = TextReaderHelper.ParseParentEob(ParentEobBlock);

            LoggerService.WriteErrorLog("header eob data completed" + fileName);

            LoggerService.WriteErrorLog("parsing second draft -" + fileName);

            var LastDraft = TextReaderHelper.SecondLabelParsing(FirstDraft);

            LoggerService.WriteErrorLog("completed second draft -" + fileName);

            var targetFilename = Path.GetFileName(file).Replace(".txt", ".xlsx");


            LastDraft.ForEach(x =>
            {
                finalLinesData.AddRange(x.LineItems);
            });

            headerEobData[0].Payer = finalLinesData.First().Payer;

            LoggerService.WriteErrorLog("Prepare list of object for export -" + fileName);


            string customParentEobExcelSavingPath = targetLocation + "\\" + "Header-" + targetFilename;

            string customExcelSavingPath = targetLocation + "\\" + targetFilename;

            LoggerService.WriteErrorLog("Export started -" + fileName);


            if (CSVExport.Contains("true"))
            {
                customParentEobExcelSavingPath = customParentEobExcelSavingPath.Replace(".xlsx", ".CSV");
                customExcelSavingPath = customExcelSavingPath.Replace(".xlsx", ".CSV");

                ExcelExport.ToCSV(Utility.ConvertToDataTable(headerEobData), customParentEobExcelSavingPath);

                var datatable = Utility.ConvertToDataTable(finalLinesData);

                datatable.Columns.Remove("AdjustmentDescription");

                ExcelExport.ToCSV(datatable, customExcelSavingPath);
            }
            else
            {
                CreateExcelFile.CreateExcelDocument(headerEobData, customParentEobExcelSavingPath);
                CreateExcelFile.CreateExcelDocument(finalLinesData, customExcelSavingPath);
            }

            LoggerService.WriteErrorLog("Export completed- " + fileName);
        }
        protected override void OnStop()
        {
            timer1.Enabled = false;
            LoggerService.WriteErrorLog("Test window service stopped");
        }
    }
}
