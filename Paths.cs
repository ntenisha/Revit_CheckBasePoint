using System.IO;

namespace CheckBasePoint
{
    public static class PathsStatic
    {
        public static string verRevit;
        public static string uName;
        public static string directoryPath = Path.Combine("X:\\01_Скрипты\\04_BIM\\00_Запуск\\CheckBasePoint\\");
        public static string errLog = Path.Combine(directoryPath);

    }

    public class Paths
    {
        private string _verRevit;
        public string BpFilePath { get; private set; }
        public string WorkingFilePath { get; private set; }
        public string BpFilePathUser { get; private set; }
        public string WorkingFilePathUser { get; private set; }
        public string LogFile { get; private set; }
        public string ErrLog { get; private set; }

        public Paths(string verRevit)
        {
            _verRevit = verRevit;

            var directoryPath = Path.Combine("X:\\01_Скрипты\\04_BIM\\00_Запуск\\CheckBasePoint\\");

            BpFilePath = Path.Combine(directoryPath, "Json\\Check_Bp_coord_files" + verRevit + ".Json");
            WorkingFilePath = Path.Combine(directoryPath, "Json\\Check_Bp_working_files" + verRevit + ".Json");
            BpFilePathUser = Path.Combine(directoryPath, "Check_Bp_coord_files" + verRevit + ".txt");
            WorkingFilePathUser = Path.Combine(directoryPath, "Check_Bp_working_files" + verRevit + ".txt");
            LogFile = Path.Combine(directoryPath, "Check_Bp_результат" + verRevit + ".Json");
            ErrLog = Path.Combine(directoryPath);


        }

    }
}
