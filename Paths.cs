using System.IO;

namespace CheckBasePoint
{

    public class Paths
    {
        public static string verRevit;

        public static readonly string DirectoryPath =       Path.Combine("X:\\01_Скрипты\\04_BIM\\00_Запуск\\CheckBasePoint\\");
        public static readonly string bpFilePath =          Path.Combine(DirectoryPath, "Json\\Check_Bp_coord_files"     + verRevit + ".Json");
        public static readonly string workingFilePath =     Path.Combine(DirectoryPath, "Json\\Check_Bp_working_files"   + verRevit + ".Json");
        public static readonly string bpFilePathUser =      Path.Combine(DirectoryPath, "Check_Bp_coord_files"           + verRevit + ".txt");
        public static readonly string workingFilePathUser = Path.Combine(DirectoryPath, "Check_Bp_working_files"         + verRevit + ".txt");
        public static readonly string logFile =             Path.Combine(DirectoryPath, "Check_Bp_результат"             + verRevit + ".Json");
        public static readonly string errLog =              Path.Combine(DirectoryPath);


    }
}
