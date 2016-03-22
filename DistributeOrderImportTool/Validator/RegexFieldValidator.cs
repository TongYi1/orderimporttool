using DistributeOrderImportTool.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DistributeOrderImportTool.Validator
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RegexFieldAttribute : Attribute, IDataValidate
    {
        public string RegexPattern { get; private set; }
        private Regex RegexExpression { get;  set; }
        public RegexFieldAttribute(string pattern)
        {
            this.RegexPattern = pattern ?? string.Empty;
            this.RegexExpression = new Regex(pattern);
        }

        public bool Validate(OrderImportRowData rowData, OrderImportCellData cellData, out string errorMsg)
        {
            if (!RegexExpression.IsMatch(cellData.Value))
            {
                errorMsg = string.Format("{0}的格式不正确！", cellData.Comment);
                return false;
            }
            errorMsg = null;
            return true;
        }
    }
}
