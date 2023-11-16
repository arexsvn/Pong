using System;
using UnityEngine;

public static class NetworkUtils
{
	public const string OFFLINE_ERROR_MESSAGE = "No Internet Connection";
    public static bool online
    {
        get
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
}