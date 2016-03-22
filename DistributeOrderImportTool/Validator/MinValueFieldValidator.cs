using DistributeOrderImportTool.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributeOrderImportTool.Validator
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class MinValueFieldAttribute : Attribute, IDataValidate
    {
        public object MinValue { get; private set; }
        public bool Inclusive { get; private set; }
        public MinValueFieldAttribute(int minValue, bool inclusive = false)
        {
            this.MinValue = minValue;
            this.Inclusive = inclusive;
        }
        public MinValueFieldAttribute(decimal minValue, bool inclusive = false)
        {
            this.MinValue = minValue;
            this.Inclusive = inclusive;
        }

        public bool Validate(OrderImportRowData rowData, OrderImportCellData cellData, out string errorMsg)
        {
            decimal value;
            decimal.TryParse(cellData.Value, out value);

            decimal minValue = Convert.ToDecimal(this.MinValue);
            bool isInValid = !this.Inclusive ? value < minValue : value <= minValue;
            if (isInValid)
            {
                errorMsg = string.Format("{0}的值必须大于{1}{2}！", cellData.Comment, (this.Inclusive ? "等于" : ""), this.MinValue);
                return false;
            }

            errorMsg = null;
            return true;
        }
    }
}
