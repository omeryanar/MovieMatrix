using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using ExcelDataReader;

namespace MovieMatrix.Helper
{
    public static class ExcelHelper
    {
        public static async Task<DataTable> CreateDataTableFromExcel(string filePath)
        {
            DataTable dataTable = null;

            await Task.Run(() =>
            {
                using (Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    bool isCSV = filePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
                    using (IExcelDataReader reader = isCSV ? ExcelReaderFactory.CreateCsvReader(stream) : ExcelReaderFactory.CreateReader(stream))
                    {
                        ExcelDataSetConfiguration configuration = new ExcelDataSetConfiguration
                        {
                            ConfigureDataTable = x => new ExcelDataTableConfiguration
                            {
                                UseHeaderRow = true
                            }
                        };

                        DataSet dataSet = reader.AsDataSet(configuration);
                        if (dataSet.Tables.Count > 0)
                            dataTable = dataSet.Tables[0];
                    }
                }
            });
            
            return dataTable;
        }

        public static T GetDataCellValue<T>(this DataRow row, string columnName)
        {
            try
            {
                object value = row[columnName];

                if (typeof(T) == typeof(bool))
                {
                    if (String.Compare(value.ToString(), Properties.Resources.Yes, true) == 0)
                        value = true;
                    else if (String.Compare(value.ToString(), Properties.Resources.No, true) == 0)
                        value = false;
                }
                else
                    value = Convert.ChangeType(value, typeof(T));

                return (T)value;
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
