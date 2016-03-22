using DistributeOrderImportTool.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DistributeOrderImportTool.Validator
{
    public class ValidationManager
    {
        public static bool Validate(OrderImportRowData rowData, out string errorMsg)
        {
            //-- 基础数据验证
            Type type = rowData.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                if (property.PropertyType.IsAssignableFrom(typeof(OrderImportCellData)))
                {
                    OrderImportCellData cellData = (OrderImportCellData)property.GetValue(rowData, null);
                    ParseValidationAttribute(property, cellData);
                    if (cellData.Validators != null && cellData.Validators.Count > 0)
                    {
                        foreach (var validator in cellData.Validators)
                        {
                            if (!validator.Validate(rowData, cellData, out errorMsg))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            errorMsg = null;
            return true;
        }

        private static void ParseValidationAttribute(PropertyInfo property, OrderImportCellData cellData)
        {
            object[] customerAttributes = property.GetCustomAttributes(false);
            if (customerAttributes != null && customerAttributes.Length > 0)
            {
                for (int i = 0; i < customerAttributes.Length; i++)
                {
                    if (typeof(IDataValidate).IsAssignableFrom(customerAttributes[i].GetType()))
                    {
                        cellData.Validators.Add((IDataValidate)customerAttributes[i]);
                    }
                }
            }
        }
    }
}
