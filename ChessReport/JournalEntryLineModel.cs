﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;

namespace ChessReport
{
    public class JournalEntryLineModel
    {
        public int TransId { get; set; }
        public int LineId { get; set; }
        public string Account { get; set; }
        public double Debit { get; set; }
        public double Credit { get; set; }
        public string ContraAccount { get; set; }
        public string SortName { get; set; }
        public string CorrectContraAccount { get; set; }
        public string CorrectContraShortName { get; set; }
        public int ContraAccountLineId { get; set; }

        public int Update()
        {
            JournalEntries journalEntry =(JournalEntries)DiManager.Company.GetBusinessObject(BoObjectTypes.oJournalEntries);
            journalEntry.GetByKey(TransId);
            journalEntry.Lines.SetCurrentLine(LineId);
            journalEntry.Lines.UserFields.Fields.Item("U_CorrectContraAcc").Value = CorrectContraAccount;
            journalEntry.Lines.UserFields.Fields.Item("U_CorrectContraShortName").Value = CorrectContraShortName??"";
            journalEntry.Lines.UserFields.Fields.Item("U_ContraAccountLineId").Value = ContraAccountLineId.ToString();
            return journalEntry.Update();
        }
        public void UpdateSql()
        {
            Recordset rec = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            string query = $"UPDATE JDT1 Set U_CorrectContraAcc = '{CorrectContraAccount}', U_CorrectContraShortName = '{CorrectContraShortName ?? ""}', U_ContraAccountLineId = '{ContraAccountLineId.ToString()}' WHERE TransId = {TransId} AND Line_ID = {LineId}";
            rec.DoQuery(query);
        }
    }
}
