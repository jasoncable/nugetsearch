using System;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NuGetSearch.Models;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using Nest;
using NuGetSearch.Repositories;

namespace NuGetSearch.Import.DiskToElasticSearch
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
        static Uri _nodeLocation;
        static ConnectionSettings _settings;
        static IndexNameResolver _resolver;
        static string _indexName;
        static ElasticClient _client;

        static async Task<int> Main(string[] args)
        {
            Configure();

            using (StreamWriter errLog = new StreamWriter(_errLogFile))
            {
                try
                {
                    await InitialToElastic(errLog);
                }
                catch (Exception exc)
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

        static async Task<int> InitialToElastic(StreamWriter errLog)
        {
            _client.CreateIndex(_indexName,
                  x => NuGetSearchMainRepo.CreateIndex(_indexName)
            );

            using (StreamReader sw = new StreamReader(_metaDataFile))
            {
                string line;
                while ((line = sw.ReadLine()) != null)
                {
                    if (String.IsNullOrWhiteSpace(line))
                        continue;

                    string[] sa = line.Split('\t');
                    Guid id = new Guid(sa[1]);
                    _nameToIdMap[sa[0]] = id;

                    if (_metaDataInfo.ContainsKey(id))
                    {
                        if (sa[4] == Boolean.TrueString)
                        {
                            _metaDataInfo.Remove(id);
                            _nameToIdMap.Remove(sa[0]);

                            var eResult = _client.Get<NuGetSearchMain>(
                                new GetRequest<NuGetSearchMain>(_indexName, _indexName, new Guid())
                            );

                            if (eResult != null && eResult.Found)
                            {
                                _client.Delete<NuGetSearchMain>(
                                    new DocumentPath<NuGetSearchMain>(id)
                                );
                            }
                        }
                        else
                        {
                            _metaDataInfo[id].LatestVersion = Convert.ToInt32(sa[3]);
                            _metaDataInfo[id].Versions.Add(sa[2]);
                        }
                    }
                    else
                    {
                        _metaDataInfo.Add(id,
                            new MetaDataInfo
                            {
                                Id = id,
                                Name = sa[0],
                                LatestVersion = Convert.ToInt32(sa[3]),
                                Versions = new List<string> { sa[2] }
                            });
                    }

                }
            }

            foreach (var item in _metaDataInfo)
            {
                var eResult = _client.Get<NuGetSearchMain>(
                    new GetRequest<NuGetSearchMain>(_indexName, _indexName, item.Key)
                );

                PackageDetail pd;

                string gzFile = Path.Combine(
                        Path.Combine(_cacheFiles,
                            item.Key.ToString().ToLower()), item.Value.LatestVersion.ToString() + ".gz");

                if (!File.Exists(gzFile))
                {
                    var dir = Path.GetDirectoryName(gzFile);
                    if (Directory.Exists(dir))
                    {
                        var files = new DirectoryInfo(dir).GetFiles();
                        if (files.Length > 0)
                        {
                            gzFile = files.OrderByDescending(x => x.CreationTime).FirstOrDefault().FullName;
                        }
                        else
                        {
                            gzFile = null;
                        }

                    }
                    else
                    {
                        gzFile = null;
                    }
                }

                if (gzFile == null)
                    continue;

                using (FileStream outFile = File.OpenRead(gzFile))
                using (GZipStream compress = new GZipStream(outFile, CompressionMode.Decompress))
                using (StreamReader reader = new StreamReader(compress))
                {
                    pd = JsonConvert.DeserializeObject<PackageDetail>(reader.ReadToEnd());
                }

                if (eResult != null && eResult.Found)
                {
                    if (!pd.Listed)
                    {
                        _client.Delete<NuGetSearchMain>(
                            new DocumentPath<NuGetSearchMain>(eResult.Id)
                        );
                        continue;
                    }


                    eResult.Source.Versions = item.Value.Versions.Concat(eResult.Source.Versions).Distinct().ToList();

                    if (!String.IsNullOrWhiteSpace(pd.Authors))
                    {
                        eResult.Source.Authors = pd.Authors.Split(',').ToList();
                    }

                    eResult.Source.Description = pd.Description;
                    eResult.Source.IconUrl = pd.IconUrl;
                    eResult.Source.ProjectUrl = pd.ProjectUrl;
                    eResult.Source.ReleaseNotes = pd.ReleaseNotes;
                    eResult.Source.Summary = pd.Summary;
                    eResult.Source.Tags = pd.Tags;
                    eResult.Source.Title = pd.Title;
                    eResult.Source.CommitTimeStamp = pd.CommitTimeStamp;
                    eResult.Source.DependencyGroups = CreateDepGroups(pd);

                    await _client.IndexDocumentAsync<NuGetSearchMain>(eResult.Source);
                }
                else
                {
                    if (!pd.Listed)
                        continue;

                    NuGetSearchMain nsm = new NuGetSearchMain();
                    nsm.Id = item.Key;
                    nsm.Name = item.Value.Name;

                    nsm.Versions = item.Value.Versions.Distinct().ToList();

                    if (!String.IsNullOrWhiteSpace(pd.Authors))
                    {
                        nsm.Authors = pd.Authors.Split(',').ToList();
                    }

                    nsm.Description = pd.Description;
                    nsm.IconUrl = pd.IconUrl;
                    nsm.ProjectUrl = pd.ProjectUrl;
                    nsm.ReleaseNotes = pd.ReleaseNotes;
                    nsm.Summary = pd.Summary;
                    nsm.Tags = pd.Tags;
                    nsm.Title = pd.Title;
                    nsm.CommitTimeStamp = pd.CommitTimeStamp;
                    nsm.DependencyGroups = CreateDepGroups(pd);

                    await _client.IndexDocumentAsync<NuGetSearchMain>(nsm);
                }
            }

            return 0;
        }

        static List<NuGetSearchMainDependencyGroup> CreateDepGroups(PackageDetail pd)
        {
            List<NuGetSearchMainDependencyGroup> newDepGroups = new List<NuGetSearchMainDependencyGroup>();

            if (pd.Dependencies == null || pd.Dependencies.Count == 0)
                return null;

            foreach (var depGroup in pd.Dependencies)
            {
                var newDepGroup = new NuGetSearchMainDependencyGroup();

                newDepGroup.TargetFramework = depGroup.TargetFramework;
                newDepGroup.Dependencies = new List<NuGetSearchMainDependency>();

                if (depGroup.Dependencies != null && depGroup.Dependencies.Count > 0)
                {
                    foreach (var dep in depGroup.Dependencies)
                    {
                        var newDep = new NuGetSearchMainDependency
                        {
                            Name = dep.PackageName,
                            VersionRange = dep.Range
                        };
                        newDepGroup.Dependencies.Add(newDep);
                    }
                }

                newDepGroups.Add(newDepGroup);
            }

            return newDepGroups;
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

            _nodeLocation = new Uri(section["ServerUri"]);
            _settings = new ConnectionSettings(_nodeLocation)
                .DefaultMappingFor<NuGetSearchMain>(m => m.IndexName(section["IndexName"])
                    .IdProperty("Id"));
            _resolver = new IndexNameResolver(_settings);
            _indexName = _resolver.Resolve<NuGetSearchMain>();
            _client = new ElasticClient(_settings);
        }
    }
}
