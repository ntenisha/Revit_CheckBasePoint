using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckBasePoint
{
    internal class StartUpCleanEvent : IExternalEventHandler
    {
        public void Execute(UIApplication uiApp)
        {
            Loger01.Write("StartUpCleanEvent start");

            if (DateTime.Now.Hour > 8) {
                Loger01.Write("DateTime.Now.Hour" + DateTime.Now.Hour.ToString());
                return;
            }
            Initialized_TimerDlg(uiApp);
        }

        protected void Initialized_TimerDlg(UIApplication uiApp)
        {

            Loger01.Write("TimerDlg: Export will start in seconds 60");
            if (new TimerDlg.TimerDlg("CheckBasePoint", "Check will start in seconds", 60).ShowDialog() == true)
            {
                Loger01.Write("TimeDlg: Ok");
                ExecuteCheckBpWorkingFiles(uiApp);
            }
            else Loger01.Write("TimeDlg: Cancel");
        }

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

            ClosingRevit();
        }

        private static void ClosingRevit()
        {
            Loger01.Write("TimerDlg: Closing Revit");
            if (new TimerDlg.TimerDlg("CheckBasePoint", "Closing Revit", 60).ShowDialog() == true)
            {
                System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            }
        }

        public string GetName()
        {
            return nameof(StartUpCleanEvent);
        }
    }
}
