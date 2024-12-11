using static System.Net.WebRequestMethods;
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
        private const string CompanyUrl = "https://www.ine.pt/ine/json_indicador/pindica.jsp?op=2&varcd=0008511&Dim1=S7A2022&Dim2=PT,1,11,111,1111601,1111602,1111603,1111604,1111605,1111606,1111607,1111608,1111609,1111610,112,1120301,1120302,1120303,1120306,1120310,1120313,119,1190304,1190307,1190308,1191705,1190309,1190311,1190312,1190314,11A,11B,11C,11C1301,11C1302,11C0106,11C0305,11C1804,11C1303,11C1305,11C1307,11C1309,11C1311,11C1813,11D,11E,16,17,18,15,2,20,3&Dim3=TOT,A,B,C,D,E,F,G,H,I,J,L,M,N,P,Q,R,S&lang=PT";

        private const string ServiceUrl = "https://www.ine.pt/ine/json_indicador/pindica.jsp?op=2&varcd=0008512&Dim1=S7A2022&Dim2=PT,1,11,111,1111601,1111602,1111603,1111604,1111605,1111606,1111607,1111608,1111609,1111610,112,1120301,1120302,1120303,1120306,1120310,1120313,119,1190304,1190307,1190308,1191705,1190309,1190311,1190312,1190314,11A,11B,11C,11C1301,11C1302,11C0106,11C0305,11C1804,11C1303,11C1305,11C1307,11C1309,11C1311,11C1813,11D,11E,16,17,18,15,2,20,3&Dim3=TOT,A,B,C,D,E,F,G,H,I,J,L,M,N,P,Q,R,S&lang=PT";

        private const string BusinessUrl = "https://www.ine.pt/ine/json_indicador/pindica.jsp?op=2&varcd=0008513&Dim1=S7A2022&Dim2=PT,1,11,111,1111601,1111602,1111603,1111604,1111605,1111606,1111607,1111608,1111609,1111610,112,1120301,1120302,1120303,1120306,1120310,1120313,119,1190304,1190307,1190308,1191705,1190309,1190311,1190312,1190314,11A,11B,11C,11C1301,11C1302,11C0106,11C0305,11C1804,11C1303,11C1305,11C1307,11C1309,11C1311,11C1813,11D,11E,16,17,18,15,2,20,3&Dim3=TOT,A,B,C,D,E,F,G,H,I,J,L,M,N,P,Q,R,S&lang=PT";

        private const string ValueUrl = "https://www.ine.pt/ine/json_indicador/pindica.jsp?op=2&varcd=0008514&Dim1=S7A2022&Dim2=PT,1,11,111,1111601,1111602,1111603,1111604,1111605,1111606,1111607,1111608,1111609,1111610,112,1120301,1120302,1120303,1120306,1120310,1120313,119,1190304,1190307,1190308,1191705,1190309,1190311,1190312,1190314,11A,11B,11C,11C1301,11C1302,11C0106,11C0305,11C1804,11C1303,11C1305,11C1307,11C1309,11C1311,11C1813,11D,11E,16,17,18,15,2,20,3&Dim3=TOT,A,B,C,D,E,F,G,H,I,J,L,M,N,P,Q,R,S&lang=PT";

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static async Task Main()
        {
            await RegisterDataIntoJsonFile();
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
        private static async Task<bool> RegisterDataIntoJsonFile()
        {
            try
            {
                IList<CompanyData?> companyData = await MakeRequest<CompanyData>(CompanyUrl);
                IList<ServiceData?> serviceData = await MakeRequest<ServiceData>(ServiceUrl);
                IList<BusinessData?> businessData = await MakeRequest<BusinessData>(BusinessUrl);
                IList<ValueData?> valueData = await MakeRequest<ValueData>(ValueUrl);

                if (companyData != null)
                {
                    WriteDataToFile("./Data/CompanyData.json", companyData);
                }

                if (serviceData != null)
                {
                    WriteDataToFile("./Data/ServiceData.json", serviceData);
                }

                if (businessData != null)
                {
                    WriteDataToFile("./Data/BusinessData.json", businessData);
                }

                if (valueData != null)
                {
                    WriteDataToFile("./Data/ValueData.json", valueData);
                }
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
        /// <typeparam name="T">The type of data to be fetched</typeparam>
        /// <param name="url">The URL to fetch the data from</param>
        /// <returns>The fetched data</returns>
        /// <exception cref="HttpRequestException">When the request does not return 200</exception>
        /// <exception cref="InvalidDataException">When the request has missing data</exception>
        private static async Task<IList<T?>> FetchData<T>(string url)
        {
            using HttpClient httpClient = new();
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Request did not return 200");
            }

            dynamic? jsonData = await response.Content.ReadAsStringAsync();

            if (IsDataInvalid(jsonData, out string errorMessage, out IList<T?> deserializedData))
            {
                throw new InvalidDataException(errorMessage);
            }

            return deserializedData;
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
        private static void WriteDataToFile<T>(string fileName, T data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, JsonSerializerOptions);

                LogInformation($"Writing data to file: {fileName}");

                // Construct the path to the Data folder in the solution directory
                var directory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory()));

                while (directory != null && !directory.GetFiles("*.sln").Any())
                {
                    directory = directory.Parent;
                }

                if (directory == null || directory.GetFiles("*.sln").Length == 0)
                {
                    directory = new DirectoryInfo(
                        Path.Combine(Directory.GetCurrentDirectory()));
                }

                // Combine the data directory path with the file name
                string fullPath = Path.Combine(directory.FullName, fileName);

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