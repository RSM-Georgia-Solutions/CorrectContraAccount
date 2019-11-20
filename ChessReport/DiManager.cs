using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;

namespace ChessReport
{
    public static class DiManager
    {
        public static Company Company => XCompany.Value;

        private static readonly Lazy<Company> XCompany =
            new Lazy<Company>(() => (Company)SAPbouiCOM.Framework
                .Application
                .SBO_Application
                .Company.GetDICompany());
        public static Recordset Recordset => recSet.Value;
        private static readonly Lazy<Recordset> recSet =
           new Lazy<SAPbobsCOM.Recordset>(() => (Recordset)
               Company
                   .GetBusinessObject(BoObjectTypes.BoRecordset));

        public static string CreateTable(string tableName, string tableDescription, BoUTBTableType tableType)
        {
            try
            {
                UserTablesMD oUTables = (UserTablesMD)Company.GetBusinessObject(BoObjectTypes.oUserTables);

                if (oUTables.GetByKey(tableName) == false)
                {
                    oUTables.TableName = tableName;
                    oUTables.TableDescription = tableDescription;
                    oUTables.TableType = tableType;
                    int ret = oUTables.Add();

                    return ret == 0 ? "" : Company.GetLastErrorDescription();
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(oUTables);
                return string.Empty;

            }
            catch (Exception e)
            {
                return $"exeption : {e.Message} sap error : {Company.GetLastErrorDescription()}";
            }
            finally
            {
                GC.Collect();
            }

        }


        public static void AddFindForm(IUserObjectsMD oUserObjectMD, string name, string description, bool isEditable)
        {
            oUserObjectMD.FindColumns.Add();
            oUserObjectMD.FindColumns.ColumnAlias = "U_" + name;
            oUserObjectMD.FindColumns.ColumnDescription = description;

            oUserObjectMD.FormColumns.Add();
            oUserObjectMD.FormColumns.FormColumnAlias = "U_" + name;
            oUserObjectMD.FormColumns.FormColumnDescription = description;
            oUserObjectMD.FormColumns.Editable = isEditable ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
        }

        public static string CreateUdo(string udoCode, string udoDescription, string headerTable, string chidlTable, int position, int fatherMenuId)
        {
            SAPbobsCOM.UserObjectsMD udo = (SAPbobsCOM.UserObjectsMD)DiManager.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD);
            bool updateFlag = udo.GetByKey(udoCode);

            udo.Code = udoCode;
            udo.Name = udoDescription;
            udo.ObjectType = BoUDOObjType.boud_Document;
            udo.TableName = headerTable;
            udo.ChildTables.TableName = chidlTable;
            udo.ChildTables.Add();


            udo.CanCancel = BoYesNoEnum.tNO;
            udo.CanClose = BoYesNoEnum.tYES;
            udo.CanDelete = BoYesNoEnum.tNO;
            udo.CanFind = BoYesNoEnum.tYES;
            udo.MenuCaption = udoDescription;
            udo.CanCreateDefaultForm = BoYesNoEnum.tYES;
            udo.EnableEnhancedForm = BoYesNoEnum.tYES;
            udo.MenuItem = BoYesNoEnum.tYES;
            udo.Position = position;
            udo.FatherMenuID = fatherMenuId;
            udo.MenuUID = udoCode;

            udo.FormColumns.FormColumnAlias = "DocEntry";
            udo.FormColumns.FormColumnDescription = "DocEntry";
            udo.FormColumns.Add();




            return updateFlag ? udo.Update() != 0 ? Company.GetLastErrorDescription() : string.Empty : udo.Add() != 0 ? Company.GetLastErrorDescription() : string.Empty;
        }

        public static string CreateField(string tablename, string fieldname, string description, BoFieldTypes type, int size, bool isMandatory, bool isSapTable = false, string likedToTAble = "", string defaultValue = "", BoFldSubTypes subType = BoFldSubTypes.st_None)
        {
            // Get a new Recordset object
            Recordset oRecordSet = (Recordset)Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            string sqlQuery = $"SELECT T0.TableID, T0.FieldID FROM CUFD T0 WHERE T0.TableID = '{tablename}' AND T0.AliasID = '{fieldname}'";
            oRecordSet.DoQuery(sqlQuery);
            var updateFlag = oRecordSet.RecordCount == 1;
            var fieldId = int.Parse(oRecordSet.Fields.Item("FieldID").Value.ToString());
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecordSet);

            UserFieldsMD oUfield = (UserFieldsMD)Company.GetBusinessObject(BoObjectTypes.oUserFields);
            if (updateFlag)
            {
                oUfield.GetByKey(tablename, fieldId);
            }
            try
            {
                oUfield.TableName = tablename;
                oUfield.Name = fieldname;
                oUfield.Description = description;
                oUfield.Type = type;
                oUfield.Mandatory = isMandatory ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                oUfield.DefaultValue = defaultValue;

                if (type == BoFieldTypes.db_Float)
                {
                    oUfield.SubType = subType;
                }

                if (type == BoFieldTypes.db_Alpha || type == BoFieldTypes.db_Numeric)
                {
                    oUfield.EditSize = size;
                }

                oUfield.LinkedTable = likedToTAble;
                int ret = updateFlag ? oUfield.Update() : oUfield.Add();
                if (ret == 0)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUfield);
                    return string.Empty;
                }
                else
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUfield);
                    return Company.GetLastErrorDescription();
                }
            }
            catch (Exception e)
            {
                return $"exeption : {e.Message}, Sap Error {Company.GetLastErrorDescription()}";
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oUfield);
            }

        }

    }
}
