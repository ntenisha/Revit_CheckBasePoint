using System;
using System.IO;
using System.Text;

namespace CheckBasePoint
{
    public class Loger01
    {
        private static object sync = new object();
        public static void LogEx(Exception ex)
        {
            try
            {
                // Путь .\\Log
                //*string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
                string pathToLog = PathsStatic.errLog;
                if (!Directory.Exists(pathToLog))
                    Directory.CreateDirectory(pathToLog); // Создаем директорию, если нужно
                string filename = Path.Combine(pathToLog, "exLog" + PathsStatic.verRevit + ".txt");
                //string fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] [{1}.{2}()] {3} {4}\r\n", DateTime.Now, ex.TargetSite.DeclaringType, ex.TargetSite.Name, ex.Message , ex.StackTrace);
                string fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] [{1}.{2}()] {3} {4} {5} {6}\r\n", DateTime.Now, PathsStatic.verRevit, PathsStatic.uName, ex.TargetSite.DeclaringType, ex.TargetSite.Name, ex.Message, ex.StackTrace);
                lock (sync)
                {
                    File.AppendAllText(filename, fullText, Encoding.GetEncoding("Windows-1251"));
                }
            }
            catch
            {
                // Перехватываем все и ничего не делаем
            }
        }
        public static void Write(string Text)
        {
            try
            {
                // Путь .\\Log
                //*string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");

                string pathToLog = PathsStatic.errLog;
                if (!Directory.Exists(pathToLog))
                    Directory.CreateDirectory(pathToLog); // Создаем директорию, если нужно
                string filename = Path.Combine(pathToLog, "exLog" + PathsStatic.verRevit + ".txt");
                //string fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] {1}\r\n", DateTime.Now, Text);
                string fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] {1} {2} {3}\r\n", DateTime.Now, PathsStatic.verRevit, PathsStatic.uName, Text);
                lock (sync)
                {
                    File.AppendAllText(filename, fullText, Encoding.GetEncoding("Windows-1251"));
                }
            }
            catch
            {
                // Перехватываем все и ничего не делаем
            }
        }

        public static void FileNotFound(string Text)
        {
            try
            {
                // Путь .\\Log
                //*string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");

                string pathToLog = PathsStatic.errLog;
                if (!Directory.Exists(pathToLog))
                    Directory.CreateDirectory(pathToLog); // Создаем директорию, если нужно
                string filename = Path.Combine(pathToLog, "Файл_не_найден" + PathsStatic.verRevit + ".txt");
                //string fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] {1}\r\n", DateTime.Now, Text);
                string fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] {1} {2} {3}\r\n", DateTime.Now, PathsStatic.verRevit, PathsStatic.uName, Text);
                lock (sync)
                {
                    File.AppendAllText(filename, fullText, Encoding.GetEncoding("Windows-1251"));
                }
            }
            catch
            {
                // Перехватываем все и ничего не делаем
            }
        }
    }
}