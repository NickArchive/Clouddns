using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Clouddns
{
#pragma warning disable CS8618 // I know for a fact that API responses will always return a non-null value.
    public class CfError
    {
        public int code;
        public string message;
    }
    
    public class CfResponse
    {
        public bool success;
        public JToken result;
        public CfError[] errors;
    }

    public class RecordMeta
    {
        public bool auto_added;
        public bool managed_by_apps;
        public bool managed_by_argo_tunnel;
        public string source;
    }

    public class DnsRecord
    {
        public string id;
        public string zone_id;
        public string zone_name;
        public string name;
        public string type;
        public string content;
        public bool proxiable;
        public bool proxied;
        public int ttl;
        public bool locked;
        public RecordMeta meta;
        public string created_on;
        public string modified_on;
    }
#pragma warning restore CS8618

    public class CfException : Exception
    {
        public CfException() : base() { }
        public CfException(string szMessage) : base(szMessage) { }
    }

    public class CfClient
    {
        public readonly HttpClient Client;
        public readonly string Endpoint = "https://api.cloudflare.com/client/v4/";
        public string? Token;

        public CfClient(HttpClient? ExtClient = null)
        {
            if (ExtClient == null)
                Client = new HttpClient();
            else
                Client = ExtClient;
        }

        string InternalRequest(string szPath, HttpMethod Method, object? Data = null)
        {
            HttpRequestMessage Req;
            if (Data == null) Req = new(Method, Endpoint + szPath);
            else Req = new(Method, Endpoint + szPath) { Content = new StringContent(JsonConvert.SerializeObject(Data), Encoding.UTF8) };

            Req.Headers.Add("Authorization", $"Bearer {Token}");

            using HttpResponseMessage Res = Client.Send(Req);
            using StreamReader Reader = new(Res.Content.ReadAsStream());
            return Reader.ReadToEnd();
        }

        // Linq
        public JObject Get(string szPath) =>
            JObject.Parse(InternalRequest(szPath, HttpMethod.Get));

        public JObject Post(string szPath, object? Data = null) =>
            JObject.Parse(InternalRequest(szPath, HttpMethod.Post, Data));

        public JObject Put(string szPath, object? Data = null) =>
            JObject.Parse(InternalRequest(szPath, HttpMethod.Put, Data));

        public JObject Patch(string szPath, object? Data = null) =>
            JObject.Parse(InternalRequest(szPath, HttpMethod.Patch, Data));

        // Class
        public T? Get<T>(string szPath) =>
            JsonConvert.DeserializeObject<T>(InternalRequest(szPath, HttpMethod.Get));

        public T? Post<T>(string szPath, object? Data = null) =>
            JsonConvert.DeserializeObject<T>(InternalRequest(szPath, HttpMethod.Post, Data));

        public T? Put<T>(string szPath, object? Data = null) =>
            JsonConvert.DeserializeObject<T>(InternalRequest(szPath, HttpMethod.Put, Data));

        public T? Patch<T>(string szPath, object? Data = null) =>
            JsonConvert.DeserializeObject<T>(InternalRequest(szPath, HttpMethod.Patch, Data));

        // Methods
        public DnsRecord[] GetDnsRecords(string szZoneId) // This is aids.
        {
            CfResponse? Res = Get<CfResponse>($"zones/{szZoneId}/dns_records");
            if (Res == null) throw new CfException("Failed to get DNS records: Server returned a malformed response.");
            if (!Res.success) throw new CfException($"Failed to get DNS records: {Res.errors[0].message}.");
            DnsRecord[]? RecordList = Res.result.ToObject<DnsRecord[]>();
            if (RecordList == null) throw new CfException("Failed to get DNS records: Server returned a malformed response.");
            return RecordList;
        }

        public void UpdateDnsRecord(DnsRecord Record)
        {
            CfResponse? Res = Put<CfResponse>($"zones/{Record.zone_id}/dns_records/{Record.id}", new Dictionary<string, dynamic>()
            {
                { "type", Record.type },
                { "name", Record.name },
                { "content", Record.content },
                { "ttl", Record.ttl },
                { "proxied", Record.proxied },
            });
            if (Res == null) throw new CfException("Failed to update DNS record: Server returned a malformed response.");
            if (!Res.success) throw new CfException($"Failed to update DNS record: {Res.errors[0].message}.");
        }
    }
}
