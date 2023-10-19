using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace CheckBasePoint
{
    [Transaction(TransactionMode.Manual)]

    public class CheckBpWorkingFiles : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;

            RunWF(uiApp);
            return Result.Succeeded;
        }

        public static void RunWF(UIApplication uiApp)
        {
            //UIDocument uiDoc = uiApp.ActiveUIDocument;
            //Document doc = uiDoc.Document;
            string _appversion = uiApp.Application.VersionNumber;


            string bpFilePath = Paths.bpFilePath;
            string workingFilePath = Paths.workingFilePath;
            string logFile = Paths.logFile;

            Loger01.Write("Запущен CheckBpWorkingFiles");
            try
            {
                uiApp.DialogBoxShowing += CommonClassBp.Application_DialogBoxShowing;
                List<List<object>> resultsFromWf = CombineDataFromBpAndWfFiles(bpFilePath, workingFilePath);
                List<string> resCheck = CheckBpFiles(uiApp, resultsFromWf);

                uiApp.DialogBoxShowing -= CommonClassBp.Application_DialogBoxShowing;

                CommonClassBp.WriteJsonWorkingFiles(resCheck, logFile);
                Loger01.Write("Завершен CheckBpWorkingFiles\n");
            }
            catch (Exception ex)
            {
                Loger01.Write("Ошибка в методе RunWF");
                Loger01.LogEx(ex);


            }

        }


        public class JsonDataItemBp
        {
            public string PathToCoordFile { get; set; }
            public double EastWestParam { get; set; }
            public double NorthSouthParam { get; set; }
            public double ElevationParam { get; set; }
            public double AngleToNorthParam { get; set; }
        }

        public class JsonDataBp
        {
            public List<JsonDataItemBp> Items { get; set; }
        }
        public static List<List<object>> ReadBpFile(string filePath)
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);

                JsonDataBp jsonData = JsonConvert.DeserializeObject<JsonDataBp>(jsonString);

                List<List<object>> results = new List<List<object>>();

                foreach (var item in jsonData.Items)
                {
                    List<object> resultItem = new List<object>
                {
                    item.PathToCoordFile,
                    item.EastWestParam,
                    item.NorthSouthParam,
                    item.ElevationParam,
                    item.AngleToNorthParam
                };

                    results.Add(resultItem);
                }

                return results;
            }
            catch (Exception ex)
            {
                Loger01.Write("Произошла ошибка при чтении файла JSON: " + ex.Message);
                return null;
            }
        }

        public class JsonItemWf
        {
            public string WorkingFile { get; set; }
            public string PathToCoordFile { get; set; }
        }

        public class JsonDataWF
        {
            public List<JsonItemWf> Items { get; set; }
        }

        public static List<List<object>> ReadWorkingFile(string filePath)
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);

                JsonDataWF jsonData = JsonConvert.DeserializeObject<JsonDataWF>(jsonString);

                List<List<object>> resultsFfromJson = new List<List<object>>();

                foreach (var item in jsonData.Items)
                {
                    List<object> resultItem = new List<object>
                {
                    item.WorkingFile,
                    item.PathToCoordFile
                };

                    if (File.Exists(item.WorkingFile))
                    {
                        resultsFfromJson.Add(resultItem);
                    }
                    else
                    {
                        Loger01.Write("Файл не существует." + item.WorkingFile);
                    }

                    
                }

                return resultsFfromJson;
            }
            catch (Exception ex)
            {
                Loger01.Write("Произошла ошибка при чтении файла JSON: " + ex.Message);
                return null;
            }
        }

        public static List<List<object>> CombineDataFromBpAndWfFiles(string bpFilePath, string WorkingFilePath)
        {
            List<List<object>> resultsFromBp = ReadBpFile(bpFilePath);

            if (resultsFromBp == null)
            {
                Loger01.Write("Ошибка при чтении данных из файла с результатами ReadBpFile");
                return null;
            }

            try
            {
                string jsonString = File.ReadAllText(WorkingFilePath);

                JsonDataWF jsonData = JsonConvert.DeserializeObject<JsonDataWF>(jsonString);

                List<List<object>> resultsFfromJson = new List<List<object>>();

                foreach (var item in jsonData.Items)
                {
                    List<object> resultItem = new List<object>
            {
                item.WorkingFile,
                item.PathToCoordFile
            };

                    var matchingItem = resultsFromBp.FirstOrDefault(bpItem => bpItem[0].ToString() == item.PathToCoordFile);

                    if (matchingItem != null && matchingItem.Count >= 5)
                    {
                        resultItem.Add(matchingItem[1]);
                        resultItem.Add(matchingItem[2]);
                        resultItem.Add(matchingItem[3]);
                        resultItem.Add(matchingItem[4]);
                    }

                    resultsFfromJson.Add(resultItem);
                }

                return resultsFfromJson;
            }
            catch (Exception ex)
            {
                Loger01.Write("Произошла ошибка при чтении файла JSON: ");
                Loger01.LogEx(ex);

                return null;
            }
        }

        public static List<string> CheckBpFiles(UIApplication uiApp, List<List<object>> modelPaths)
        {
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            List<string> res02 = new List<string>();

            foreach (var resultItem in modelPaths)
            {
                try
                {
                    string modelPath = resultItem[0] as string;
                    BasicFileInfo bfi = BasicFileInfo.Extract(modelPath);
                    Tuple<Document, string> docTuple = CommonClassBp.OpenDocBackground(app, modelPath, null);
                    Document cdoc = docTuple.Item1;

                    using (Transaction t = new Transaction(cdoc, "Change Doc"))
                    {
                        t.Start();
                        List<object> coordTemp = CommonClassBp.GetBp(cdoc);
                        cdoc.Regenerate();
                        t.Commit();

                        if (!coordTemp.SequenceEqual(resultItem.Skip(2).Take(4)))
                        {
                            res02.Add(modelPath);
                        }
                    }

                    if (bfi.IsWorkshared)
                    {
                        CommonClassBp.SyncWithoutRelinquishing(cdoc);
                    }

                    cdoc.Close(true);
                }
                catch (Exception ex)
                {
                    Loger01.Write($"Error processing file {resultItem}: {ex.Message}");
                }
            }

            return res02;
        }




    }

}