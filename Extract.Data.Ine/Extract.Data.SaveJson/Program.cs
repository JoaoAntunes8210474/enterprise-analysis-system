﻿using static System.Net.WebRequestMethods;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using Extract.Data.SaveJson.dtos;
using File = System.IO.File;
using Extract.Data.SaveJson.models;
using System.Text.Json.Serialization;

namespace Extract.Data.SaveJson
{
    public static class Program
    {
        private const string DataPath = "\\Data\\";

        private const string CompanyUrl = "https://www.ine.pt/ine/json_indicador/pindica.jsp?op=2&varcd=0008511&Dim1=S7A2022&Dim2=";

        private const string ServiceUrl = "https://www.ine.pt/ine/json_indicador/pindica.jsp?op=2&varcd=0008512&Dim1=S7A2022&Dim2=";

        private const string BusinessUrl = "https://www.ine.pt/ine/json_indicador/pindica.jsp?op=2&varcd=0008513&Dim1=S7A2022&Dim2=";

        private const string ValueUrl = "https://www.ine.pt/ine/json_indicador/pindica.jsp?op=2&varcd=0008514&Dim1=S7A2022&Dim2=";

        private const string _Dim3 = "01,02,03,05,06,07,08,09,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,35,36,37,38,39,41,42,43,45,46,47,49,50,51,52,53,55,56,58,59,60,61,62,63,68,69,70,71,72,73,74,75,77,78,79,80,81,82,85,86,87,88,90,91,92,93,94,95,96";

        private static readonly Dictionary<string, string> _Dim2Mapping = new()
        {
            { "AltoMinho", "1111601,1111602,1111603,1111604,1111605,1111606,1111607,1111608,1111609,1111610" },
            { "Cavado", "1120301,1120302,1120303,1120306,1120310,1120313" },
            { "Ave", "1190304,1190307,1190308,1191705,1190309,1190311,1190312,1190314" },
            { "AreaMetropolitanaPorto", "11A0104,11A0107,11A1304,11A1306,11A1308,11A0113,11A1310,11A1312,11A1313,11A0109,11A1314,11A0116,11A1318,11A0119,11A1315,11A1316,11A1317" },
            { "AltoTamega", "11B1702,11B1703,11B1706,11B1709,11B1712,11B1713" },
            { "TamegaSousa", "11C1301,11C1302,11C0106,11C0305,11C1804,11C1303,11C1305,11C1307,11C1309,11C1311,11C1813" },
            { "Douro", "11D1701,11D1801,11D0403,11D0404,11D1805,11D1704,11D1807,11D1707,11D1812,11D1708,11D1710,11D1711,11D1815,11D1818,11D1819,11D1820,11D0409,11D0914,11D1714" },
            { "TerrasTransMonstes", "11E0401,11E0402,11E0405,11E0406,11E0407,11E0408,11E0410,11E0411,11E0412" },
            { "Oeste", "16B1001,16B1101,16B1102,16B1005,16B1104,16B1006,16B1108,16B1011,16B1012,16B1014,16B1112,16B1113" },
            { "Aveiro", "16D0101,16D0102,16D0103,16D0105,16D0108,16D0110,16D0112,16D0114,16D0115,16D0117,16D0118" },
            { "Coimbra", "16E0601,16E0602,16E0603,16E0604,16E0605,16E0606,16E0607,16E0111,16E0608,16E0609,16E0610,16E1808,16E0611,16E0612,16E0613,16E0614,16E0615,16E0616,16E0617" },
            { "Leiria", "16F1002,16F1003,16F1004,16F1007,16F1008,16F1009,16F1010,16F1013,16F1015,16F1016" },
            { "Viseu", "16G0901,16G1802,16G1803,16G1806,16G1809,16G1810,16G1811,16G1814,16G1816,16G1817,16G1821,16G1822,16G1823,16G1824" },
            { "BeiraBaixa", "16H0502,16H0505,16H0506,16H0507,16H0508,16H0511" },
            { "MedioTejo", "16I1401,16I1402,16I1408,16I1410,16I1411,16I1413,16I1421,16I1417,16I0509,16I1418,16I1419,16I0510,16I1420" },
            { "SerraEstrela", "16J0902,16J0501,16J0903,16J0503,16J0904,16J0905,16J0504,16J0906,16J0907,16J0908,16J0909,16J0910,16J0911,16J0912,16J0913" },
            { "AreaMetropolitanaLisboa", "1701502,1701503,1701115,1701504,1701105,1701106,1701107,1701109,1701506,1701507,1701116,1701110,1701508,1701510,1701511,1701512,1701111,1701114" },
            { "AlentejoLitoral", "1811501,1811505,1810211,1811509,1811513" },
            { "BaixoAlentejo", "1840201,1840202,1840203,1840204,1840205,1840206,1840207,1840208,1840209,1840210,1840212,1840213,1840214" },
            { "LeziriaTejo", "1851403,1851404,1851103,1851405,1851406,1851407,1851409,1851412,1851414,1851415,1851416" },
            { "AltoAlentejo", "1861201,1861202,1861203,1861204,1861205,1861206,1861207,1861208,1861209,1861210,1861211,1861212,1861213,1861214,1861215" },
            { "AlentejoCentral", "1870701,1870702,1870703,1870704,1870705,1870706,1870707,1870708,1870709,1870710,1870711,1870712,1870713,1870714" },
            { "Algarve", "1500801,1500802,1500803,1500804,1500805,1500806,1500807,1500808,1500809,1500810,1500811,1500812,1500813,1500814,1500815,1500816" },
            { "Acores", "2004301,2004501,2004901,2004701,2004201,2004801,2004601,2004602,2004202,2004203,2004204,2004205,2004401,2004802,2004603,2004502,2004302,2004101,2004206" },
            { "Madeira", "3003101,3003102,3003103,3003104,3003105,3003106,3003201,3003107,3003108,3003109,3003110" }
        };

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static async Task Main(string[] args)
        {
            if (!ValidateArgument(args, out int year))
            {
                throw new ArgumentException("Invalid argument - Expected: <year>, example: 2022");
            }

            await RegisterDataIntoJsonFile(year);
        }

