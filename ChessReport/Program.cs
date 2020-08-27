using System;
using System.Collections.Generic;
using SAPbouiCOM.Framework;
using SAPbobsCOM;
using CorrectContraAccountLogicDLL;
using System.Threading;

namespace ChessReport
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application oApp = null;
                oApp = args.Length < 1 ? new Application() : new Application(args[0]);
                Menu MyMenu = new Menu();
                MyMenu.AddMenuItems();
                oApp.RegisterMenuEventHandler(MyMenu.SBO_Application_MenuEvent);
                Application.SBO_Application.AppEvent += new SAPbouiCOM._IApplicationEvents_AppEventEventHandler(SBO_Application_AppEvent);
                var company = DiManager.Company;
                DiManager.CreateTable("RSM_CRHY", "Contra Recalculation", SAPbobsCOM.BoUTBTableType.bott_NoObjectAutoIncrement);//table of contra recalculation histort

                DiManager.CreateField("RSM_CRHY", "Date", "თარიღი", BoFieldTypes.db_Date, 20, false, true);
                DiManager.CreateField("RSM_CRHY", "Executed", "შესრულებული", BoFieldTypes.db_Alpha, 20, false, true);
                DiManager.Recordset.DoQuery($"select * from [@RSM_CRHY] WHERE U_Date = '{DateTime.Today.ToString("s")}'");
                if (DiManager.Recordset.RecordCount == 0)
                {

                    Thread xz = new Thread(Recalculation);
                    xz.Start(); 
                }


                oApp.Run();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private static void Recalculation()
        {
            CorrectionLogic logic = new CorrectionLogic(DiManager.Company);
            logic.CorrectionJournalEntries(new CorrectionJournalEntriesParams { MaxLine = 10000, MustSkip = true, StartDate = new DateTime(2020, 1, 1), EndDate = DateTime.Today, WaitingTimeInMinutes = 3 });
            logic.CorrectionJournalEntriesSecondLogic(new CorrectionJournalEntriesParams { MaxLine = 10000, MustSkip = true, StartDate = new DateTime(2020, 1, 1), EndDate = DateTime.Today, WaitingTimeInMinutes = 3 });

            string query = $"insert into [@RSM_CRHY] (U_Date, U_Executed) Values('{DateTime.Today.ToString("s")}', 'True')";
            DiManager.Recordset.DoQuery(query);
        }

        static void SBO_Application_AppEvent(SAPbouiCOM.BoAppEventTypes EventType)
        {
            switch (EventType)
            {
                case SAPbouiCOM.BoAppEventTypes.aet_ShutDown:
                    //Exit Add-On
                    System.Windows.Forms.Application.Exit();
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_FontChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_LanguageChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition:
                    break;
                default:
                    break;
            }
        }
    }
}
