﻿using Synapse;
using Synapse.Api.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Utf8Json;
using VT_Api.Exceptions;
using VT_Api.Reflexion;

namespace VT_Api.Core.Plugin.Updater
{
    public abstract class AbstractUpdateHandler<T> : IUpdateHandler<T>
        where T : IPlugin
    {
        #region Properties & Variable
        public const string Unknow = "Unknown";
        public const string GitHubPage = "https://api.github.com/repositories/{0}/releases/?per_page=20&page=1";
        public const string DefaultsRegexVersion = @"[v,V]?(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)";

        private string _tempDirectory;
        public string TempDirectory
        {
            get
            {
                if (!Directory.Exists(_tempDirectory))
                {
                    Directory.CreateDirectory(_tempDirectory);
                }

                return _tempDirectory;
            }
            private set
            {
                _tempDirectory = value;
            }
        }

        private string _oldDllDirectory;
        public string OldDllDirectory
        {
            get
            {
                if (!Directory.Exists(_oldDllDirectory))
                {
                    if (!Directory.Exists(_tempDirectory))
                        Directory.CreateDirectory(_tempDirectory);
                    
                    Directory.CreateDirectory(_oldDllDirectory);
                }

                return _oldDllDirectory;
            }
            private set
            {
                _oldDllDirectory = value;
            }
        }


        private string _DownloadDirectory;
        public string DownloadDirectory
        {
            get
            {
                if (!Directory.Exists(_DownloadDirectory))
                {
                    if (!Directory.Exists(_tempDirectory))
                        Directory.CreateDirectory(_tempDirectory);

                    Directory.CreateDirectory(_DownloadDirectory);
                }

                return _DownloadDirectory;
            }
            private set
            {
                _DownloadDirectory = value;
            }
        }

        public abstract long GithubID { get; }
        public virtual string RegexExpressionVersion { get; } = DefaultsRegexVersion;
        #endregion

        #region Constructor & Destructor
        public AbstractUpdateHandler()
        {
            TempDirectory = Path.Combine(Server.Get.Files.SynapseDirectory, "temp");
            DownloadDirectory = Path.Combine(_tempDirectory, "download");
            OldDllDirectory = Path.Combine(_tempDirectory, "old");
        }
        #endregion

        #region Methods
        public void DeletetTempDirectory()
        {
            if (!Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory);
            }
        }

        public virtual bool TryDownload(HttpClient client, Release release, string name, out string filePath)
        {
            var asset = release.Assets.FirstOrDefault(r => r.Name.Contains(name) && r.Name.Contains(".dll"));

            if (asset.Size == 0)
            {
                filePath = string.Empty;
                return false;
            }

            using var reponse = client.GetAsync(asset.BrowserDownloadUrl).ConfigureAwait(false).GetAwaiter().GetResult();
            using var stream = reponse.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            filePath = Path.Combine(DownloadDirectory, asset.Name);
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            stream.CopyToAsync(fileStream).ConfigureAwait(false).GetAwaiter().GetResult();

            return true;
        }

        public virtual Version GetPluginVersion()
        {
            var info = (PluginInformation)Attribute.GetCustomAttribute(typeof(T), typeof(PluginInformation));
            if (info.Version == Unknow)
            {
                if (info.Name == Unknow)
                    throw new VtUnknownVersionException($"Vt-AutoUppdate : The plugin in the assembly {typeof(T).Assembly.GetName().Name} did not set its version", typeof(T).Assembly.FullName);
                else
                    throw new VtUnknownVersionException($"Vt-AutoUppdate : The plugin {info.Name} in the assembly {typeof(T).Assembly.GetName().Name} did not set its version", typeof(T).Assembly.FullName, info.Name);
            }
            return new Version(info.Version, RegexExpressionVersion);
        }

        public virtual Version GetGithubVersion(HttpClient client, out Release release, bool ignorePrerealase = true)
        {
            var realases = GetRealases(client);

            Version highestVersion = new Version(0,0,0);
            Release highestRelease = null;
            foreach (var realase in realases)
            {
                if (ignorePrerealase && realase.PreRelease)
                    continue;
                if (Version.TryParse(realase.TagName, out var version) && version > highestVersion)
                {
                    highestVersion = version;
                    highestRelease = realase;
                }
            }
            release = highestRelease;
            return highestVersion;
        } 

        private List<Release> GetRealases(HttpClient client)
        {
            client.Timeout = TimeSpan.FromSeconds(500);
            client.DefaultRequestHeaders.Add("User-Agent", $"VT-API");

            var url = string.Format(GitHubPage, GithubID);
            using var response = client.GetAsync(url).ConfigureAwait(false).GetAwaiter().GetResult();
            using var stream = response.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            
            return JsonSerializer.Deserialize<Release[]>(stream).OrderByDescending(r => r.CreatedAt.Ticks).ToList();
        }

        public virtual bool NeedToUpdate(Version PluginVersion, Version GitVersion)
            => PluginVersion < GitVersion;
            
        public virtual void Replace(string newPluginPath)
        {
            var plugins = SynapseController.PluginLoader.GetFieldValueOrPerties<List<IPlugin>>("_plugins");
            var plugin  = plugins.First(p => p.GetType() == typeof(T));

            if (plugin == null)
                throw new Exception($"Update Plugin of {typeof(T).Assembly.GetName().Name} but the instnace of the plugin is not found !");


            var pluginName = typeof(T).Assembly.GetName().Name + ".dll";
            var pluginPath = Path.Combine(plugin.PluginDirectory, pluginName);
            var pluginNewPath = Path.Combine(OldDllDirectory, pluginName);

            var newPluginName = Path.GetFileName(pluginPath);
            var newPluginNewPath = Path.Combine(plugin.PluginDirectory, newPluginName);

            File.Move(pluginPath, pluginNewPath);
            File.Move(newPluginPath, newPluginNewPath);
        }
        #endregion
    }
}