        /// <summary>
        /// Validate the argument passed to the program
        /// </summary>
        /// <param name="args">The argument passed to the program</param>
        /// <param name="year">The year to be validated</param>
        /// <returns>
        ///     The validity of the argument and the parsed year into an integer
        ///     If the argument is invalid, return false and 0
        ///     Else, return true and the parsed year
        /// </returns>
        private static bool ValidateArgument(string[] args, out int year)
        {
            if (args.Length != 1 || !int.TryParse(args[0], out year))
            {
                year = 0;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Register data into JSON files
        /// </summary>
        /// <returns>Whether the data was successfully registered into the JSON files</returns>
        /// <exception cref="HttpRequestException">When the request does not return 200</exception>
        /// <exception cref="InvalidDataException">When the request has missing data</exception>
        /// <exception cref="Exception">When an error occurs</exception>
        /// <exception cref="JsonException">When an error occurs while deserializing the JSON</exception>
        /// <exception cref="IOException">When an error occurs while reading or writing a file</exception>
        private static async Task<bool> RegisterDataIntoJsonFile(int year)
        {
            try
            {
                LogInformation("Fetching data from the URLs");

                var companyDataTask = MakeRequest<CompanyData>(CompanyUrl);
                var serviceDataTask = MakeRequest<ServiceData>(ServiceUrl);
                var businessDataTask = MakeRequest<BusinessData>(BusinessUrl);
                var valueDataTask = MakeRequest<ValueData>(ValueUrl);

                await Task.WhenAll(companyDataTask, serviceDataTask, businessDataTask, valueDataTask);

                IList<CompanyData?> companyData = await companyDataTask;

                IList<ServiceData?> serviceData = await serviceDataTask;

                IList<BusinessData?> businessData = await businessDataTask;

                IList<ValueData?> valueData = await valueDataTask;

                var tasks = new List<Task>();

                LogInformation("Writing data to files");

                if (companyData != null)
                {
                    tasks.Add(Task.Run(() => WriteDataToFile("\\CompanyData.json", companyData, year)));
                }

                if (serviceData != null)
                {
                    tasks.Add(Task.Run(() => WriteDataToFile("\\ServiceData.json", serviceData, year)));
                }

                if (businessData != null)
                {
                    tasks.Add(Task.Run(() => WriteDataToFile("\\BusinessData.json", businessData, year)));
                }

                if (valueData != null)
                {
                    tasks.Add(Task.Run(() => WriteDataToFile("\\ValueData.json", valueData, year)));
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Make a request to a given URL and return the data
        /// </summary>
        /// <typeparam name="T">The type of data to be fetched</typeparam>
        /// <param name="url">The URL to fetch the data from</param>
        /// <returns>The fetched data</returns>
        private static async Task<IList<T?>> MakeRequest<T>(string url)
        {
            try
            {
                LogInformation($"Fetching data from URL for the data type: {typeof(T).Name}");

                var data = await FetchData<T>(url);

                return data;
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                return default;
            }
        }

        /// <summary>
        /// Fetch data from a given URL
        /// </summary>
        /// <typeparam name="T">Type of data to be fetched</typeparam>
        /// <param name="url">Url to fetch the data from</param>
        /// <returns>The fetched data</returns>
        /// <exception cref="HttpRequestException">If the request does not return 200</exception>
        /// <exception cref="InvalidDataException">If the request has missing data</exception>
        private static async Task<IList<T?>> FetchData<T>(string url)
        {
            IList<T?> allData = [];
            var tasks = new List<Task>();

            // Split the dictionary into chunks of 5
            var chunks = _Dim2Mapping
                .Select((entry, index) => new { entry, index })
                .GroupBy(x => x.index / 5)
                .Select(g => g.Select(x => x.entry).ToList())
                .ToList();

            foreach (var chunk in chunks)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using HttpClient httpClient = new();

                    foreach (var mapping in chunk)
                    {
                        string modifiedUrl = $"{url}{mapping.Value}&Dim3={_Dim3}&lang=PT";

                        var response = await httpClient.GetAsync(modifiedUrl);

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException("Request did not return 200");
                        }

                        dynamic? jsonData = await response.Content.ReadAsStringAsync();

                        if (IsDataInvalid(jsonData, out string errorMessage, out IList<T?> deserializedData))
                        {
                            throw new InvalidDataException(errorMessage);
                        }

                        lock (allData)
                        {
                            foreach (var data in deserializedData)
                            {
                                bool isCompanyDataWithTotalLegalCode = data is CompanyData companyData && companyData.LegalFormCode == "T";

                                if (isCompanyDataWithTotalLegalCode)
                                {
                                    continue; // Skip this entry
                                }

                                allData.Add(data);
                            }
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            LogInformation($"Finished fetching data from URL for the data type: {typeof(T).Name}");

            return allData;
        }

        /// <summary>
        /// Check if the information in the serialized object is valid.
        /// Check if data exists and if the entity information is valid
        /// </summary>
        /// <typeparam name="T">The type of data to be checked --> can be none if the data is invalid or one of the following: CompanyData, ServiceData, BusinessData, ValueData</typeparam>
        /// <param name="jsonData">The JSON data to be checked</param>
        /// <param name="errorMessage">The error message to be returned if the data is invalid</param>
        /// <param name="responseDeserializedData">The deserialized data to be returned if the data is valid</param>
        /// <returns>
        ///     If the data is valid, return the deserialized data for <typeparamref name="T"/> type and false.
        ///     Otherwise, return the error message and true.
        /// </returns>
        private static bool IsDataInvalid
            <T>(string jsonData, out string errorMessage, out IList<T>? responseDeserializedData)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                errorMessage = "Request made had missing data >>> JSON was empty";
                responseDeserializedData = default;
                return true;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, // To handle JSON property casing mismatch
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var responseDataList = JsonSerializer.Deserialize<IList<ResponseDto>>(jsonData, options) ?? [];

            // Get the first index of the list and store it in responseData
            var responseData = responseDataList.Count > 0 ? responseDataList[0] : new ResponseDto();

            if (responseData.Dados == null || responseData.Dados.Count == 0)
            {
                errorMessage = "Request made had missing data >>> Data dictionary was empty";
                responseDeserializedData = default;
                return true;
            }

            List<DataDto> data = responseData.Dados.Values.FirstOrDefault() ?? [];

            if (data.Count == 0)
            {
                errorMessage = "Request made had missing data >>> Data list was empty";
                responseDeserializedData = default;
                return true;
            }

            responseDeserializedData = [];

            for (int i = 0; i < data!.Count; i++)
            {
                var dataValue = data[i];

                if (IsEntityInformationValid(dataValue, out errorMessage, out T? deserializedData))
                {
                    if (deserializedData == null)
                    {
                        throw new InvalidDataException(errorMessage);
                    }

                    responseDeserializedData.Add(deserializedData);
                }
            }

            errorMessage = string.Empty;
            return false;
        }

        /// <summary>
        /// Check if the entity information is valid
        /// </summary>
        /// <typeparam name="T">The type of data to be checked --> can be none if the data is invalid or one of the following: CompanyData, ServiceData, BusinessData, ValueData</typeparam>
        /// <param name="responseDeserializedData">The deserialized data to be checked</param>
        /// <param name="errorMessage">The error message to be returned if the data is invalid</param>
        /// <param name="deserializedData">The deserialized data to be returned if the data is valid</param>
        /// <returns>
        ///     If the entity information is valid, return the deserialized data for <typeparamref name="T"/> type and true.
        ///     Otherwise, return the error message and false.
        /// </returns>
        private static bool IsEntityInformationValid<T>(DataDto? dataValue, out string errorMessage, out T? deserializedData)
        {
            if (dataValue == null)
            {
                errorMessage = "Request made had missing data >>> Data value was null";
                deserializedData = default;
                return true;
            }

            errorMessage = string.Empty;

            if (typeof(T) == typeof(CompanyData))
            {
                var companyData = new CompanyData
                {
                    NumberOfCompanies = dataValue.valor,
                    EconomicActivityCode = dataValue.dim_3,
                    EconomicActivityDescription = dataValue.dim_3_t,
                    GeographicAreaCode = dataValue.geocod,
                    GeographicAreaDescription = dataValue.geodsg,
                    LegalFormCode = dataValue.dim_4,
                    LegalFormDescription = dataValue.dim_4_t,
                    ConvSignal = dataValue.sinal_conv,
                    ConvSignalDescription = dataValue.sinal_conv_desc
                };

                bool areMandatoryFieldsValid = !string.IsNullOrEmpty(companyData.EconomicActivityCode) &&
                                               !string.IsNullOrEmpty(companyData.EconomicActivityDescription) &&
                                               !string.IsNullOrEmpty(companyData.GeographicAreaCode) &&
                                               !string.IsNullOrEmpty(companyData.GeographicAreaDescription);

                bool areDependentFieldsValid = !string.IsNullOrEmpty(companyData.NumberOfCompanies) ||
                                               (!string.IsNullOrEmpty(companyData.ConvSignal) &&
                                                !string.IsNullOrEmpty(companyData.ConvSignalDescription));

                if (!areMandatoryFieldsValid || !areDependentFieldsValid)
                {
                    errorMessage = "Request made had missing data >>> Could not deserialize to CompanyData >>> Missing data in values";
                    deserializedData = default;
                    return false;
                }

                deserializedData = (T)(object)companyData;
                return true;
            }
            else if (typeof(T) == typeof(ServiceData))
            {
                var serviceData = new ServiceData
                {
                    NumberOfPeopleWorkingForCompanies = dataValue.valor,
                    EconomicActivityCode = dataValue.dim_3,
                    EconomicActivityDescription = dataValue.dim_3_t,
                    GeographicAreaCode = dataValue.geocod,
                    GeographicAreaDescription = dataValue.geodsg,
                    ConvSignal = dataValue.sinal_conv,
                    ConvSignalDescription = dataValue.sinal_conv_desc
                };

                bool areMandatoryFieldsValid = !string.IsNullOrEmpty(serviceData.EconomicActivityCode) &&
                                               !string.IsNullOrEmpty(serviceData.EconomicActivityDescription) &&
                                               !string.IsNullOrEmpty(serviceData.GeographicAreaCode) &&
                                               !string.IsNullOrEmpty(serviceData.GeographicAreaDescription);

                bool areDependentFieldsValid = !string.IsNullOrEmpty(serviceData.NumberOfPeopleWorkingForCompanies) ||
                                               (!string.IsNullOrEmpty(serviceData.ConvSignal) &&
                                                !string.IsNullOrEmpty(serviceData.ConvSignalDescription));

                if (!areMandatoryFieldsValid || !areDependentFieldsValid)
                {
                    errorMessage = "Request made had missing data >>> Could not deserialize to ServiceData >>> Missing data in values";
                    deserializedData = default;
                    return false;
                }

                deserializedData = (T)(object)serviceData;
                return true;
            }
            else if (typeof(T) == typeof(BusinessData))
            {
                var businessData = new BusinessData
                {
                    NumberOfVolumeOfBusinessForCompanies = dataValue.valor,
                    EconomicActivityCode = dataValue.dim_3,
                    EconomicActivityDescription = dataValue.dim_3_t,
                    GeographicAreaCode = dataValue.geocod,
                    GeographicAreaDescription = dataValue.geodsg,
                    ConvSignal = dataValue.sinal_conv,
                    ConvSignalDescription = dataValue.sinal_conv_desc
                };

                bool areMandatoryFieldsValid = !string.IsNullOrEmpty(businessData.EconomicActivityCode) &&
                                               !string.IsNullOrEmpty(businessData.EconomicActivityDescription) &&
                                               !string.IsNullOrEmpty(businessData.GeographicAreaCode) &&
                                               !string.IsNullOrEmpty(businessData.GeographicAreaDescription);

                bool areDependentFieldsValid = !string.IsNullOrEmpty(businessData.NumberOfVolumeOfBusinessForCompanies) ||
                                               (!string.IsNullOrEmpty(businessData.ConvSignal) &&
                                                !string.IsNullOrEmpty(businessData.ConvSignalDescription));

                if (!areMandatoryFieldsValid || !areDependentFieldsValid)
                {
                    errorMessage = "Request made had missing data >>> Could not deserialize to BusinessData >>> Missing data in values";
                    deserializedData = default;
                    return false;
                }

                deserializedData = (T)(object)businessData;
                return true;
            }
            else if (typeof(T) == typeof(ValueData))
            {
                var valueData = new ValueData
                {
                    IncreasedValueForCompanies = dataValue.valor,
                    EconomicActivityCode = dataValue.dim_3,
                    EconomicActivityDescription = dataValue.dim_3_t,
                    GeographicAreaCode = dataValue.geocod,
                    GeographicAreaDescription = dataValue.geodsg,
                    ConvSignal = dataValue.sinal_conv,
                    ConvSignalDescription = dataValue.sinal_conv_desc
                };

                bool areMandatoryFieldsValid = !string.IsNullOrEmpty(valueData.EconomicActivityCode) &&
                                               !string.IsNullOrEmpty(valueData.EconomicActivityDescription) &&
                                               !string.IsNullOrEmpty(valueData.GeographicAreaCode) &&
                                               !string.IsNullOrEmpty(valueData.GeographicAreaDescription);

                bool areDependentFieldsValid = !string.IsNullOrEmpty(valueData.IncreasedValueForCompanies) ||
                                               (!string.IsNullOrEmpty(valueData.ConvSignal) &&
                                                !string.IsNullOrEmpty(valueData.ConvSignalDescription));

                if (!areMandatoryFieldsValid || !areDependentFieldsValid)
                {
                    errorMessage = "Request made had missing data >>> Could not deserialize to ValueData >>> Missing data in values";
                    deserializedData = default;
                    return false;
                }

                deserializedData = (T)(object)valueData;
                return true;
            }

            errorMessage = "Request made had missing data >>> Data value was invalid";
            deserializedData = default;
            return false;
        }

        /// <summary>
        /// Write data to a file
        /// </summary>
        /// <typeparam name="T">The type of data to be written to the file</typeparam>
        /// <param name="fileName">The name of the file to be written to</param>
        /// <param name="data">The data to be written to the file</param>
        private static void WriteDataToFile<T>(string fileName, T data, int year)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, JsonSerializerOptions);

                // Construct the path to the Data folder in the solution directory
                var directory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory()));

                while (directory != null && directory.GetFiles("*.sln").Length == 0)
                {
                    directory = directory.Parent;
                }

                if (directory == null || directory.GetFiles("*.sln").Length == 0)
                {
                    directory = new DirectoryInfo(
                        Path.Combine(Directory.GetCurrentDirectory()));
                }

                // Combine the data directory path with the file name
                string dataPath = $"{directory.FullName}{DataPath}";
                string yearPath = $"{dataPath}{year}";
                string fullPath = $"{yearPath}{fileName}";

                if (!Directory.Exists(dataPath))
                {
                    Directory.CreateDirectory(dataPath);
                }

                if (!Directory.Exists(yearPath))
                {
                    Directory.CreateDirectory(yearPath);
                }

                // Write the JSON data to the file
                File.WriteAllText(fullPath, json);

                LogInformation($"Data written to file: {fullPath}");
            }
            catch (Exception ex)
            {
                LogError($"An error occurred while writing data to file: {ex.Message}");
            }
        }

        /// <summary>
        /// Log error message
        /// </summary>
        /// <param name="message">The message to be logged into the console when this method is called</param>
        private static void LogError(string message)
        {
            var logMessage = $"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] Fetching Data did not work on step: {message}";
            Console.WriteLine(logMessage);
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