using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using INGP.Utils.RevitAPI.DialogHandler;

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
            string _appversion = uiApp.Application.VersionNumber;

            Paths path01 = new Paths(uiApp.Application.VersionNumber.ToString());
            PathsStatic.verRevit = uiApp.Application.VersionNumber.ToString();
            PathsStatic.uName = Environment.UserName;

            List<string> pathsFileNotFound = new List<string>();
            List<string> filesChecked = new List<string>();
            List<string> filesError = new List<string>();


            string filename = Path.Combine(PathsStatic.errLog, "exLog" + PathsStatic.verRevit + ".txt");
            TrimLogFile(filename);
            Loger01.Write("\n\n\n Новый запуск");

            if (File.Exists(Path.Combine(PathsStatic.errLog, "Файл_не_найден" + PathsStatic.verRevit + ".txt")))
            {
                File.Delete(Path.Combine(PathsStatic.errLog, "Файл_не_найден" + PathsStatic.verRevit + ".txt"));
            }
            List<List<object>> resCheck = null;
            try
            {
                DialogHandlerUtils handler = new DialogHandlerUtils(uiApp);
                handler.DisableRevitDialogs();
                //uiApp.DialogBoxShowing += CommonClassBp.Application_DialogBoxShowing;

                List<List<object>> resultsFromWf = CombineDataFromBpAndWfFiles02(path01.BpFilePath, path01.WorkingFilePathUser, ref pathsFileNotFound);
                if (resultsFromWf.Count > 0)
                {
                    resCheck = CheckBpFiles(uiApp, resultsFromWf, ref filesChecked, ref filesError);
                }

                handler.EnableRevitDialogs();

                //uiApp.DialogBoxShowing -= CommonClassBp.Application_DialogBoxShowing;
                if (resultsFromWf.Count > 0)
                {
                    CommonClassBp.WriteResultsToJsonFileWorking(path01.LogFile, resCheck);
                }
                if (pathsFileNotFound.Count > 0)
                {
                    CommonClassBp.AddResultsFileNotFoundToJsonFile(path01.LogFile, pathsFileNotFound);
                }
                if (resultsFromWf.Count != filesChecked.Count || filesError.Any())
                {
                    List<string> misssingFiles = FindMissingFiles(resultsFromWf, filesChecked);
                    misssingFiles = FindMissingFiles(misssingFiles, pathsFileNotFound);
                    filesError.AddRange(misssingFiles);
                    filesError = filesError.Distinct().ToList();

                    if (filesError.Any()) { AddMissingFilesToJson(path01.LogFile, filesError); }
                }
                CommonClassBp.DeleteFilesAndSubfolders(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CheckBasePoint_temp_dir"));
            }
            catch (Exception ex)
            {
                Loger01.Write("Ошибка в методе RunWF");
                Loger01.LogEx(ex);
            }
        }

        public static void TrimLogFile(string filePath, int maxLines = 1000)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                // Чтение всех строк из файла
                List<string> allLines = new List<string>(File.ReadAllLines(filePath));

                // Если количество строк больше чем maxLines, обрезаем
                if (allLines.Count > maxLines)
                {
                    // Получаем последние maxLines строк
                    allLines = allLines.GetRange(allLines.Count - maxLines, maxLines);
                }

                // Перезаписываем файл с только последними maxLines строками
                File.WriteAllLines(filePath, allLines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }

        public static void AddMissingFilesToJson(string jsonFilePath, List<string> missingFiles)
        {
            try
            {
                // Чтение существующего JSON файла
                string jsonText = File.ReadAllText(jsonFilePath);

                // Десериализация содержимого файла в Dictionary
                var jsonResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonText);

                // Проверка, если блок "CheckError" существует, иначе создаем его
                if (!jsonResult.ContainsKey("CheckError"))
                {
                    jsonResult["CheckError"] = new List<Dictionary<string, object>>();
                }

                // Преобразуем к типу List<Dictionary<string, object>> для добавления новых данных
                var checkErrorList = (List<Dictionary<string, object>>)jsonResult["CheckError"];

                // Добавление новых файлов в секцию "CheckError"
                foreach (string missingFile in missingFiles)
                {
                    var newEntry = new Dictionary<string, object>
            {
                { "PathToModel", missingFile },
                { "Data", DateTime.Now.ToString("yyyy.MM.dd") }
            };

                    checkErrorList.Add(newEntry);
                }

                // Сериализация обратно в JSON и запись в файл
                string updatedJsonText = JsonConvert.SerializeObject(jsonResult, Formatting.Indented);
                File.WriteAllText(jsonFilePath, updatedJsonText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при обновлении JSON файла: {ex.Message}");
            }
        }

        public static List<string> FindMissingFiles(List<List<object>> resultsFromWf, List<string> filesChecked)
        {
            List<string> missingFiles = new List<string>();

            foreach (List<object> result in resultsFromWf)
            {
                if (result.Count > 0 && result[0] is string filePath)
                {
                    if (!filesChecked.Contains(filePath))
                    {
                        missingFiles.Add(filePath);
                    }
                }
            }

            return missingFiles;
        }

        public static List<string> FindMissingFiles(List<string> resultsFromWf, List<string> filesChecked)
        {
            List<string> missingFiles = new List<string>();

            foreach (string filePath in resultsFromWf)
            {

                if (!filesChecked.Contains(filePath))
                {

                    missingFiles.Add(filePath);
                }
            }

            return missingFiles;
        }


        public static List<List<object>> ReadFileWithPipeDelimiter(string filePath, ref List<string> fileNotFound02)
        {
            List<List<object>> result = new List<List<object>>();
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                return result;
            }
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }
                        string[] parts = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length == 2)
                        {
                            string temp01 = ParsePaths(parts[0]);
                            string temp02 = ParseValueInQuotesOrTrim(parts[1]);

                            List<object> row = new List<object> { temp01, temp02 };
                            if (File.Exists(temp01)) { result.Add(row); }
                            else
                            {
                                Loger01.FileNotFound($"Файл не найден: {temp01}");
                                Loger01.Write($"Файл не найден {temp01}");
                                fileNotFound02.Add($"{temp01}");
                            }
                        }
                        else
                        {
                            Loger01.Write($"Invalid line: {line}");
                            fileNotFound02.Add($"{line}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Loger01.LogEx(ex);
            }

            return result;
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

            string trimmedText = inputText.Trim();
            trimmedText = trimmedText.Trim(',');
            string normalizedPath02 = Path.GetFullPath(trimmedText);
            return normalizedPath02;
        }

        public static string ParseValueInQuotesOrTrim(string input)
        {
            // Поиск значения в кавычках
            Match match = Regex.Match(input, "\"(.*?)\"");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                string trimmed = input.Trim();
                trimmed = trimmed.Trim(',');
                trimmed = trimmed.Trim();
                return trimmed;
            }
        }

        public class JsonDataItemBp
        {
            public string PathToCoordFile { get; set; }
            public string EasyName { get; set; }
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
            List<List<object>> results = new List<List<object>>();
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                return results;
            }
            try
            {
                string jsonString = File.ReadAllText(filePath);

                JsonDataBp jsonData = JsonConvert.DeserializeObject<JsonDataBp>(jsonString);

                foreach (var item in jsonData.Items)
                {
                    List<object> resultItem = new List<object>
                {
                    item.EasyName,
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
            public string EasyName { get; set; }
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
                    item.EasyName
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

        public static List<List<object>> CombineDataFromBpAndWfFiles02(string bpFilePath, string WorkingFilePath, ref List<string> fileNotFound01)
        {
            List<List<object>> combinedResults = new List<List<object>>();
            List<List<object>> resultsFromBp = ReadBpFile(bpFilePath);
            List<List<object>> resultsFromWf = ReadFileWithPipeDelimiter(WorkingFilePath, ref fileNotFound01);

            if (resultsFromBp == null)
            { Loger01.Write("Ошибка при чтении данных из файла с результатами ReadBpFile"); return combinedResults; }
            else if (resultsFromBp.Count == 0)
            { Loger01.Write("Файл Check_Bp_coord_files 2019 или 2022 пустой "); return combinedResults; }
            else if (resultsFromWf == null)
            { Loger01.Write("Ошибка при чтении данных из файла с результатами WorkingFilePath"); return combinedResults; }
            else if (resultsFromWf.Count == 0)
            { Loger01.Write("Файл Check_Bp_working_files 2019 или 2022 пустой "); return combinedResults; }

            foreach (List<object> wfItem in resultsFromWf)
            {
                bool foundMatch = false;

                string pathFromWokingFile = wfItem[0] as string;
                string easyNameFromWokingFile = wfItem[1] as string;

                foreach (List<object> bpItem in resultsFromBp)
                {
                    string easyNameFromCoordFile = bpItem[0] as string;

                    // Сравниваем второй элемент из wfItem с первым элементом из bpItem
                    //if (wfItem[1].Equals(bpItem[0]))
                    if (easyNameFromWokingFile.Equals(easyNameFromCoordFile))
                    {
                        double EastWestParam = (double)bpItem[1];
                        double NorthSouthParam = (double)bpItem[2];
                        double ElevationParam = (double)bpItem[3];
                        double AngleToNorthParam = (double)bpItem[4];

                        // Найдено совпадение, создаем новый список и добавляем нужные элементы
                        List<object> combinedItem = new List<object>
                        {
                            pathFromWokingFile,
                            EastWestParam,
                            NorthSouthParam,
                            ElevationParam,
                            AngleToNorthParam
                        };

                        combinedResults.Add(combinedItem);
                        foundMatch = true;
                        break; // Если совпадение найдено, выходим из внутреннего цикла
                    }
                }
                if (!foundMatch)
                {
                    Loger01.Write($"Ошибка нет совпадения" + easyNameFromWokingFile);
                    fileNotFound01.Add($"Ошибка нет совпадения c координационным" + pathFromWokingFile);
                }
            }
            return combinedResults;
        }

        public static double roundToFive(object number)
        {
            double newNumber = Math.Round(Convert.ToDouble(number), 5);
            return newNumber;
        }

        public static List<List<object>> CheckBpFiles(UIApplication uiApp, List<List<object>> modelPaths, ref List<string> filesChecked, ref List<string> filesError)
        {
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            List<List<object>> res02 = new List<List<object>>();

            Paths path01 = new Paths(uiApp.Application.VersionNumber.ToString());
            PathsStatic.verRevit = uiApp.Application.VersionNumber.ToString();
            PathsStatic.uName = Environment.UserName;

            File.WriteAllText(path01.FileOkTxt, "");

            if (!modelPaths.Any())
            {
                Loger01.Write("CheckBpFiles Список modelPaths пуст");
                return res02;
            }

            foreach (var resultItem in modelPaths)
            {
                try
                {
                    string modelPath = resultItem[0] as string;
                    BasicFileInfo bfi = BasicFileInfo.Extract(modelPath);
                    Tuple<Document, string> docTuple = CommonClassBp.OpenDocumentWithDetach(app, modelPath, null);
                    Document cdoc = docTuple.Item1;

                    List<object> coordTemp = CommonClassBp.GetBp(cdoc);

                    FilteredElementCollector collector = new FilteredElementCollector(cdoc).OfCategory(BuiltInCategory.OST_ProjectBasePoint);
                    string list_last_changed = "";
                    foreach (Element bp in collector)
                    {
                        WorksharingTooltipInfo info = WorksharingUtils.GetWorksharingTooltipInfo(cdoc, bp.Id);
                        list_last_changed = list_last_changed + info.LastChangedBy + " ";
                    }
                    list_last_changed = list_last_changed.Trim();
                    if (!coordTemp.Take(3).SequenceEqual(resultItem.Skip(1).Take(3)))
                    //if (!coordTemp.SequenceEqual(resultItem.Skip(1).Take(4)))
                    {
                        filesChecked.Add(modelPath);
                        File.AppendAllText(path01.FileOkTxt, $"У файла не совпадает базовая точка {modelPath} \n");
                        List<object> templist = new List<object>
                        {
                            modelPath,
                            //Math.Round(Convert.ToDouble(resultItem[1]) - Convert.ToDouble(coordTemp[0]),5),
                            //Math.Round(Convert.ToDouble(resultItem[2]) - Convert.ToDouble(coordTemp[1]),5),
                            //Math.Round(Convert.ToDouble(resultItem[3]) - Convert.ToDouble(coordTemp[2]),5),
                            //Math.Round(Convert.ToDouble(resultItem[4]) - Convert.ToDouble(coordTemp[3]),5),

                            roundToFive(roundToFive(resultItem[1]) - roundToFive(coordTemp[0])),
                            roundToFive(roundToFive(resultItem[2]) - roundToFive(coordTemp[1])),
                            roundToFive(roundToFive(resultItem[3]) - roundToFive(coordTemp[2])),
                            roundToFive(roundToFive(resultItem[4]) - roundToFive(coordTemp[3])),
                            list_last_changed
                            //без округления
                            //Convert.ToDouble(resultItem[1]) - Convert.ToDouble(coordTemp[0]), 
                            //Convert.ToDouble(resultItem[2]) - Convert.ToDouble(coordTemp[1]), 
                            //Convert.ToDouble(resultItem[3]) - Convert.ToDouble(coordTemp[2]), 
                            //Convert.ToDouble(resultItem[4]) - Convert.ToDouble(coordTemp[3]) 
                        };
                        res02.Add(templist);
                    }
                    else
                    {
                        filesChecked.Add(modelPath);
                        File.AppendAllText(path01.FileOkTxt, $"Файл прошел проверку {modelPath} \n");
                    }


                    cdoc.Close(false);
                }
                catch (Exception ex)
                {
                    string modelPath = resultItem[0] as string;
                    filesError.Add(modelPath);
                    Loger01.Write($"111111111 {modelPath}");
                    Loger01.Write($"CheckBpFiles Working Error processing file {resultItem}: {ex.Message}");
                }
            }

            return res02;
        }

    }

}