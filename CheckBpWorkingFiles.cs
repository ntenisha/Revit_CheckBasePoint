using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace CheckBasePoint
{
    [Transaction(TransactionMode.Manual)]

    public class CheckBpWorkingFiles : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Paths.verRevit = uiApp.Application.VersionNumber.ToString();


            RunWF(uiApp);
            return Result.Succeeded;
        }


        public static void RunWF(UIApplication uiApp)
        {
            string _appversion = uiApp.Application.VersionNumber;
            string logFile = Paths.logFile;

            Loger01.Write("Запущен CheckBpWorkingFiles");

            try
            {
                uiApp.DialogBoxShowing += CommonClassBp.Application_DialogBoxShowing;
                //List<List<object>> resultsFromWf = CombineDataFromBpAndWfFiles(bpFilePath, workingFilePath);
                //List<List<object>> resultsFromWf = CombineDataFromBpAndWfFiles02(bpFilePath, "X:\\01_Скрипты\\04_BIM\\00_Запуск\\CheckBasePoint\\222.txt");
                List<List<object>> resultsFromWf = CombineDataFromBpAndWfFiles02(Paths.bpFilePath, Paths.workingFilePathUser);
                //List<string> resCheck = CheckBpFiles(uiApp, resultsFromWf);
                List<List<object>> resCheck = CheckBpFiles(uiApp, resultsFromWf);

                uiApp.DialogBoxShowing -= CommonClassBp.Application_DialogBoxShowing;

                CommonClassBp.WriteResultsToJsonFileWorking(logFile, resCheck);
                Loger01.Write("Завершен CheckBpWorkingFiles\n");
            }
            catch (Exception ex)
            {
                Loger01.Write("Ошибка в методе RunWF");
                Loger01.LogEx(ex);

            }

        }

        public static List<List<object>> ReadFileWithPipeDelimiter(string filePath)
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
                        string[] parts = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length == 2)
                        {
                            string temp01 = ParsePaths(parts[0]);
                            string temp02 = ParseValueInQuotesOrTrim(parts[1]);
                            Loger01.Write("temp01" + temp01.ToString());
                            Loger01.Write("temp02" + temp02.ToString());
                            List<object> row = new List<object> { temp01, temp02 };
                            result.Add(row);
                        }
                        else
                        {
                            Loger01.Write($"Invalid line: {line}");
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

            string trimmedText = inputText.Trim(); // Удалить начальные и конечные пробелы
            trimmedText = trimmedText.Trim(','); // Удалить начальные и конечные запятые
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
                // Обрезка пробелов, запятых и еще раз пробелов
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

/*        public static List<List<object>> CombineDataFromBpAndWfFiles(string bpFilePath, string WorkingFilePath)
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
                item.EasyName
            };

                    var matchingItem = resultsFromBp.FirstOrDefault(bpItem => bpItem[0].ToString() == item.EasyName);

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
        }*/

        public static List<List<object>> CombineDataFromBpAndWfFiles02(string bpFilePath, string WorkingFilePath)
        {
            List<List<object>> combinedResults = new List<List<object>>();
            List<List<object>> resultsFromBp = ReadBpFile(bpFilePath);
            List<List<object>> resultsFromWf = ReadFileWithPipeDelimiter(WorkingFilePath);

            if (resultsFromBp == null)
            {
                Loger01.Write("Ошибка при чтении данных из файла с результатами ReadBpFile");
                return combinedResults;
            }
            else if (resultsFromWf == null)
            {
                Loger01.Write("Ошибка при чтении данных из файла с результатами WorkingFilePath");
                return combinedResults;
            }

            foreach (List<object> wfItem in resultsFromWf)
            {
                bool foundMatch = false;

                foreach (List<object> bpItem in resultsFromBp)
                {
                    // Сравниваем второй элемент из wfItem с первым элементом из bpItem
                    if (wfItem[1].Equals(bpItem[0]))
                    {
                        // Найдено совпадение, создаем новый список и добавляем нужные элементы
                        List<object> combinedItem = new List<object>
                        {
                            wfItem[0], 
                            wfItem[1], 
                            bpItem[2],
                            bpItem[3],
                            bpItem[4]  
                        };

                        combinedResults.Add(combinedItem);
                        foundMatch = true;
                        break; // Если совпадение найдено, выходим из внутреннего цикла
                    }
                }
                if (!foundMatch)
                {
                    Loger01.Write($"Ошибка нет совпадения" + wfItem[1].ToString());
                }
            }
            return combinedResults;
        }


        public static List<List<object>> CheckBpFiles(UIApplication uiApp, List<List<object>> modelPaths)
        {
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            List<List<object>> res02 = new List<List<object>>();

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
                            List<object> templist = new List<object> { 
                                modelPath, 
                                Convert.ToDouble(resultItem[1]) - Convert.ToDouble(coordTemp[0]), 
                                Convert.ToDouble(resultItem[2]) - Convert.ToDouble(coordTemp[1]), 
                                Convert.ToDouble(resultItem[3]) - Convert.ToDouble(coordTemp[2]), 
                                Convert.ToDouble(resultItem[4]) - Convert.ToDouble(coordTemp[3]) 
                            };
                            res02.Add(templist);
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


/*        public static List<string> CheckBpFiles(UIApplication uiApp, List<List<object>> modelPaths)
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
                            //List<object> templist = (modelPath,);
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
        }*/




    }

}