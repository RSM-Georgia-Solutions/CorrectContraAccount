using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using Application = SAPbouiCOM.Framework.Application;
using System.Diagnostics;
using System.Timers;
using System.Threading.Tasks;
using CorrectContraAccountLogicDLL;

namespace ChessReport
{
    [FormAttribute("ChessReport.Form1", "Form1.b1f")]
    class Form1 : UserFormBase
    {
        public Form1()
        {
        }

        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            this.Button0 = ((SAPbouiCOM.Button)(this.GetItem("Item_0").Specific));
            this.Button0.PressedAfter += new SAPbouiCOM._IButtonEvents_PressedAfterEventHandler(this.Button0_PressedAfter);
            this.EditText0 = ((SAPbouiCOM.EditText)(this.GetItem("Item_1").Specific));
            this.EditText1 = ((SAPbouiCOM.EditText)(this.GetItem("Item_2").Specific));
            this.StaticText0 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_3").Specific));
            this.StaticText1 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_4").Specific));
            this.CheckBox0 = ((SAPbouiCOM.CheckBox)(this.GetItem("Item_5").Specific));
            this.EditText2 = ((SAPbouiCOM.EditText)(this.GetItem("Item_6").Specific));
            this.StaticText2 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_7").Specific));
            this.Button1 = ((SAPbouiCOM.Button)(this.GetItem("Item_8").Specific));
            this.Button1.PressedAfter += new SAPbouiCOM._IButtonEvents_PressedAfterEventHandler(this.Button1_PressedAfter);
            this.StaticText3 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_9").Specific));
            this.EditText3 = ((SAPbouiCOM.EditText)(this.GetItem("Item_10").Specific));
            this.StaticText4 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_11").Specific));
            this.EditText4 = ((SAPbouiCOM.EditText)(this.GetItem("Item_12").Specific));
            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
        }

        private SAPbouiCOM.Button Button0;
        private void OnCustomInitialize()
        {
        }
        private void Button0_PressedAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            CorrectionLogic logic = new CorrectionLogic(DiManager.RoundAccuracy, DiManager.Company);
            int maxLine;
            int.TryParse(EditText2.Value,
               out maxLine);
            bool mustSkip = CheckBox0.Checked;
            string startDate = EditText0.Value;
            string endDate = EditText1.Value;
            string transIdParam = EditText3.Value;
            string waitingTimeString = EditText4.Value;
            int waitingTime = int.Parse(waitingTimeString);
            logic.CorrectionJournalEntries(maxLine, mustSkip, startDate, endDate, waitingTime, transIdParam);
        }
        private EditText EditText0;
        private EditText EditText1;
        private StaticText StaticText0;
        private StaticText StaticText1;
        private CheckBox CheckBox0;
        private EditText EditText2;
        private StaticText StaticText2;
        private Button Button1;

        private void Button1_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            CorrectionLogic logic = new CorrectionLogic(DiManager.RoundAccuracy, DiManager.Company);
            int maxLine;
            int.TryParse(EditText2.Value,
                out maxLine);
            bool mustSkip = CheckBox0.Checked;
            string startDate = EditText0.Value;
            string endDate = EditText1.Value;
            string transIdParam = EditText3.Value;
            string waitingTimeString = EditText4.Value;
            int waitingTime = int.Parse(waitingTimeString);
            if (!string.IsNullOrWhiteSpace(transIdParam))
            {
                logic.CorrectionJournalEntriesSecondLogic(transIdParam,
                    waitingTime);
            }
            else
            {
                logic.CorrectionJournalEntriesSecondLogic(maxLine, mustSkip, startDate, endDate, waitingTime);
            }
        }
        private StaticText StaticText3;
        private EditText EditText3;
        private StaticText StaticText4;
        private EditText EditText4;
    }
}