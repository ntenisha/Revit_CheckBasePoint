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

            bool flag01 = false;
            string[] envArgs = Environment.GetCommandLineArgs();
            foreach (string arg in envArgs)
            {
                //Loger01.Write(arg);
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

            ExecuteCheckBpWorkingFiles(uiApp);
        }

        private void ExecuteCheckBpWorkingFiles(UIApplication uiApp)
        {
            if (uiApp == null)
            {
                throw new Exception("uiApp is null!!!");
            }

            try { CheckBpWorkingFiles.RunWF(uiApp); }
            catch (Exception e)
            {
                Loger01.Write("StartUp Exception:" + e);
            }
            //ClosingRevit();
            Thread.Sleep(20000);
            PressCloseWindow(uiApp.MainWindowHandle);

        }

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

        }

    }
}
