using static System.Net.WebRequestMethods;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using Extract.Data.SaveJson.dtos;

namespace Extract.Data.SaveJson
{
    public static class Program
    {
        private const string CompanyUrl = "https://www.ine.pt/ine/json_indicador/pindica.jsp?op=2&varcd=0008511&Dim1=S7A2022&Dim2=PT,1,11,111,1111601,1111602,1111603,1111604,1111605,1111606,1111607,1111608,1111609,1111610,112,1120301,1120302,1120303,1120306,1120310,1120313,119,1190304,1190307,1190308,1191705,1190309,1190311,1190312,1190314,11A,11B,11C,11C1301,11C1302,11C0106,11C0305,11C1804,11C1303,11C1305,11C1307,11C1309,11C1311,11C1813,11D,11E,16,17,18,15,2,20,3&Dim3=TOT,A,B,C,D,E,F,G,H,I,J,L,M,N,P,Q,R,S&lang=PT";

        private const string ServiceUrl = "https://www.ine.pt/ine/json_indicador/pindica.jsp?op=2&varcd=0008512&Dim1=S7A2022,S7A2021&Dim2=PT,1,11,111,1111601,1111602,1111603,1111604,1111605,1111606,1111607,1111608,1111609,1111610,112,1120301,1120302,1120303,1120306,1120310,1120313,119,1190304,1190307,1190308,1191705,1190309,1190311,1190312,1190314,11A,11B,11C,11C1301,11C1302,11C0106,11C0305,11C1804,11C1303,11C1305,11C1307,11C1309,11C1311,11C1813,11D,11E,16,17,18,15,2,20,3&Dim3=TOT&lang=PT";

        private const string BusinessUrl = "https://www.ine.pt/ine/json_indicador/pindica.jsp?op=2&varcd=0008513&Dim1=S7A2022,S7A2021&Dim2=PT,1,11,111,1111601,1111602,1111603,1111604,1111605,1111606,1111607,1111608,1111609,1111610,112,1120301,1120302,1120303,1120306,1120310,1120313,119,1190304,1190307,1190308,1191705,1190309,1190311,1190312,1190314,11A,11B,11C,11C1301,11C1302,11C0106,11C0305,11C1804,11C1303,11C1305,11C1307,11C1309,11C1311,11C1813,11D,11E,16,17,18,15,2,20,3&Dim3=TOT&lang=PT";

        private const string ValueUrl = "https://www.ine.pt/ine/json_indicador/pindica.jsp?op=2&varcd=0008514&Dim1=S7A2022&Dim2=PT,1,11,111,1111601,1111602,1111603,1111604,1111605,1111606,1111607,1111608,1111609,1111610,112,1120301,1120302,1120303,1120306,1120310,1120313,119,1190304,1190307,1190308,1191705,1190309,1190311,1190312,1190314,11A,11B,11C,11C1301,11C1302,11C0106,11C0305,11C1804,11C1303,11C1305,11C1307,11C1309,11C1311,11C1813,11D,11E,16,17,18,15,2,20,3&Dim3=TOT&lang=PT";

        private static async Task Main()
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
            var companyData = await MakeRequest<ResponseDto>(CompanyUrl);
            var serviceData = await MakeRequest<ResponseDto>(ServiceUrl);
            var businessData = await MakeRequest<ResponseDto>(BusinessUrl);
            var valueData = await MakeRequest<ResponseDto>(ValueUrl);

            if (companyData != null)
                WriteDataToFile("CompanyData.json", companyData);

            if (serviceData != null)
                WriteDataToFile("ServiceData.json", serviceData);

            if (businessData != null)
                WriteDataToFile("BusinessData.json", businessData);

            if (valueData != null)
                WriteDataToFile("ValueData.json", valueData);
        }

        /// <summary>
        /// Make a request to a given URL and return the data
        /// </summary>
        /// <typeparam name="T">The type of data to be fetched</typeparam>
        /// <param name="url">The URL to fetch the data from</param>
        /// <returns>The fetched data</returns>
        private static async Task<T?> MakeRequest<T>(string url)
        {
            try
            {
                var data = await FetchData<T>(url);
                if (data == null)
                {
                    LogError("Request had missing data");
                    return default;
                }
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
        private static async Task<T?> FetchData<T>(string url)
        {
            HttpClient httpClient = new();
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Request did not return 200");
            }

            var jsonData = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(jsonData))
            {
                throw new InvalidDataException("Request had missing data >>> JSON was empty");
            }

            return JsonSerializer.Deserialize<T>(jsonData);
        }

        /// <summary>
        /// Write data to a file
        /// </summary>
        /// <typeparam name="T">The type of data to be written to the file</typeparam>
        /// <param name="fileName">The name of the file to be written to</param>
        /// <param name="data">The data to be written to the file</param>
        private static void WriteDataToFile<T>(string fileName, T data)
        {
            var json = JsonSerializer.Serialize(data);

            if (!ValidateSerializedData(json, out string errorMessage, out Response response))
            {
                LogError(errorMessage);
                return;
            }

            if (!System.IO.File.Exists(fileName))
            {
                System.IO.File.Create(fileName).Dispose();
            }

            System.IO.File.WriteAllText(fileName, response);
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
    }
}