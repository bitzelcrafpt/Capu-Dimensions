#if EDITOR

#else

using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Capu_Dimensions;

public class LatestVersion
{
    public static bool NeedToUpdate()
    {
        string localVersion = "1.0.0";
        string githubVersion = GetGithubVersion();

        return localVersion != githubVersion;
        //return false;
    }

    private static string GetGithubVersion()
    {
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = client.GetAsync("https://raw.githubusercontent.com/bitzelcrafpt/Capu-Dimensions/master/VERSION").Result;
            response.EnsureSuccessStatusCode();

            using (Stream stream = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadLine();
            }
        }
    }
}
#endif