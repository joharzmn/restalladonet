using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RESTAll.Data.Common;
using RESTAll.Data.Contracts;
using RESTAll.Data.Models;

namespace RESTAll.Data.Utilities
{
    public class ConfigProvider:IConfigProvider
    {
        private RestAllConnectionStringBuilder _Builder;
        public bool IsConfigured { set; get; }
        public ConfigProvider(RestAllConnectionStringBuilder cb)
        {
            _Builder = cb;
            LoadConfig();
        }
        public ConfigMaps Config { set; get; }

        private void LoadConfig()
        {
            var filePath = $"{_Builder.Profile}/Config/mappings.json";
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);
                Config = JsonConvert.DeserializeObject<ConfigMaps>(fileText);
                IsConfigured = true;
            }
        }
    }
}
