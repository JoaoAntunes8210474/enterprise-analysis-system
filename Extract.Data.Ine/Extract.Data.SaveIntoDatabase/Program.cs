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

        private static async Task Main(string[] args)
        {
            LogInformation("Starting the program");

            if (!ValidateArguments(args, out int biggerYear, out int smallerYear))
            {
                throw new ArgumentException("Invalid arguments - Expected: <biggerYear> <smallerYear>, example: 2022 2017");
            }

            LogInformation("Creating the data tables");

            CreateDataTables();

            LogInformation("Looping through the JSON files to save the data into the DataTables");

            LoopThroughJsonFiles(biggerYear, smallerYear);

            await InsertDataIntoDatabaseAsync();
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
            _companyTable.Columns.Add("LegalFormCode", typeof(string));
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
            _businessTable.Columns.Add("ConvSignal", typeof(string));
            _businessTable.Columns.Add("ConvSignalDescription", typeof(string));

            _serviceTable = new DataTable();
            _serviceTable.Columns.Add("Year", typeof(int));
            _serviceTable.Columns.Add("NumberOfPeopleWorkingForCompanies", typeof(int));
            _serviceTable.Columns.Add("EconomicActivityCode", typeof(string));
            _serviceTable.Columns.Add("EconomicActivityDescription", typeof(string));
            _serviceTable.Columns.Add("GeographicAreaCode", typeof(string));
            _serviceTable.Columns.Add("GeographicAreaDescription", typeof(string));
            _serviceTable.Columns.Add("ConvSignal", typeof(string));
            _serviceTable.Columns.Add("ConvSignalDescription", typeof(string));

            _valueTable = new DataTable();
            _valueTable.Columns.Add("Year", typeof(int));
            _valueTable.Columns.Add("IncreasedValueForCompanies", typeof(int));
            _valueTable.Columns.Add("EconomicActivityCode", typeof(string));
            _valueTable.Columns.Add("EconomicActivityDescription", typeof(string));
            _valueTable.Columns.Add("GeographicAreaCode", typeof(string));
            _valueTable.Columns.Add("GeographicAreaDescription", typeof(string));
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
                    SaveDataIntoDataTables(jsonData, jsonFile, year);
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
            DirectoryInfo directory = new(Directory.GetCurrentDirectory());

            // Traverse up the directory tree until we find the solution directory
            while (directory != null && directory.GetFiles("*.sln").Length == 0)
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
        private static void SaveDataIntoDataTables(string jsonData, string jsonFilePath, int year)
        {
            // Deserialize the JSON data into a list of DataDto objects
            var dataList = JsonSerializer.Deserialize<List<DataDto>>(jsonData);

            ArgumentNullException.ThrowIfNull(dataList);

            string[] splitJsonFilePath = jsonFilePath.Split('\\');
            string jsonFileName = splitJsonFilePath[^1];

            // Loop through the data and add it to the appropriate data table
            foreach (var data in dataList)
            {
                if (data != null)
                {
                    DataRow row;
                    switch (jsonFileName)
                    {
                        case "CompanyData.json":
                            row = _companyTable.NewRow();
                            row["Year"] = year;
                            row["NumberOfCompanies"] = int.TryParse(data.NumberOfCompanies, out int numberOfCompanies) ? numberOfCompanies : 0;
                            row["EconomicActivityCode"] = data.EconomicActivityCode;
                            row["EconomicActivityDescription"] = data.EconomicActivityDescription;
                            row["GeographicAreaCode"] = data.GeographicAreaCode;
                            row["GeographicAreaDescription"] = data.GeographicAreaDescription;
                            row["LegalFormCode"] = data.LegalFormCode;
                            row["LegalFormDescription"] = data.LegalFormDescription;
                            row["ConvSignal"] = data.ConvSignal;
                            row["ConvSignalDescription"] = data.ConvSignalDescription;
                            _companyTable.Rows.Add(row);
                            break;

                        case "BusinessData.json":
                            row = _businessTable.NewRow();
                            row["Year"] = year;
                            row["NumberOfVolumeOfBusinessForCompanies"] = int.TryParse(data.NumberOfVolumeOfBusinessForCompanies, out int numberOfVolumeOfBusiness) ? numberOfVolumeOfBusiness : 0;
                            row["EconomicActivityCode"] = data.EconomicActivityCode;
                            row["EconomicActivityDescription"] = data.EconomicActivityDescription;
                            row["GeographicAreaCode"] = data.GeographicAreaCode;
                            row["GeographicAreaDescription"] = data.GeographicAreaDescription;
                            row["ConvSignal"] = data.ConvSignal;
                            row["ConvSignalDescription"] = data.ConvSignalDescription;
                            _businessTable.Rows.Add(row);
                            break;

                        case "ServiceData.json":
                            row = _serviceTable.NewRow();
                            row["Year"] = year;
                            row["NumberOfPeopleWorkingForCompanies"] = int.TryParse(data.NumberOfPeopleWorkingForCompanies, out int numberOfPeopleWorking) ? numberOfPeopleWorking : 0;
                            row["EconomicActivityCode"] = data.EconomicActivityCode;
                            row["EconomicActivityDescription"] = data.EconomicActivityDescription;
                            row["GeographicAreaCode"] = data.GeographicAreaCode;
                            row["GeographicAreaDescription"] = data.GeographicAreaDescription;
                            row["ConvSignal"] = data.ConvSignal;
                            row["ConvSignalDescription"] = data.ConvSignalDescription;
                            _serviceTable.Rows.Add(row);
                            break;

                        case "ValueData.json":
                            row = _valueTable.NewRow();
                            row["Year"] = year;
                            row["IncreasedValueForCompanies"] = int.TryParse(data.IncreasedValueForCompanies, out int increasedValue) ? increasedValue : 0;
                            row["EconomicActivityCode"] = data.EconomicActivityCode;
                            row["EconomicActivityDescription"] = data.EconomicActivityDescription;
                            row["GeographicAreaCode"] = data.GeographicAreaCode;
                            row["GeographicAreaDescription"] = data.GeographicAreaDescription;
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
        private static async Task InsertDataIntoDatabaseAsync()
        {
            LogInformation("Inserting data into the CompanyData");
            var companyTask = MergeDataTableIntoDatabaseAsync(_companyTable, "CompanyData", true);

            LogInformation("Inserting data into the BusinessData");
            var businessTask = MergeDataTableIntoDatabaseAsync(_businessTable, "BusinessData");

            LogInformation("Inserting data into the ServiceData");
            var serviceTask = MergeDataTableIntoDatabaseAsync(_serviceTable, "ServiceData");

            LogInformation("Inserting data into the ValueData");
            var valueTask = MergeDataTableIntoDatabaseAsync(_valueTable, "ValueData");

            await Task.WhenAll(companyTask, businessTask, serviceTask, valueTask);

            LogInformation("Finished inserting data into the database");
        }

        /// <summary>
        /// Function to execute a merge into statement in SQL for the data inside the created DataTables - CompanyTable, BusinessTable, ServiceTable, ValueTable
        /// </summary>
        /// <param name="connection">The connection to the database</param>
        /// <param name="transaction">The transaction to be used so that any errors can be rolled back</param>
        /// <param name="dataTable">The current DataTable that is being used to save the data into the database</param>
        /// <param name="tableName">The name of the table in the database where the data will be saved</param>
        private static async Task MergeDataTableIntoDatabaseAsync(DataTable dataTable, string tableName, bool hasLegalForm = false)
        {
            SqlConnectionStringBuilder connectionStringBuilder = new()
            {
                DataSource = "localhost",
                InitialCatalog = "eas",
                IntegratedSecurity = true,
                TrustServerCertificate = true
            };

            using SqlConnection connection = new(connectionStringBuilder.ConnectionString);
            await connection.OpenAsync();

            using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            try
            {
                const int batchSize = 500;
                int totalRows = dataTable.Rows.Count;
                int processedRows = 0;

                while (processedRows < totalRows)
                {
                    DataTable batchTable = dataTable.Clone();
                    for (int i = processedRows; i < processedRows + batchSize && i < totalRows; i++)
                    {
                        batchTable.ImportRow(dataTable.Rows[i]);
                    }

                    string tempTableName = $"#Temp{tableName}";
                    await CreateTemporaryTableAsync(connection, transaction, batchTable, tempTableName);

                    string mergeSql = tableName switch
                    {
                        "CompanyData" => $@"
                    MERGE INTO {tableName} AS target
                    USING {tempTableName} AS source
                    ON target.Year = source.Year
                    AND target.EconomicActivityCode = source.EconomicActivityCode
                    AND target.GeographicAreaCode = source.GeographicAreaCode
                    AND target.LegalFormCode = source.LegalFormCode
                    WHEN MATCHED THEN
                        UPDATE SET
                            NumberOfCompanies = source.NumberOfCompanies,
                            EconomicActivityDescription = source.EconomicActivityDescription,
                            GeographicAreaDescription = source.GeographicAreaDescription,
                            LegalFormDescription = source.LegalFormDescription,
                            ConvSignal = source.ConvSignal,
                            ConvSignalDescription = source.ConvSignalDescription
                    WHEN NOT MATCHED THEN
                        INSERT (Year, NumberOfCompanies, EconomicActivityCode, EconomicActivityDescription, GeographicAreaCode,
                                GeographicAreaDescription, LegalFormCode, LegalFormDescription, ConvSignal, ConvSignalDescription)
                        VALUES (source.Year, source.NumberOfCompanies, source.EconomicActivityCode, source.EconomicActivityDescription,
                                source.GeographicAreaCode, source.GeographicAreaDescription, source.LegalFormCode, source.LegalFormDescription,
                                source.ConvSignal, source.ConvSignalDescription);",

                        "BusinessData" => $@"
                    MERGE INTO {tableName} AS target
                    USING {tempTableName} AS source
                    ON target.Year = source.Year
                    AND target.EconomicActivityCode = source.EconomicActivityCode
                    AND target.GeographicAreaCode = source.GeographicAreaCode
                    WHEN MATCHED THEN
                        UPDATE SET
                            NumberOfVolumeOfBusinessForCompanies = source.NumberOfVolumeOfBusinessForCompanies,
                            EconomicActivityDescription = source.EconomicActivityDescription,
                            GeographicAreaDescription = source.GeographicAreaDescription,
                            ConvSignal = source.ConvSignal,
                            ConvSignalDescription = source.ConvSignalDescription
                    WHEN NOT MATCHED THEN
                        INSERT (Year, NumberOfVolumeOfBusinessForCompanies, EconomicActivityCode, EconomicActivityDescription,
                                GeographicAreaCode, GeographicAreaDescription, ConvSignal, ConvSignalDescription)
                        VALUES (source.Year, source.NumberOfVolumeOfBusinessForCompanies, source.EconomicActivityCode, source.EconomicActivityDescription,
                                source.GeographicAreaCode, source.GeographicAreaDescription, source.ConvSignal, source.ConvSignalDescription);",

                        "ServiceData" => $@"
                    MERGE INTO {tableName} AS target
                    USING {tempTableName} AS source
                    ON target.Year = source.Year
                    AND target.EconomicActivityCode = source.EconomicActivityCode
                    AND target.GeographicAreaCode = source.GeographicAreaCode
                    WHEN MATCHED THEN
                        UPDATE SET
                            NumberOfPeopleWorkingForCompanies = source.NumberOfPeopleWorkingForCompanies,
                            EconomicActivityDescription = source.EconomicActivityDescription,
                            GeographicAreaDescription = source.GeographicAreaDescription,
                            ConvSignal = source.ConvSignal,
                            ConvSignalDescription = source.ConvSignalDescription
                    WHEN NOT MATCHED THEN
                        INSERT (Year, NumberOfPeopleWorkingForCompanies, EconomicActivityCode, EconomicActivityDescription,
                                GeographicAreaCode, GeographicAreaDescription, ConvSignal, ConvSignalDescription)
                        VALUES (source.Year, source.NumberOfPeopleWorkingForCompanies, source.EconomicActivityCode, source.EconomicActivityDescription,
                                source.GeographicAreaCode, source.GeographicAreaDescription, source.ConvSignal, source.ConvSignalDescription);",

                        "ValueData" => $@"
                    MERGE INTO {tableName} AS target
                    USING {tempTableName} AS source
                    ON target.Year = source.Year
                    AND target.EconomicActivityCode = source.EconomicActivityCode
                    AND target.GeographicAreaCode = source.GeographicAreaCode
                    WHEN MATCHED THEN
                        UPDATE SET
                            IncreasedValueForCompanies = source.IncreasedValueForCompanies,
                            EconomicActivityDescription = source.EconomicActivityDescription,
                            GeographicAreaDescription = source.GeographicAreaDescription,
                            ConvSignal = source.ConvSignal,
                            ConvSignalDescription = source.ConvSignalDescription
                    WHEN NOT MATCHED THEN
                        INSERT (Year, IncreasedValueForCompanies, EconomicActivityCode, EconomicActivityDescription,
                                GeographicAreaCode, GeographicAreaDescription, ConvSignal, ConvSignalDescription)
                        VALUES (source.Year, source.IncreasedValueForCompanies, source.EconomicActivityCode, source.EconomicActivityDescription,
                                source.GeographicAreaCode, source.GeographicAreaDescription, source.ConvSignal, source.ConvSignalDescription);",

                        _ => throw new InvalidOperationException($"Unsupported table name: {tableName}")
                    };

                    using SqlCommand command = new(mergeSql, connection, transaction)
                    {
                        CommandTimeout = 300
                    };
                    await command.ExecuteNonQueryAsync();

                    await DropTemporaryTableAsync(connection, transaction, tempTableName);

                    processedRows += batchSize;
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error in {tableName}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Create a temporary table in the database and insert the data from the DataTable into the temporary table
        /// </summary>
        /// <param name="connection">The connection to the database</param>
        /// <param name="transaction">The current transaction that is being used</param>
        /// <param name="dataTable">The current DataTable that is being used to save the data before it is inserted into the database</param>
        /// <param name="tempTableName">The name of the temporary table that will be created</param>
        /// <returns>Nothing important - Only used to wait for the task to finish</returns>
        private static async Task CreateTemporaryTableAsync(SqlConnection connection, SqlTransaction transaction, DataTable dataTable, string tempTableName)
        {
            string createTableSql = $"CREATE TABLE {tempTableName} (";
            foreach (DataColumn column in dataTable.Columns)
            {
                createTableSql += $"[{column.ColumnName}] {GetSqlDataType(column.DataType)},";
            }
            createTableSql = createTableSql.TrimEnd(',') + ");";

            using SqlCommand createTableCommand = new(createTableSql, connection, transaction);
            await createTableCommand.ExecuteNonQueryAsync();

            using SqlBulkCopy bulkCopy = new(connection, SqlBulkCopyOptions.Default, transaction)
            {
                DestinationTableName = tempTableName
            };

            foreach (DataColumn column in dataTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }

            await bulkCopy.WriteToServerAsync(dataTable);
        }

        /// <summary>
        /// Drop the temporary table after the data has been inserted into the database
        /// </summary>
        /// <param name="connection">The connection to the database</param>
        /// <param name="transaction">The current transaction that is being used</param>
        /// <param name="tempTableName">The name of the temporary table that needs to be dropped</param>
        /// <returns>No return value - Only used to wait for the task to finish</returns>
        private static async Task DropTemporaryTableAsync(SqlConnection connection, SqlTransaction transaction, string tempTableName)
        {
            string dropTableSql = $"DROP TABLE {tempTableName};";
            using SqlCommand dropTableCommand = new(dropTableSql, connection, transaction);
            await dropTableCommand.ExecuteNonQueryAsync();
        }

        private static string GetSqlDataType(Type type)
        {
            return type switch
            {
                _ when type == typeof(int) => "INT",
                _ when type == typeof(decimal) => "DECIMAL(18, 2)",
                _ when type == typeof(string) => "NVARCHAR(MAX)",
                _ => throw new NotSupportedException($"Type {type} is not supported")
            };
        }

        /// <summary>
        /// Log information message
        /// </summary>
        /// <param name="message">The message to be logged into the console when this method is called</param>
        private static void LogInformation(string message)
        {
            var logMessage = $"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] {message}";
            Console.WriteLine(logMessage);
        }
    }
}