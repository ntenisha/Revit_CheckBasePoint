using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace CheckBasePoint
{
    [Transaction(TransactionMode.Manual)]

    public class GetBpFromCoordFiles : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            string bpFilePath = Paths.bpFilePath;

            Loger01.Write("запущен GetBpFromCoordFiles");

            uiApp.DialogBoxShowing += CommonClassBp.Application_DialogBoxShowing;

            List<string> modelPaths = ReadSourcesFromBpCoord(bpFilePath);
            List<List<object>> results = CheckBpFiles(commandData, modelPaths);

            uiApp.DialogBoxShowing -= CommonClassBp.Application_DialogBoxShowing;

            CommonClassBp.WriteResultsToJsonFile(bpFilePath, results);
            Loger01.Write("Завершен GetBpFromCoordFiles\n");
            return Result.Succeeded;
        }


        public static List<string> ReadSourcesFromBpCoord(string filePath)
        {
            List<string> sources = new List<string>();

            try
            {
                string jsonText = File.ReadAllText(filePath);
                JObject jsonObject = JObject.Parse(jsonText);
                JArray itemsArray = (JArray)jsonObject["Items"];

                foreach (JToken item in itemsArray)
                {
                    string source = item["PathToCoordFile"].ToString();
                    if (File.Exists(source))
                    {
                        sources.Add(source);
                    }
                    else
                    {
                        Loger01.Write("Файл не существует." + source);
                    }
                    
                }

                Loger01.Write("Извлечены следующие значения Source из JSON-файла:");
                foreach (string source in sources)
                {
                    Loger01.Write(source);
                }
            }
            catch (Exception ex)
            {
                Loger01.Write("Произошла ошибка при чтении JSON-файла: " + ex.Message);
            }

            return sources;
        }


        public List<List<object>> CheckBpFiles(ExternalCommandData commandData, List<string> modelPaths)
        {
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            List<List<object>> result = new List<List<object>>();

            foreach (string modelPath in modelPaths)
            {
                try
                {
                    BasicFileInfo bfi = BasicFileInfo.Extract(modelPath);
                    Tuple<Document, string> docTuple = CommonClassBp.OpenDocBackground(app, modelPath);
                    Document cdoc = docTuple.Item1;

                    using (Transaction t = new Transaction(cdoc, "Change Doc"))
                    {
                        t.Start();
                        List<object> coordTemp = CommonClassBp.GetBp(cdoc);
                        coordTemp.Insert(0, modelPath);
                        result.Add(coordTemp);
                        cdoc.Regenerate();
                        t.Commit();
                    }

                    if (bfi.IsWorkshared)
                    {
                        CommonClassBp.SyncWithoutRelinquishing(cdoc);
                    }


                    cdoc.Close(true);
                }
                catch (Exception e)
                {
                    Loger01.Write($"Error processing file {modelPath}: {e.Message}");
                }
            }

            return result;
        }


    }
}