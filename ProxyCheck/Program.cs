using System.Net.Sockets;


class Program
{
    static async Task Main()
    {
        string fileName = "proxy-list.txt";
        List<string> proxyList = await LoadProxyListFromFile(fileName);
        // replace the above filename with the name of your proxy list file

        Console.WriteLine($"Loaded {proxyList.Count} proxies from {fileName}");

        List<Task<(string, bool)>> tasks = new List<Task<(string, bool)>>();
        foreach (var proxy in proxyList)
        {
            tasks.Add(Task.Run(async () => (proxy, await IsProxyWorking(proxy))));
        }

        await Task.WhenAll(tasks);

        List<string> workingProxies = new List<string>();
        foreach (var task in tasks)
        {
            (string proxy, bool result) = task.Result;
            if (result)
            {
                Console.WriteLine($"Proxy {proxy} is working");
                workingProxies.Add(proxy);
            }
            else
            {
                Console.WriteLine($"Proxy {proxy} is not working");
            }
        }

        string outputFileName = "working-proxy-list.txt";
        await WriteWorkingProxiesToFile(outputFileName, workingProxies);
        // replace the above filename with the name of your output file

        Console.WriteLine($"Wrote {workingProxies.Count} working proxies to {outputFileName}");
    }

    static async Task<List<string>> LoadProxyListFromFile(string fileName)
    {
        List<string> proxyList = new List<string>();
        using (StreamReader reader = new StreamReader(fileName))
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                proxyList.Add(line);
            }
        }
        return proxyList;
    }

    static async Task<bool> IsProxyWorking(string proxy)
    {
        try
        {
            var client = new TcpClient();
            var proxyUri = new Uri($"http://{proxy}");
            await client.ConnectAsync(proxyUri.Host, proxyUri.Port);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    static async Task WriteWorkingProxiesToFile(string fileName, List<string> workingProxies)
    {
        using (StreamWriter writer = new StreamWriter(fileName))
        {
            foreach (var proxy in workingProxies)
            {
                await writer.WriteLineAsync(proxy);
            }
        }
    }
}
