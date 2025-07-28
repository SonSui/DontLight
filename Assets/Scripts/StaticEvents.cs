using System.Net.Sockets;
using System.Net;
using UnityEngine;

public class StaticEvents : MonoBehaviour
{
    public static string playerStat;
    public static string hostIP;
    public static bool Dissolution;

    public static string GetLocalIPAddress()
    {
        foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        return "Unknown";
    }
}
