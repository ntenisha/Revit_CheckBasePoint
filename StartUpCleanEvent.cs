using Autodesk.Revit.UI;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace CheckBasePoint
{
    internal class StartUpCleanEvent : IExternalEventHandler
    {

        public void Execute(UIApplication uiApp)
        {
            PathsStatic.verRevit = uiApp.Application.VersionNumber.ToString();
            Thread.Sleep(10000);
            Loger01.Write("StartUpCleanEvent start");
            bool flag01 = false;
            string[] envArgs = Environment.GetCommandLineArgs();
            foreach (string arg in envArgs)
            {
                Loger01.Write(arg);
                if (arg.Contains("CheckBasePoint"))
                {
                    flag01 = true;
                    break;
                }
            }
            if (flag01 == false)
            {
                return;
            }
            //if (DateTime.Now.Hour > 8 || DateTime.Now.Hour < 4)
            //{
            //    Loger01.Write("DateTime.Now.Hour" + DateTime.Now.Hour.ToString());
            //    return;
            //}
            //Initialized_TimerDlg(uiApp);
            ExecuteCheckBpWorkingFiles(uiApp);
        }

        //protected void Initialized_TimerDlg(UIApplication uiApp)
        //{

        //Loger01.Write("TimerDlg: Export will start in seconds 60");
        //if (new TimerDlg.TimerDlg("CheckBasePoint", "Check will start in seconds", 60).ShowDialog() == true)
        //{
        //    Loger01.Write("TimeDlg: Ok");
        //    ExecuteCheckBpWorkingFiles(uiApp);
        //}
        //else Loger01.Write("TimeDlg: Cancel");
        //ExecuteCheckBpWorkingFiles(uiApp);
        //}

        private void ExecuteCheckBpWorkingFiles(UIApplication uiApp)
        {
            if (uiApp == null)
            {
                throw new Exception("uiApp is null!!!");
            }

            // удалить
            Loger01.Write("Execute CheckBpWorkingFiles :" + uiApp.ToString());


            try { CheckBpWorkingFiles.RunWF(uiApp); }
            catch (Exception e) { Loger01.Write("StartUp Exception:" + e); }
            //ClosingRevit();
            Thread.Sleep(20000);
            PressCloseWindow(uiApp.MainWindowHandle);

        }

        //private static void ClosingRevit()
        //{
        //    Loger01.Write("TimerDlg: Closing Revit");
        //    if (new TimerDlg.TimerDlg("CheckBasePoint", "Closing Revit", 60).ShowDialog() == true)
        //    {
        //        System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
        //    }
        //}


        public string GetName()
        {
            return nameof(StartUpCleanEvent);
        }


        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        const int WM_CLOSE = 0x10;

        public void PressCloseWindow(IntPtr revitWindowHandle)
        {
            PostMessage(revitWindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            //Loger01.Write("Start PressCloseWindow  ");
        }

    }
}
