using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SSMT
{
    public class GitHubReleaseInfo
    {
        [JsonPropertyName("tag_name")]  // 添加这个属性映射
        public string TagName { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("assets")]
        public Asset[] Assets { get; set; }
    }

    public class Asset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }
    public class GitHubApiHelper
    {
        private readonly HttpClient _httpClient = new();

        public async Task<GitHubReleaseInfo> GetLatestRelease(string owner, string repo)
        {
            var url = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("request");


            var release = await _httpClient.GetFromJsonAsync<GitHubReleaseInfo>(url);
            return release;
        }


        public async Task<string> DownloadReleaseZip(string downloadUrl, string downloadPath)
        {
            var fileName = Path.GetFileName(new Uri(downloadUrl).AbsolutePath);
            var fullPath = Path.Combine(downloadPath, fileName);

            using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fs);

            Console.WriteLine($"Downloaded to: {fullPath}");
            return fullPath;
        }

        public void ExtractZip(string zipPath, string extractPath)
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath, overwriteFiles: true);
            Console.WriteLine($"Extracted to: {extractPath}");
        }

        public void DeleteZip(string zipPath)
        {
            File.Delete(zipPath);
            Console.WriteLine("ZIP file deleted.");
        }


    }

}
