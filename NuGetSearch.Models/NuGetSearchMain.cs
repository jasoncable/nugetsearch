using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace NuGetSearch.Models
{
    public class NuGetSearchMain
    {
        [JsonIgnore]
        private static readonly Regex _splitCamelCase =
            new Regex("((?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z]))",
                      RegexOptions.Compiled);

        [JsonIgnore]
        private static readonly Regex _multipleWhitespace =
            new Regex("\\s+", RegexOptions.Compiled);

        [JsonIgnore]
        private static readonly Regex _splitName =
            new Regex("([^a-zA-Z0-9]+)", RegexOptions.Compiled);

        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<string> Authors { get; set; }
        public List<string> Versions { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string ProjectUrl { get; set; }
        public string ReleaseNotes { get; set; }
        public string Summary { get; set; }
        public List<string> Tags { get; set; }
        public string Title { get; set; }
        public DateTime CommitTimeStamp { get; set; }
        public List<NuGetSearchMainDependencyGroup> DependencyGroups { get; set; }

        public string NameSplitCamel 
        {
            get 
            {
                return _multipleWhitespace.Replace( _splitCamelCase.Replace(
                    Name.Replace("-", " ").Replace(".", " ").Replace("_", " "), " $1"), " ");
                //Regex.Replace("TheCapitalOfTheUAEIsAbuDhabi", "((?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z]))", " $1").Trim();
            }

            set
            {
            }
        }

        public string NameSplit
        {
            get
            {
                return _multipleWhitespace.Replace(_splitName.Replace(Name, " "), " ");
            }
            set
            {

            }
        }
    }
}