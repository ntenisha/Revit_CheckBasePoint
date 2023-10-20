using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Linq;
using System.Collections;

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
            Paths.verRevit = uiApp.Application.VersionNumber.ToString();
            Loger01.Write("Paths.verRevit\t" + Paths.verRevit);

            Loger01.Write("запущен GetBpFromCoordFiles");

            uiApp.DialogBoxShowing += CommonClassBp.Application_DialogBoxShowing;

            List<string> modelPaths = ReadFileWithPaths(Paths.bpFilePathUser);
            Loger01.Write("Paths.bpFilePathUser " + Paths.bpFilePathUser);
            List<List<object>> results = CheckBpFiles(commandData, modelPaths);

            uiApp.DialogBoxShowing -= CommonClassBp.Application_DialogBoxShowing;

            CommonClassBp.WriteResultsToJsonFile(Paths.bpFilePath, results);
            Loger01.Write("Завершен GetBpFromCoordFiles\n");
            return Result.Succeeded;
        }

        public static List<string> ReadFileWithPaths(string filePath)
        {
            List<string> allFilePaths = new List<string>();
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                return allFilePaths;
            }
            
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string filePath01 = ParsePaths(line);
                        if (File.Exists(filePath01))
                        {
                            //Loger01.Write($"Файл существует: {filePath01}");
                            allFilePaths.Add(filePath01);
                        }
                        else
                        {
                            Loger01.Write($"Файла нет: {filePath01}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Loger01.Write($"Ошибка при чтении файла: {ex.Message}");
            }

                foreach (var item in allFilePaths)
                {
                    foreach (var value in item)
                    {
                    Loger01.Write(value + "\t");
                    }
                    Loger01.Write("\n");
                }


            List<string> uniqueList = allFilePaths.Distinct().ToList();

            foreach (var item in uniqueList)
            {
                foreach (var value in item)
                {
                    Loger01.Write(value + "\t");
                }
                Loger01.Write("\n");
            }
            return uniqueList;
        }

        public static string ParsePaths(string inputText)
        {
            string pattern = "\"(.*?)\"";
            Regex regex = new Regex(pattern);

            MatchCollection matches = regex.Matches(inputText);

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    string filePath = match.Groups[1].Value;
                    string normalizedPath = Path.GetFullPath(filePath);
                    return normalizedPath;
                }
            }

            string trimmedText = inputText.Trim(); // Удалить начальные и конечные пробелы
            trimmedText = trimmedText.Trim(','); // Удалить начальные и конечные запятые
            string normalizedPath02 = Path.GetFullPath(trimmedText);
            return normalizedPath02;
        }



        public List<List<object>> CheckBpFiles(ExternalCommandData commandData, List<string> modelPaths)
        {
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            List<List<object>> result = new List<List<object>>();
            if (!modelPaths.Any())
            {
                Loger01.Write("CheckBpFiles Список modelPaths пуст");
                return result;
            }

            int nextNumber = 1;
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
                        coordTemp.Insert(1, nextNumber.ToString());
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
                nextNumber++;
            }

            return result;
        }


    }
}