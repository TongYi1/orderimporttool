using DistributeOrderImportTool.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributeOrderImportTool.Validator
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RequireFieldAttribute : Attribute, IDataValidate
    {
        public bool Validate(OrderImportRowData rowData, OrderImportCellData cellData, out string errorMsg)
        {
            if (string.IsNullOrWhiteSpace(cellData.Value))
            {
                errorMsg = string.Format("{0}的值不能为空！", cellData.Comment);
                return false;
            }
            errorMsg = null;
            return true;
        }
    }
}
