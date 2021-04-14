using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;

namespace Nop.Plugin.Misc.Scheduler.Services
{
    public class S3Service: IDisposable
    {
        private string _apiKey;
        private string _endPoint;
        private Guid _organisationIdentifier;

        public S3Service()
        {
        }

        public  S3Service (string endPoint, string apiKey, Guid organisationIdentifier)
        {
            
            _endPoint = endPoint;
            _apiKey = apiKey;
            _organisationIdentifier = organisationIdentifier != default ? organisationIdentifier : throw new ArgumentNullException();
            
        }

        private IRestClient getRestClient(string url) => new RestClient(url) {Timeout = -1};
        private IRestRequest getRestRequest(Method httpMethod) => new RestRequest(httpMethod)
                .AddHeader("x-api-key", _apiKey);

        private string createValidPath(string path)
        {
            if (path.StartsWith('/')) throw new ArgumentOutOfRangeException(nameof(path));
            var newPath = $"{_organisationIdentifier}/{path}";
            return newPath == path ? path : newPath;
        }

        public async Task<IRestResponse> ListFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || folder.Last() != '/')
                throw new ArgumentException(nameof(folder));
            
            var client = getRestClient(_endPoint)
                .AddDefaultQueryParameter("folder", createValidPath(folder));

            var request = getRestRequest(Method.GET);
            return await client.ExecuteAsync(request);
        }

        private async Task<string> getFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !filePath.EndsWith(".zip")) throw new ArgumentException(null, nameof(filePath));
            
            var client = getRestClient(_endPoint)
                .AddDefaultQueryParameter("key", createValidPath(filePath));
            var request = getRestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
            return response.Content.Replace('"', ' ').Trim();
        }

        /// <summary>
        /// Downloads a zipArchive from the server
        /// </summary>
        /// <param name="filePath">filePath on the server</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task DownloadFile( string filePath)
        {
            var sourceUrl = await getFile(filePath);
            if (string.IsNullOrWhiteSpace(sourceUrl)) throw new ArgumentOutOfRangeException(filePath);
            if (Directory.Exists(filePath)) throw new ArgumentException(filePath);

            var client = getRestClient(sourceUrl);
            var request = getRestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);

            await using var fileStream = File.Create(filePath);
            foreach (var b in response.RawBytes)
                fileStream.WriteByte(b);

            // todo verify download
        }

        private record UrlRequest(string key);
        private async Task<string> getUploadUrl(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException(nameof(filePath));
            var body = new UrlRequest(createValidPath(filePath));
            
            var client = getRestClient(_endPoint);
            var request = getRestRequest(Method.POST)
                .AddHeader("Content-Type", "application/json")
                .AddJsonBody(body);

            var response = await client.ExecuteAsync(request);
            return response.Content.Replace('"', ' ').Trim();
        }
        /// <summary>
        /// Uploads a ZipArchive 
        /// </summary>
        /// <param name="filePath">Path in the Server</param>
        /// <param name="stream">MemoryStream</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Invalid filePath</exception>
        public async Task<IRestResponse> UploadZip(string filePath, byte[] bytes)
        {
            var uploadUrl = await getUploadUrl(filePath);
            if (string.IsNullOrWhiteSpace(uploadUrl)) throw new ArgumentOutOfRangeException(nameof(filePath));
            
            var client = getRestClient(uploadUrl);
            var request = new RestRequest(Method.PUT);
            request.AddHeader("Content-Type", "application/zip");
            request.AddParameter("application/zip", value: bytes, ParameterType.RequestBody);
            return await client.ExecuteAsync(request);
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}