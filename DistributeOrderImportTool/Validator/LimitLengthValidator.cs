using DistributeOrderImportTool.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributeOrderImportTool.Validator
{


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class LimitLengthAttribute : Attribute, IDataValidate
    {
        public int MaxLength { get; private set; }
        public LimitLengthAttribute(int maxLength)
        {
            this.MaxLength = maxLength;
        }

        public bool Validate(OrderImportRowData rowData, OrderImportCellData cellData, out string errorMsg)
        {
            if (!string.IsNullOrWhiteSpace(cellData.Value) && cellData.Value.Length > MaxLength)
            {
                errorMsg = string.Format("{0}的值超过最大长度限制{1}！", cellData.Comment, MaxLength);
                return false;
            }
            errorMsg = null;
            return true;
        }
    }
}

