// Welcome to my spaghetti code! Please feel free to criticize or fix this because I didn't even bother to take a second look at the code.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Clouddns;
using CommandLine;

internal class Program
{
    public class Options
    {
        [Option('r', "rate", Required = false, Default = 10, HelpText = "Check rate. Expects time in seconds.")]
        public int CheckRate { get; set; }

        [Option('t', "token", Required = true, HelpText = "Cloudflare API token.")]
        public string? Token { get; set; }

        [Option('z', "zone", Required = true, HelpText = "Domain Zone ID.")]
        public string? ZoneId { get; set; }

        [Option('n', "name", Required = true, HelpText = "DNS record name.")]
        public string? RecordName { get; set; }
    }
    
    static HttpClient Client = new();
    static CfClient CfFrontend = new(Client);

    static string FetchIP()
    {
        try
        {
            using HttpResponseMessage Res = Client.Send(new HttpRequestMessage(HttpMethod.Get, "http://ident.me/"));
            using StreamReader Reader = new(Res.Content.ReadAsStream());
            return Reader.ReadToEnd();
        }
        catch (HttpRequestException)
        {
            Logs.Write($"FETCH interrupted.", LogType.Error);
            return string.Empty;
        }
        catch (IOException)
        {
            Logs.Write($"FETCH interrupted.", LogType.Error);
            return string.Empty;
        }
    }

    static bool SafeUpdateDnsRecord(DnsRecord Record)
    {
        try
        {
            CfFrontend.UpdateDnsRecord(Record);
            return true;
        }
        catch (CfException Ex)
        {
            Logs.Write(Ex.Message, LogType.Error);
        }
        catch (HttpRequestException)
        {
            Logs.Write($"UPDATE interrupted", LogType.Error);
        }
        catch (IOException)
        {
            Logs.Write($"UPDATE interrupted", LogType.Error);
        }
        return false;
    }

    static void MainEx(Options Opt)
    {
        // Setup
        Opt.CheckRate *= 1000;
        CfFrontend.Token = Opt.Token;

        // Scan DNS records for the correct name.
        Logs.Write("Scanning DNS records...", LogType.Info);
        DnsRecord? MyRecord = null; // There's probably a cleaner way to do this but it works.
        foreach (DnsRecord r in CfFrontend.GetDnsRecords(Opt.ZoneId))
        {
            if (r.name == Opt.RecordName)
            {
                MyRecord = r;
                break;
            }
        }

        if (MyRecord == null) // Just some error handling...
        {
            Logs.Write($"DNS record {Opt.RecordName} not found.", LogType.Error);
            Environment.Exit(-1);
        }

        string CurrentIP = FetchIP();
        Logs.Write($"START {CurrentIP}", LogType.Info);

        if (MyRecord.content != CurrentIP)
        {
            Logs.Write($"UPDATE {MyRecord.content} -> {CurrentIP}", LogType.Info);
            MyRecord.content = CurrentIP;
            CfFrontend.UpdateDnsRecord(MyRecord); // Probably wont ever error.
        }

        string NewIP;
        do
        {
            Thread.Sleep(Opt.CheckRate);
            NewIP = FetchIP();
            if (!string.IsNullOrEmpty(NewIP) && (CurrentIP != NewIP))
            {
                Logs.Write($"UPDATE {CurrentIP} -> {NewIP}", LogType.Info);
                MyRecord.content = NewIP;
                if (SafeUpdateDnsRecord(MyRecord))
                    CurrentIP = NewIP;
            }
        } while (true);
    }

    static void Main(string[] Args) =>
        Parser.Default.ParseArguments<Options>(Args).WithParsed(MainEx);
}