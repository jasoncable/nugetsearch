using System;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace NuGetSearch.Import
{
    public class JsonNetSerializer : IDeserializer, ISerializer
    {
        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
        public string ContentType { get; set; } = "application/json";

        public T Deserialize<T>(IRestResponse response)
        {
            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private JsonSerializerSettings CreateSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();

            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.TraceWriter = new TraceWriter();

            return settings;
        }
    }

    public class TraceWriter : ITraceWriter
    {
        public TraceLevel LevelFilter => TraceLevel.Verbose;

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            Console.WriteLine(message);
            if(ex != null)
                Console.WriteLine(ex.ToString());

        }
    }
}
