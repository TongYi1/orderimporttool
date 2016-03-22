using DistributeOrderImportTool.Entity;
using DistributeOrderImportTool.Validator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributeOrderImportTool.Validator
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DecimalFieldAttribute : Attribute, IDataValidate
    {
        public bool Validate(OrderImportRowData rowData, OrderImportCellData cellData, out string errorMsg)
        {
            decimal value;
            if (!decimal.TryParse(cellData.Value, out value))
            {
                errorMsg = string.Format("{0}的值必须是整数或小数！", cellData.Comment);
                return false;
            }
            errorMsg = null;
            return true;
        }
    }
}
