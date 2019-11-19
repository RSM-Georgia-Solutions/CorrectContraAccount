﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using Application = SAPbouiCOM.Framework.Application;

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
            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
        }

        private SAPbouiCOM.Button Button0;
        private Solver _solver;
        private void OnCustomInitialize()
        {
            _solver = new Solver();
        }

        private void Button0_PressedAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            //ვიღებთ ტრანზაქციას და ვწერთ მოდელში
            List<JournalEntryLineModel> jdtLines = new List<JournalEntryLineModel>();
            Recordset recSet = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSet.DoQuery($@"SELECT *
            FROM JDT1
            LEFT JOIN OJDT ON JDT1.TransId = OJDT.TransId
            WHERE CONVERT(DATE, ojdt.RefDate) >= '{EditText0.Value}'

            AND CONVERT(DATE, ojdt.RefDate) <= '{EditText1.Value}'");             
            
            // recSet.DoQuery($@"SELECT * FROM JDT1 WHERE TransId = 3209");
            while (!recSet.EoF)
            {
                JournalEntryLineModel model = new JournalEntryLineModel
                {
                    Account = recSet.Fields.Item("Account").Value.ToString(),
                    ContraAccount = recSet.Fields.Item("ContraAct").Value.ToString(),
                    Credit = double.Parse(recSet.Fields.Item("Credit").Value.ToString()),
                    Debit = double.Parse(recSet.Fields.Item("Debit").Value.ToString()),
                    SortName = recSet.Fields.Item("ShortName").Value.ToString(),
                    TransId = int.Parse(recSet.Fields.Item("TransId").Value.ToString()),
                    LineId = int.Parse(recSet.Fields.Item("Line_ID").Value.ToString())
                };
                jdtLines.Add(model);
                recSet.MoveNext();
            }

            //ვაჯგუფებთ ტრანზაქციის ID-ს მიხედვით (ამოვაგდებთ სადაც დებიტი და კრედიტი 0 ის ტოლია)
            IEnumerable<IGrouping<int, JournalEntryLineModel>> groupBy = jdtLines
                .Where(y => y.Debit != 0 || y.Credit != 0)
                .GroupBy(x => x.TransId);
            int increment = 0;
            int total = groupBy.Count();
            foreach (IGrouping<int, JournalEntryLineModel> journalEntryLineModels in groupBy)
            {
                int transId = journalEntryLineModels.Key; //ტრანზაქციის ID
                List<JournalEntryLineModel> debitLines = journalEntryLineModels.Where(x => x.Debit != 0).Select(x => x).ToList();// სტრიქონები სადაც დებიტი არაა 0
                List<JournalEntryLineModel> creditLines = journalEntryLineModels.Where(x => x.Credit != 0).Select(x => x).ToList();// სტრიქონები სადაც კრედიტი არაა 0

                // ლოგიკა რომელიც გვიბრუნებს საჟურნალო გატარების სტრიქონებს რომლის ჯამიც გვაძლებს გადაცემული სტრიქონის თანხას

                while (debitLines.Count > 0 && creditLines.Count > 0)
                {
                    JournalEntryLineModel maxDebitLine = debitLines.Where(x => Math.Abs(x.Debit) == debitLines.Max(y => Math.Abs(y.Debit))).ToList().First();// მაქსიმალური დებიტის თანხა
                    JournalEntryLineModel maxCreditLine = creditLines.Where(x => Math.Abs(x.Credit) == creditLines.Max(y => Math.Abs(y.Credit))).ToList().First();// მაქსიმალური კრედიტის თანხა

                    if (maxCreditLine.Credit == maxDebitLine.Debit)
                    {
                       // maxCreditLine.CorrectContraAccount = maxDebitLine.Account;
                        maxDebitLine.CorrectContraAccount = maxCreditLine.Account;
                        maxDebitLine.ContraAccountLineId = maxCreditLine.LineId;
                        maxDebitLine.CorrectContraShortName = maxCreditLine.SortName;
                        //maxCreditLine.Update();
                     var x =   maxDebitLine.Update();
                    var y =    DiManager.Company.GetLastErrorDescription();
                        maxCreditLine.CorrectContraAccount = "SourceSimple";
                        maxCreditLine.ContraAccountLineId = -1;
                        maxCreditLine.Update();
                        creditLines.Remove(maxCreditLine);
                        debitLines.Remove(maxDebitLine);
                    }

                    if (Math.Abs(maxCreditLine.Credit) > Math.Abs(maxDebitLine.Debit))
                    {
                        List<JournalEntryLineModel> sources = _solver.SolveCombinations(maxCreditLine, debitLines);
                        foreach (JournalEntryLineModel journalEntryLineModel in sources)
                        {
                            journalEntryLineModel.CorrectContraAccount = maxCreditLine.Account;
                            journalEntryLineModel.CorrectContraShortName = maxCreditLine.SortName;
                            journalEntryLineModel.ContraAccountLineId = maxCreditLine.LineId;
                            journalEntryLineModel.Update();
                        }
                        maxCreditLine.CorrectContraAccount = "SourceComplex";
                        maxCreditLine.ContraAccountLineId = -1;
                        maxCreditLine.Update();
                        creditLines.Remove(maxCreditLine);
                        debitLines = debitLines.Except(sources).ToList();
                    }
                    else if (Math.Abs(maxDebitLine.Debit) > Math.Abs(maxCreditLine.Credit))
                    {
                        List<JournalEntryLineModel> sources = _solver.SolveCombinations(maxDebitLine, creditLines);
                        foreach (JournalEntryLineModel journalEntryLineModel in sources)
                        {
                            journalEntryLineModel.CorrectContraAccount = maxDebitLine.Account;
                            journalEntryLineModel.CorrectContraShortName = maxDebitLine.SortName;
                            journalEntryLineModel.ContraAccountLineId = maxDebitLine.LineId;
                            journalEntryLineModel.Update();
                        }
                        maxDebitLine.CorrectContraAccount = "SourceComplex";
                        maxDebitLine.ContraAccountLineId = -1;
                        maxDebitLine.Update();
                        debitLines.Remove(maxDebitLine);
                        creditLines = creditLines.Except(sources).ToList();
                    }
                }

                increment++;
                Application.SBO_Application.SetStatusBarMessage($"{increment} of {total}",
                    BoMessageTime.bmt_Short,false);
            }

            SAPbouiCOM.Framework.Application.SBO_Application.MessageBox("SUCCESS");
        }

        private EditText EditText0;
        private EditText EditText1;
        private StaticText StaticText0;
        private StaticText StaticText1;
    }
}