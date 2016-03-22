using DistributeOrderImportTool.Validator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DistributeOrderImportTool.Entity
{
    public class OrderImportCellData
    {
        private static Regex ExtractValueRegex = new Regex("\\[(?<Value>.+)\\]");

        public string Comment { get; set; }

        private string m_value;
        public string Value
        {
            get
            {
                return SafeTrim(m_value);
            }
            set
            {
                m_value = value;
            }
        }
        public string ExtractValue
        {

            get
            {
                if (ExtractValueRegex.IsMatch(Value))
                {
                    return SafeTrim(ExtractValueRegex.Match(Value).Groups["Value"].Value);
                }
                return string.Empty;
            }
        }
        public List<IDataValidate> Validators { get; private set; }

        public OrderImportCellData(string value)
            : this(null, value)
        {
        }

        public OrderImportCellData(string comment, string value)
            : this(comment, value, null)
        {
        }

        public OrderImportCellData(string comment, string value, List<IDataValidate> validators)
        {
            this.Comment = comment;
            this.Value = value;
            this.Validators = validators;
            if (Validators == null)
            {
                Validators = new List<IDataValidate>();
            }
        }

        public override string ToString()
        {
            return Value;
        }

        private static string SafeTrim(string inputString)
        {
            if (!string.IsNullOrWhiteSpace(inputString))
            {
                return inputString.Trim();
            }
            return inputString;
        }
    }
}
