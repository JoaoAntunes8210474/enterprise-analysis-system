using Extract.Data.SaveJson.dtos;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace Extract.Data.SaveIntoDatabase
{
    public class Program
    {
        private static DataTable _companyTable;
        private static DataTable _businessTable;
        private static DataTable _serviceTable;
        private static DataTable _valueTable;

        private static void Main(string[] args)
        {
            if (!ValidateArguments(args, out int biggerYear, out int smallerYear))
            {
                throw new ArgumentException("Invalid arguments - Expected: <biggerYear> <smallerYear>, example: 2022 2017");
            }

            CreateDataTables();

            // For all the years in between the two years
            // check for all the json files with data
            LoopThroughJsonFiles(biggerYear, smallerYear);

            InsertDataIntoDatabase();
        }

        /// <summary>
        /// Create the data tables that will be used to save the data
        /// into the database
        /// </summary>
        private static void CreateDataTables()
        {
            _companyTable = new DataTable();
            _companyTable.Columns.Add("Year", typeof(int));
            _companyTable.Columns.Add("NumberOfCompanies", typeof(int));
            _companyTable.Columns.Add("EconomicActivityCode", typeof(string));
            _companyTable.Columns.Add("EconomicActivityDescription", typeof(string));
            _companyTable.Columns.Add("GeographicAreaCode", typeof(string));
            _companyTable.Columns.Add("GeographicAreaDescription", typeof(string));
            _companyTable.Columns.Add("LegalFormCode", typeof(int));
            _companyTable.Columns.Add("LegalFormDescription", typeof(string));
            _companyTable.Columns.Add("ConvSignal", typeof(string));
            _companyTable.Columns.Add("ConvSignalDescription", typeof(string));

            _businessTable = new DataTable();
            _businessTable.Columns.Add("Year", typeof(int));
            _businessTable.Columns.Add("NumberOfVolumeOfBusinessForCompanies", typeof(int));
            _businessTable.Columns.Add("EconomicActivityCode", typeof(string));
            _businessTable.Columns.Add("EconomicActivityDescription", typeof(string));
            _businessTable.Columns.Add("GeographicAreaCode", typeof(string));
            _businessTable.Columns.Add("GeographicAreaDescription", typeof(string));
            _businessTable.Columns.Add("LegalFormCode", typeof(int));
            _businessTable.Columns.Add("LegalFormDescription", typeof(string));
            _businessTable.Columns.Add("ConvSignal", typeof(string));
            _businessTable.Columns.Add("ConvSignalDescription", typeof(string));

            _serviceTable = new DataTable();
            _serviceTable.Columns.Add("Year", typeof(int));
            _serviceTable.Columns.Add("NumberOfPeopleWorkingForCompanies", typeof(int));
            _serviceTable.Columns.Add("EconomicActivityCode", typeof(string));
            _serviceTable.Columns.Add("EconomicActivityDescription", typeof(string));
            _serviceTable.Columns.Add("GeographicAreaCode", typeof(string));
            _serviceTable.Columns.Add("GeographicAreaDescription", typeof(string));
            _serviceTable.Columns.Add("LegalFormCode", typeof(int));
            _serviceTable.Columns.Add("LegalFormDescription", typeof(string));
            _serviceTable.Columns.Add("ConvSignal", typeof(string));
            _serviceTable.Columns.Add("ConvSignalDescription", typeof(string));

            _valueTable = new DataTable();
            _valueTable.Columns.Add("Year", typeof(int));
            _valueTable.Columns.Add("IncreasedValueForCompanies", typeof(decimal));
            _valueTable.Columns.Add("EconomicActivityCode", typeof(string));
            _valueTable.Columns.Add("EconomicActivityDescription", typeof(string));
            _valueTable.Columns.Add("GeographicAreaCode", typeof(string));
            _valueTable.Columns.Add("GeographicAreaDescription", typeof(string));
            _valueTable.Columns.Add("LegalFormCode", typeof(int));
            _valueTable.Columns.Add("LegalFormDescription", typeof(string));
            _valueTable.Columns.Add("ConvSignal", typeof(string));
            _valueTable.Columns.Add("ConvSignalDescription", typeof(string));
        }

        /// <summary>
        /// Validate the arguments passed to the program
        /// </summary>
        /// <param name="args">These are the arguments passed to the program</param>
        /// <param name="biggerYear">This is the bigger year of the two years being compared</param>
        /// <param name="smallerYear">This is the smaller year of the two years being compared</param>
        /// <returns>
        ///     If the arguments are valid, it returns true and the biggerYear and smallerYear
        ///     are set to the values passed in the arguments
        ///     Otherwise, it returns false, afterwards
        ///     the program should throw an exception
        /// </returns>
        private static bool ValidateArguments(string[] args, out int biggerYear, out int smallerYear)
        {
            // Check if the arguments are all being passed
            if (args.Length != 2)
            {
                biggerYear = 0;
                smallerYear = 0;
                return false;
            }

            // Check if the arguments can be parsed into integers
            bool canParseBiggerYear = int.TryParse(args[0], out biggerYear);
            bool canParseSmallerYear = int.TryParse(args[1], out smallerYear);

            if (canParseBiggerYear && canParseSmallerYear)
            {
                // If they can be parsed, check if the bigger year is greater than the smaller year
                if (biggerYear < smallerYear)
                {
                    biggerYear = 0;
                    smallerYear = 0;
                    return false;
                }
            }
            else
            {
                biggerYear = 0;
                smallerYear = 0;
                return false;
            }

            // If all the checks pass, return true because
            // the values were set when trying to parse the integers
            return true;
        }

        /// <summary>
        /// Loop through the json files for the years between the two years passed as parameters
        /// </summary>
        /// <param name="biggerYear">This is the bigger year of the two years</param>
        /// <param name="smallerYear">This is the smaller year of the two years</param>
        private static void LoopThroughJsonFiles(int biggerYear, int smallerYear)
        {
            for (int year = smallerYear; year <= biggerYear; year++)
            {
                // Get the json files for the year
                string[] jsonFiles = GetJsonFilesForYear(year);

                // Loop through the json files
                foreach (string jsonFile in jsonFiles)
                {
                    // Get the data from the json file
                    string jsonData = GetDataFromJsonFile(jsonFile);
                    // Save the data into the database
                    SaveDataIntoDataTables(jsonData);
                }
            }
        }

        /// <summary>
        /// Get the JSON files for the specified year
        /// </summary>
        /// <param name="year">The year for which to get the JSON files</param>
        /// <returns>An array of file paths to the JSON files</returns>
        private static string[] GetJsonFilesForYear(int year)
        {
            // Get the current directory
            DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            // Traverse up the directory tree until we find the solution directory
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent!;
            }

            // If we didn't find the solution directory, use the current directory
            if (directory == null || directory.GetFiles("*.sln").Length == 0)
            {
                directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            }

            // Combine the data directory path with the year
            string dataDirectoryPath = Path.Combine(directory.FullName, "Data", year.ToString());

            // Get all JSON files in the data directory
            return Directory.GetFiles(dataDirectoryPath, "*.json");
        }

        /// <summary>
        /// Get the data from the specified JSON file
        /// </summary>
        /// <param name="jsonFile">The path to the JSON file</param>
        /// <returns>The data from the JSON file as a string</returns>
        private static string GetDataFromJsonFile(string jsonFile)
        {
            return File.ReadAllText(jsonFile);
        }

        /// <summary>
        /// Save the data into the database
        /// </summary>
        /// <param name="jsonData">The data to save into the database</param>
        private static void SaveDataIntoDataTables(string jsonData)
        {
            // Deserialize the JSON data into a list of DataDto objects
            var dataList = JsonSerializer.Deserialize<List<DataDto>>(jsonData);

            ArgumentNullException.ThrowIfNull(dataList);

            // Loop through the data and add it to the appropriate data table
            foreach (var data in dataList)
            {
                if (data != null)
                {
                    DataRow row;
                    switch (data.FileName)
                    {
                        case "CompanyData.json":
                            row = _companyTable.NewRow();
                            row["NumberOfCompanies"] = int.Parse(data.NumberOfCompanies);
                            row["EconomicActivityCode"] = data.EconomicActivityCode;
                            row["EconomicActivityDescription"] = data.EconomicActivityDescription;
                            row["GeographicAreaCode"] = data.GeographicAreaCode;
                            row["GeographicAreaDescription"] = data.GeographicAreaDescription;
                            row["LegalFormCode"] = int.Parse(data.LegalFormCode);
                            row["LegalFormDescription"] = data.LegalFormDescription;
                            row["ConvSignal"] = data.ConvSignal;
                            row["ConvSignalDescription"] = data.ConvSignalDescription;
                            _companyTable.Rows.Add(row);
                            break;

                        case "BusinessData.json":
                            row = _businessTable.NewRow();
                            row["NumberOfVolumeOfBusinessForCompanies"] = int.Parse(data.NumberOfVolumeOfBusinessForCompanies);
                            row["EconomicActivityCode"] = data.EconomicActivityCode;
                            row["EconomicActivityDescription"] = data.EconomicActivityDescription;
                            row["GeographicAreaCode"] = data.GeographicAreaCode;
                            row["GeographicAreaDescription"] = data.GeographicAreaDescription;
                            row["LegalFormCode"] = int.Parse(data.LegalFormCode);
                            row["LegalFormDescription"] = data.LegalFormDescription;
                            row["ConvSignal"] = data.ConvSignal;
                            row["ConvSignalDescription"] = data.ConvSignalDescription;
                            _businessTable.Rows.Add(row);
                            break;

                        case "ServiceData.json":
                            row = _serviceTable.NewRow();
                            row["NumberOfPeopleWorkingForCompanies"] = int.Parse(data.NumberOfPeopleWorkingForCompanies);
                            row["EconomicActivityCode"] = data.EconomicActivityCode;
                            row["EconomicActivityDescription"] = data.EconomicActivityDescription;
                            row["GeographicAreaCode"] = data.GeographicAreaCode;
                            row["GeographicAreaDescription"] = data.GeographicAreaDescription;
                            row["LegalFormCode"] = int.Parse(data.LegalFormCode);
                            row["LegalFormDescription"] = data.LegalFormDescription;
                            row["ConvSignal"] = data.ConvSignal;
                            row["ConvSignalDescription"] = data.ConvSignalDescription;
                            _serviceTable.Rows.Add(row);
                            break;

                        case "ValueData.json":
                            row = _valueTable.NewRow();
                            row["IncreasedValueForCompanies"] = decimal.Parse(data.IncreasedValueForCompanies);
                            row["EconomicActivityCode"] = data.EconomicActivityCode;
                            row["EconomicActivityDescription"] = data.EconomicActivityDescription;
                            row["GeographicAreaCode"] = data.GeographicAreaCode;
                            row["GeographicAreaDescription"] = data.GeographicAreaDescription;
                            row["LegalFormCode"] = int.Parse(data.LegalFormCode);
                            row["LegalFormDescription"] = data.LegalFormDescription;
                            row["ConvSignal"] = data.ConvSignal;
                            row["ConvSignalDescription"] = data.ConvSignalDescription;
                            _valueTable.Rows.Add(row);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Function to insert the data from the DataTables into the database
        /// </summary>
        private static void InsertDataIntoDatabase()
        {
            string connectionString = "YourConnectionStringHere";

            using SqlConnection connection = new(connectionString);
            connection.Open();

            using SqlTransaction transaction = connection.BeginTransaction();
            try
            {
                MergeDataTableIntoDatabase(connection, transaction, _companyTable, "CompanyTable");
                MergeDataTableIntoDatabase(connection, transaction, _businessTable, "BusinessTable");
                MergeDataTableIntoDatabase(connection, transaction, _serviceTable, "ServiceTable");
                MergeDataTableIntoDatabase(connection, transaction, _valueTable, "ValueTable");

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Function to execute a merge into statement in SQL for the data inside the created DataTables - CompanyTable, BusinessTable, ServiceTable, ValueTable
        /// </summary>
        /// <param name="connection">The connection to the database</param>
        /// <param name="transaction">The transaction to be used so that any errors can be rolled back</param>
        /// <param name="dataTable">The current DataTable that is being used to save the data into the database</param>
        /// <param name="tableName">The name of the table in the database where the data will be saved</param>
        private static void MergeDataTableIntoDatabase(SqlConnection connection, SqlTransaction transaction, DataTable dataTable, string tableName)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                string mergeSql = $@"
                    MERGE INTO {tableName} AS target
                    USING (VALUES (@Year, @NumberOfCompanies, @EconomicActivityCode, @EconomicActivityDescription, @GeographicAreaCode, @GeographicAreaDescription, @LegalFormCode, @LegalFormDescription, @ConvSignal, @ConvSignalDescription)) AS source
                    (Year, NumberOfCompanies, EconomicActivityCode, EconomicActivityDescription, GeographicAreaCode, GeographicAreaDescription, LegalFormCode, LegalFormDescription, ConvSignal, ConvSignalDescription)
                    ON target.Year = source.Year AND target.EconomicActivityCode = source.EconomicActivityCode AND target.GeographicAreaCode = source.GeographicAreaCode AND target.LegalFormCode = source.LegalFormCode
                    WHEN MATCHED THEN
                        UPDATE SET
                            NumberOfCompanies = source.NumberOfCompanies,
                            EconomicActivityDescription = source.EconomicActivityDescription,
                            GeographicAreaDescription = source.GeographicAreaDescription,
                            LegalFormCode = source.LegalFormCode,
                            LegalFormDescription = source.LegalFormDescription,
                            ConvSignal = source.ConvSignal,
                            ConvSignalDescription = source.ConvSignalDescription
                    WHEN NOT MATCHED THEN
                        INSERT (Year, NumberOfCompanies, EconomicActivityCode, EconomicActivityDescription, GeographicAreaCode, GeographicAreaDescription, LegalFormCode, LegalFormDescription, ConvSignal, ConvSignalDescription)
                        VALUES (source.Year, source.NumberOfCompanies, source.EconomicActivityCode, source.EconomicActivityDescription, source.GeographicAreaCode, source.GeographicAreaDescription, source.LegalFormCode, source.LegalFormDescription, source.ConvSignal, source.ConvSignalDescription);";

                using SqlCommand command = new(mergeSql, connection, transaction);

                command.Parameters.AddWithValue("@Year", row["Year"]);
                command.Parameters.AddWithValue("@NumberOfCompanies", row["NumberOfCompanies"]);
                command.Parameters.AddWithValue("@EconomicActivityCode", row["EconomicActivityCode"]);
                command.Parameters.AddWithValue("@EconomicActivityDescription", row["EconomicActivityDescription"]);
                command.Parameters.AddWithValue("@GeographicAreaCode", row["GeographicAreaCode"]);
                command.Parameters.AddWithValue("@GeographicAreaDescription", row["GeographicAreaDescription"]);
                command.Parameters.AddWithValue("@LegalFormCode", row["LegalFormCode"]);
                command.Parameters.AddWithValue("@LegalFormDescription", row["LegalFormDescription"]);
                command.Parameters.AddWithValue("@ConvSignal", row["ConvSignal"]);
                command.Parameters.AddWithValue("@ConvSignalDescription", row["ConvSignalDescription"]);

                command.ExecuteNonQuery();
            }
        }
    }
}