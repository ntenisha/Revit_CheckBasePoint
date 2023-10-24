using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Reflection;


namespace CheckBasePoint
{
    public class ExternalApplication : IExternalApplication
    {
        public static readonly string PATH_LOCACTION = Assembly.GetExecutingAssembly().Location;

        public void Initialized(object sender, ApplicationInitializedEventArgs e)

        {
#pragma warning disable IDE0067 // Ликвидировать объекты перед потерей области
            ExternalEvent.Create(new StartUpCleanEvent()).Raise();
#pragma warning restore IDE0067 // Ликвидировать объекты перед потерей области
        }

        public Result OnStartup(UIControlledApplication application)
        {

            Paths path01 = new Paths(application.ControlledApplication.VersionNumber.ToString());
            PathsStatic.verRevit = application.ControlledApplication.VersionNumber.ToString();
            Loger01.Write("Application startup " + Assembly.GetExecutingAssembly().GetName().Version.ToString());

            application.ControlledApplication.ApplicationInitialized += new EventHandler<ApplicationInitializedEventArgs>(Initialized);

            RibbonPanel ribbonPanel = application.CreateRibbonPanel("CheckBasePoint");

            PushButtonData buttonDataExport = new PushButtonData("Coord",   "Get Base point",        PATH_LOCACTION, "CheckBasePoint.GetBpFromCoordFiles");
            PushButtonData buttonDataConfig = new PushButtonData("Working", "Check working files",   PATH_LOCACTION, "CheckBasePoint.CheckBpWorkingFiles");

            buttonDataExport.AvailabilityClassName = "CheckBasePoint.AvailabilityClass";
            buttonDataConfig.AvailabilityClassName = "CheckBasePoint.AvailabilityClass";

            //buttonDataExport.Image = new System.Windows.Media.Imaging.BitmapImage(new Uri(Path.Combine(PATH_LOCACTIONDIR, "images\\export.png")));
            //buttonDataConfig.Image = new System.Windows.Media.Imaging.BitmapImage(new Uri(Path.Combine(PATH_LOCACTIONDIR, "images\\config.png")));

            List<RibbonItem> projectButtons = new List<RibbonItem>();
            projectButtons.AddRange(ribbonPanel.AddStackedItems(buttonDataExport, buttonDataConfig));
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            application.ControlledApplication.ApplicationInitialized -= Initialized;
            Loger01.Write("Application shutdown\n");
            return Result.Succeeded;
        }
    }

    public class AvailabilityClass : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories) => true;
    }
}
