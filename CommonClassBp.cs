using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using Autodesk.Revit.UI.Events;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Contexts;


namespace CheckBasePoint
{
    [Transaction(TransactionMode.Manual)]

    public class CommonClassBp
    {

        public static void WriteResultsToJsonFileWorking(string outputPath, List<List<object>> results)
        {
            if (!File.Exists(outputPath))
            {
                File.Create(outputPath).Close();
            }

            try
            {
                List<Dictionary<string, object>> resultObjects = new List<Dictionary<string, object>>();

                foreach (List<object> result in results)
                {
                    Dictionary<string, object> resultDict = new Dictionary<string, object>
                {
                    { "PathToModel", result[0] },
                    { "Data", DateTime.Now.ToString("yyyy.MM.dd") },
                    { "DeltaEastWest", result[1] },
                    { "DeltaNorthSouth", result[2] },
                    { "DeltaElevation", result[3] },
                    { "DeltaAngleToNorth", result[4] },
                    { "LastChangedBy", result[5] }
                };
                    resultObjects.Add(resultDict);
                }

                Dictionary<string, List<Dictionary<string, object>>> jsonResult = new Dictionary<string, List<Dictionary<string, object>>>
            {
                { "Items", resultObjects }
            };

                string jsonText = JsonConvert.SerializeObject(jsonResult, Formatting.Indented);

                File.WriteAllText(outputPath, jsonText);

            }
            catch (Exception ex)
            {
                Loger01.Write("Произошла ошибка при записи в файл: " + ex.Message);
            }
        }

        public static void AddResultsFileNotFoundToJsonFile(string outputPath, List<string> results)
        {
            List<Dictionary<string, object>> resultObjectsFileNotFound = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> resultObjectsItems = new List<Dictionary<string, object>>();

            if (File.Exists(outputPath))
            {
                try
                {
                    // Чтение существующего содержимого файла
                    string existingJsonText = File.ReadAllText(outputPath);
                    var existingJsonResult = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(existingJsonText);

                    if (existingJsonResult != null)
                    {
                        if (existingJsonResult.ContainsKey("Items"))
                        {
                            resultObjectsItems = existingJsonResult["Items"];
                        }

                        if (existingJsonResult.ContainsKey("FileNotFound"))
                        {
                            resultObjectsFileNotFound = existingJsonResult["FileNotFound"];
                        }
                    }
                }
                catch (Exception ex)
                {
                    Loger01.Write("Произошла ошибка при чтении существующего файла: " + ex.Message);
                }
            }

            try
            {
                // Добавление новых данных к существующим
                foreach (string result in results)
                {
                    Dictionary<string, object> resultDict = new Dictionary<string, object>
                        {
                            { "PathToModel", result },
                            { "Data", DateTime.Now.ToString("yyyy.MM.dd") }
                        };
                    resultObjectsFileNotFound.Add(resultDict);
                }

                Dictionary<string, List<Dictionary<string, object>>> jsonResult = new Dictionary<string, List<Dictionary<string, object>>>
                    {
                        { "Items", resultObjectsItems },
                        { "FileNotFound", resultObjectsFileNotFound }
                    };


                string jsonText = JsonConvert.SerializeObject(jsonResult, Formatting.Indented);

                // Запись обратно в файл
                File.WriteAllText(outputPath, jsonText);
            }
            catch (Exception ex)
            {
                Loger01.Write("Произошла ошибка при записи в файл: " + ex.Message);
            }
        }




        public static void WriteResultsToJsonFile(string outputPath, List<List<object>> results)
        {
            if (!File.Exists(outputPath))
            {
                File.Create(outputPath).Close();
            }
            try
            {
                List<Dictionary<string, object>> resultObjects = new List<Dictionary<string, object>>();

                foreach (List<object> result in results)
                {
                    Dictionary<string, object> resultDict = new Dictionary<string, object>
                {
                    { "PathToCoordFile", result[0] },
                    { "EasyName", result[1] },
                    { "EastWestParam", result[2] },
                    { "NorthSouthParam", result[3] },
                    { "ElevationParam", result[4] },
                    { "AngleToNorthParam", result[5] }
                };
                    resultObjects.Add(resultDict);
                }

                Dictionary<string, List<Dictionary<string, object>>> jsonResult = new Dictionary<string, List<Dictionary<string, object>>>
            {
                { "Items", resultObjects }
            };

                string jsonText = JsonConvert.SerializeObject(jsonResult, Formatting.Indented);

                File.WriteAllText(outputPath, jsonText);

            }
            catch (Exception ex)
            {
                Loger01.Write("Произошла ошибка при записи в файл: " + ex.Message);
            }
        }
        public static void LoggerTxt(string outputPath, string message)
        {
            string logMessage = $"{DateTime.Now} {message} успешно выполнен";
            if (!File.Exists(outputPath))
            {
                File.Create(outputPath).Close();
            }
            try
            {
                File.AppendAllText(outputPath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Loger01.Write($"Ошибка записи в файл лога: {ex.Message}");
            }
        }

        public static List<object> GetBp(Document document)
        {
            List<object> result = new List<object>();
            ElementCategoryFilter categoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_ProjectBasePoint);
            FilteredElementCollector collector = new FilteredElementCollector(document);
            List<Element> oProjectBasePoints = collector.WherePasses(categoryFilter).ToElements().ToList();

            if (oProjectBasePoints.Count > 0)
            {
                Element oProjectBasePoint = oProjectBasePoints[0];

                Parameter eastWestParam = oProjectBasePoint.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM);
                Parameter northSouthParam = oProjectBasePoint.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM);
                Parameter elevationParam = oProjectBasePoint.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM);
                Parameter angleToNorthParam = oProjectBasePoint.get_Parameter(BuiltInParameter.BASEPOINT_ANGLETON_PARAM);

