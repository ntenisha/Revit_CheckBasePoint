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

            Paths path01 = new Paths(uiApp.Application.VersionNumber.ToString());
            PathsStatic.verRevit = uiApp.Application.VersionNumber.ToString();
            Loger01.Write("запущен GetBpFromCoordFiles");
            //исправить
            // bpFilePathUser = Paths.DirectoryPath + "Check_Bp_coord_files" + Paths.verRevit + ".txt";
            //string bpFilePath = Paths.DirectoryPath + "Json\\Check_Bp_coord_files" + Paths.verRevit + ".Json";
            uiApp.DialogBoxShowing += CommonClassBp.Application_DialogBoxShowing;

            List<string> modelPaths = ReadFileWithPaths(path01.BpFilePathUser);

            List<List<object>> results = CheckBpFiles(commandData, modelPaths);

            uiApp.DialogBoxShowing -= CommonClassBp.Application_DialogBoxShowing;

            CommonClassBp.WriteResultsToJsonFile(path01.BpFilePath, results);

            CommonClassBp.DeleteFilesAndSubfolders(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "temp_dir"));
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

            List<string> uniqueList = allFilePaths.Distinct().ToList();

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
                    Tuple<Document, string> docTuple = CommonClassBp.OpenDocumentWithDetach(app, modelPath);
                    Document cdoc = docTuple.Item1;

                    //using (Transaction t = new Transaction(cdoc, "Change Doc"))
                    //{
                    //    t.Start();
                        List<object> coordTemp = CommonClassBp.GetBp(cdoc);
                        coordTemp.Insert(0, modelPath);
                        coordTemp.Insert(1, nextNumber.ToString());
                        result.Add(coordTemp);
                    //    cdoc.Regenerate();
                    //    t.Commit();
                    //}

/*                    if (bfi.IsWorkshared)
                    {
                        CommonClassBp.SyncWithoutRelinquishing(cdoc);
                    }*/


                    cdoc.Close(false);
                }
                catch (Exception ex)
                {
                    Loger01.Write($"Error processing file {modelPath}: {ex.Message}");
                    Loger01.LogEx(ex);
                }
                nextNumber++;
            }

            return result;
        }


    }
}