using DistributeOrderImportTool.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributeOrderImportTool.Validator
{
    public interface IDataValidate
    {
        bool Validate(OrderImportRowData rowData, OrderImportCellData cellData, out string errorMsg);
    }

}