                //double x = eastWestParam.AsDouble();
                //double y = northSouthParam.AsDouble();
                //double z = elevationParam.AsDouble();
                //double r = angleToNorthParam.AsDouble();

                // округление до 5 знаков
                double x = Math.Round(eastWestParam.AsDouble(), 5);
                double y = Math.Round(northSouthParam.AsDouble(), 5);
                double z = Math.Round(elevationParam.AsDouble(), 5);
                double r = Math.Round(angleToNorthParam.AsDouble(), 5);

                result.Add(x);
                result.Add(y);
                result.Add(z);
                result.Add(r);
            }

            return result;
        }

        public static Tuple<Document, string> OpenDocBackground(
            Autodesk.Revit.ApplicationServices.Application application,
            string modelPath,
            string tempDir = null)
        {
            string datestamp = DateTime.Now.Ticks.ToString();
            string fileExtension = Path.GetExtension(modelPath);
            string fileBaseName = Path.GetFileNameWithoutExtension(modelPath);
            string newLocalFileName = $"{fileBaseName}_{application.Username}_{datestamp}{fileExtension}";
            string newLocalFullpath = Path.Combine(tempDir ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), newLocalFileName);

            ModelPath local = ModelPathUtils.ConvertUserVisiblePathToModelPath(newLocalFullpath);
            ModelPath central = ModelPathUtils.ConvertUserVisiblePathToModelPath(modelPath);
            WorksharingUtils.CreateNewLocal(central, local);
            string localStr = ModelPathUtils.ConvertModelPathToUserVisiblePath(local);

            Document docBackground = application.OpenDocumentFile(localStr);
            return new Tuple<Document, string>(docBackground, newLocalFullpath);
        }

        public static void SyncWithoutRelinquishing(Document doc)
        {
            TransactWithCentralOptions transOpts = new TransactWithCentralOptions();
            SynchLockCallback transCallBack = new SynchLockCallback();
            transOpts.SetLockCallback(transCallBack);

            SynchronizeWithCentralOptions syncOpts = new SynchronizeWithCentralOptions();
            RelinquishOptions relinquishOpts = new RelinquishOptions(false);
            syncOpts.SetRelinquishOptions(relinquishOpts);
            syncOpts.SaveLocalAfter = false;
            syncOpts.Comment = "Get BP";

            try
            {


                doc.SynchronizeWithCentral(transOpts, syncOpts);
            }
            catch (Exception ex)
            {
                Loger01.LogEx(ex);
            }
        }

        class SynchLockCallback : ICentralLockedCallback
        {
            public bool ShouldWaitForLockAvailability()
            {


                return false;
            }

        }

        public static Tuple<Document, string> OpenDocumentWithDetach(Autodesk.Revit.ApplicationServices.Application app, string modelPath, WorksetConfiguration worksetConfiguration = null, string tempDir = null)
        {
            //WorksetConfigurationOption.CloseAllWorksets) - с закрытием всех раб наборов
            //WorksetConfigurationOption.OpenAllWorksets) - с открытием всех раб наборов
            //WorksetConfigurationOption.OpenLastViewed) - с открытием последних просмотренных

            if (string.IsNullOrEmpty(tempDir))
            {
                tempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CheckBasePoint_temp_dir");
            }

            string fileNameWithExtension = Path.GetFileName(modelPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameWithExtension);

            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            string tempFilePath = Path.Combine(tempDir, fileNameWithExtension);

            File.Copy(modelPath, tempFilePath, true);

            ModelPath modelPathObj = ModelPathUtils.ConvertUserVisiblePathToModelPath(tempFilePath);
            OpenOptions openOptions = new OpenOptions
            {
                DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets
            };

            if (worksetConfiguration == null)
            {
                worksetConfiguration = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);
            }
            openOptions.SetOpenWorksetsConfiguration(worksetConfiguration);

            Document docBackground = app.OpenDocumentFile(modelPathObj, openOptions);

            return new Tuple<Document, string>(docBackground, tempFilePath);
        }

        public static async void Application_DialogBoxShowing(object sender, DialogBoxShowingEventArgs e)
        {
            try
            {

                switch (e)
                {
                    case TaskDialogShowingEventArgs args:
                        //Loger01.Write($"Обработка окна {args.DialogId}...");

                        if (args.DialogId == "TaskDialog_Unresolved_References")
                        {
                            args.OverrideResult(1002);
                        }
                        else if (args.DialogId == "TaskDialog_File_Name_In_Use")
                        {
                            args.OverrideResult(1001);
                        }
                        else if (args.DialogId == "TaskDialog_Changes_Not_Saved")
                        {
                            args.OverrideResult(1001);
                        }
                        else if (args.DialogId == "TaskDialog_Missing_Third_Party_Updaters")
                        {
                            args.OverrideResult(1001);
                        }
                        else
                        {
                            // сюда попадают в т.ч. пользовательские таск диалоги, они не имеют DialogId, Message

                            Loger01.Write($"Необработанное окно -  {args.DialogId}. Текст окна - '{args.Message}', Cancellable: {args.Cancellable}. Использую код 1001");
                            args.OverrideResult(1001);
                        }


                        break;

                    case DialogBoxShowingEventArgs args2:
                        //Loger01.Write($"Обработка окна {args2.DialogId}...");

                        if (args2.DialogId == "Dialog_Revit_DocWarnDialog")
                        {
                            Loger01.Write($"{args2.DialogId}. Использую Win32Api.ClickOk()");
                            await Win32Api.ClickOk();
                        }
                        else if (args2.DialogId == "Dialog_Revit_PartitionsSaveToMaster")
                        {
                            args2.OverrideResult(1);
                        }
                        else
                        {
                            Loger01.Write($"Необработанное окно - {args2.DialogId}. Использую код 1");
                            args2.OverrideResult(1);
                        }

                        break;


                    default:
                        Loger01.Write($"Необработанное окно - {e.DialogId}");
                        return;
                }

            }
            catch (Exception ex)
            {
                Loger01.Write($"Не удалось выполнить метод {nameof(Application_DialogBoxShowing)}. Подробнее:  {ex.Message} \n {ex.StackTrace}");
            }

        }

        public static class Win32Api
        {
            // A delegate which is used by EnumChildWindows to execute a callback method.
            public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);

            [DllImport("user32")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i);

            [DllImport("user32.dll", EntryPoint = "GetWindowText", CharSet = CharSet.Auto)]
            private static extern IntPtr GetWindowCaption(IntPtr hWnd, StringBuilder lpString, int maxCount);

            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
            private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

            public static async Task ClickOk()
            {
                const string windowTitle = "Autodesk Revit 2020";
                while (FindWindowByCaption(IntPtr.Zero, windowTitle) == IntPtr.Zero)
                {
                    await DelayWork(100);
                }

                // Loop though the child windows, and execute the EnumChildWindowsCallback method
                EnumChildWindows(FindWindowByCaption(IntPtr.Zero, windowTitle), EnumChildWindowsCallback, IntPtr.Zero);
            }

            #region Utilities

            private static bool EnumChildWindowsCallback(IntPtr handle, IntPtr pointer)
            {
                const uint WM_LBUTTONDOWN = 0x0201;
                const uint WM_LBUTTONUP = 0x0202;

                var sb = new StringBuilder(256);

                // Get the control's text.
                GetWindowCaption(handle, sb, 256);
                var text = sb.ToString();

                // If the text on the control == &OK send a left mouse click to the handle.
                if (text != @"&OK")
                    return true;

                PostMessage(handle, WM_LBUTTONDOWN, IntPtr.Zero, IntPtr.Zero);
                PostMessage(handle, WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);

                return true;
            }

            private static async Task DelayWork(int i)
            {
                await Task.Delay(i);
            }

            #endregion
        }


        public static void DeleteFilesAndSubfolders(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    // Удаляем все файлы в папке
                    string[] files = Directory.GetFiles(folderPath);
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }

                    // Рекурсивно удаляем все подпапки и их содержимое
                    string[] subfolders = Directory.GetDirectories(folderPath);
                    foreach (string subfolder in subfolders)
                    {
                        DeleteFilesAndSubfolders(subfolder);
                    }

                    // Удаляем саму папку
                    Directory.Delete(folderPath);

                    //Loger01.Write("Все файлы и подпапки в указанной папке были удалены.");
                }
                else
                {
                    Loger01.Write("Указанная папка не существует.");
                }
            }
            catch (Exception ex)
            {
                Loger01.Write("Произошла ошибка при удалении файлов и папок: " + ex.Message);
            }
        }




    }

}