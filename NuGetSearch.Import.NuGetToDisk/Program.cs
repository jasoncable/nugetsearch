using System;
using Newtonsoft.Json;
using RestSharp;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NuGetSearch.Models;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using NuGetSearch.Import.DiskToElasticSearch;

namespace NuGetSearch.Import.NuGetToDisk
{
    class Program
    {
        static string _baseDir;
        static string _cursorFile;
        static string _metaDataFile;
        static string _cacheFiles;
        static string _errLogFile;
        static Dictionary<string, Guid> _nameToIdMap = new Dictionary<string, Guid>();
        static Dictionary<Guid, MetaDataInfo> _metaDataInfo = new Dictionary<Guid, MetaDataInfo>();

        static async Task<int> Main(string[] args)
        {
            Configure();

            using (StreamWriter errLog = new StreamWriter(_errLogFile))
            {
                try
                {
                    await Initial(errLog);
                }
                catch(Exception exc) 
                {
                    errLog.WriteLine(exc.ToString());
                }
                finally 
                {
                    errLog.Flush();
                }
            }

            Console.WriteLine("DONE");

            return 0;
        }

        async static Task<int> Initial(StreamWriter errLog)
        {
            DateTime startDate = new DateTime(1970, 1, 1);

            if (File.Exists(_cursorFile)){
                using (StreamReader sr = new StreamReader(_cursorFile))
                {
                    startDate = Convert.ToDateTime(sr.ReadToEnd());
                }
            };

            errLog.WriteLine("Starting with date: " + startDate.ToString());
            errLog.Flush();

            using (StreamWriter sw = new StreamWriter(_cursorFile, false))
            {
                sw.Write(startDate.ToString("o"));
            }

            FileInfo fi = new FileInfo(_metaDataFile);
            if ( fi != null && fi.Exists && fi.Length > 0)
            {
                using (StreamReader metadataSR = new StreamReader(_metaDataFile))
                {
                    string line;
                    while((line = metadataSR.ReadLine()) != null)
                    {
                        string[] sa = line.Split('\t');
                        _nameToIdMap[sa[0]] = new Guid(sa[1]);
                    }
                }
            }

            using (StreamWriter metaData = new StreamWriter(_metaDataFile, true))
            {
                RestClient restClient = new RestClient("https://api.nuget.org");
                restClient.AddDefaultHeader("X-NuGet-Protocol-Version", "4.1.0");
                restClient.AddDefaultHeader("X-NuGet-Session-Id", Guid.NewGuid().ToString());
                restClient.ClearHandlers();
                restClient.AddHandler(new JsonNetSerializer(), new string[] { "application/json" });

                RestRequest request = new RestRequest("v3/index.json", Method.GET);
                IRestResponse<Services> response = restClient.Execute<Services>(request);
                Services services = response.Data;

                string catalogRootUrl = services.Resources.Single(x => x.Type == "Catalog/3.0.0").Id;
                Uri uri = new Uri(catalogRootUrl);

                restClient.BaseUrl = new Uri(uri.Scheme + "://" + uri.Host);
                request = new RestRequest(uri.AbsolutePath, Method.GET);

                CatalogIndex index;

                try
                {
                    index = restClient.Execute<CatalogIndex>(request).Data;
                }
                catch(Exception exc)
                {
                    errLog.WriteLine(exc);
                    metaData.Flush();
                    return 1;
                }


                foreach (CatalogPage page in index.Items.Where(x => x.CommitTimeStamp >= startDate).OrderBy(x => x.CommitTimeStamp))
                {
                    startDate = page.CommitTimeStamp;

                    uri = new Uri(page.Id);
                    restClient.BaseUrl = new Uri(uri.Scheme + "://" + uri.Host);
                    request = new RestRequest(uri.AbsolutePath, Method.GET);
                    IRestResponse<CatalogPage> response2;

                    try
                    {
                        response2 = restClient.Execute<CatalogPage>(request);
                    }
                    catch (Exception exc)
                    {
                        errLog.WriteLine(exc.ToString());
                        metaData.Flush();
                        return 1;
                    }

                    if (response2.StatusCode != HttpStatusCode.OK)
                    {
                        continue;
                    }

                    CatalogPage details = response2.Data;

                    if (details.Items != null && details.Items.Count > 0)
                    {
                        foreach (var item in details.Items.OrderBy( x=> x.CommitTimeStamp))
                        {
                            Guid id;
                            if (!_nameToIdMap.TryGetValue(item.PackageName, out id))
                            {
                                id = Guid.NewGuid();
                                _nameToIdMap.Add(item.PackageName, id);
                            }

                            await metaData.WriteLineAsync($"{item.PackageName}\t{id}\t{item.PackageVersion}\t" +
                                $"{item.CommitTimeStamp.ToUnixTime()}\t{item.IsDelete}");
                        }

                        var newest = details.Items.Where(x=> !x.IsDelete).GroupBy(x => x.PackageName).Select(
                            x => x.OrderByDescending(y => y.CommitTimeStamp).First()).ToList();

                        foreach (var item in newest)
                        {
                            Guid id = _nameToIdMap[item.PackageName];

                            uri = new Uri(item.Id);
                            restClient.BaseUrl = new Uri(uri.Scheme + "://" + uri.Host);
                            request = new RestRequest(uri.PathAndQuery, Method.GET);
                            IRestResponse<PackageDetail> response1;

                            try
                            {
                                response1 = restClient.Execute<PackageDetail>(request);
                            }
                            catch(Exception exc)
                            {
                                errLog.WriteLine(exc.ToString());
                                metaData.Flush();
                                return 1;
                            }

                            if (response1.StatusCode == HttpStatusCode.NotFound)
                            {
                                continue;
                            }

                            PackageDetail pd = response1.Data;

                            if (pd != null)
                            {
                                string directory = Path.Combine(_cacheFiles, id.ToString());
                                Directory.CreateDirectory(directory);

                                string myFile = Path.Combine(directory,
                                     pd.CommitTimeStamp.ToUnixTime().ToString() + ".gz");

                                if (File.Exists(myFile))
                                    File.Delete(myFile);

                                using (FileStream outFile = File.Create(myFile))
                                using (GZipStream compress = new GZipStream(outFile, CompressionMode.Compress))
                                using (StreamWriter writer = new StreamWriter(compress))
                                {
                                    writer.Write(JsonConvert.SerializeObject(pd));
                                }
                            }
                        }
                    }

                    if (File.Exists(_cursorFile))
                        File.Delete(_cursorFile);

                    using (StreamWriter sw = new StreamWriter(_cursorFile, false))
                    {
                        sw.Write(startDate.ToString("o"));
                    }
                }
            }

            return 0;
        }

        static void Configure()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            IConfigurationRoot configuration = builder.Build();
            var section = configuration.GetSection("NuGetSearch");
            _baseDir = section["CacheFileDirectory"];
            _cursorFile = Path.Combine(_baseDir, "cursor.txt");
            _metaDataFile = Path.Combine(_baseDir, "metadata.txt");
            _cacheFiles = Path.Combine(_baseDir, "files");
            _errLogFile = Path.Combine(_baseDir, "errors.txt");
        }

    }
}
