using UnityEngine;
using signals;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class ApplicationMonitor : MonoBehaviour
{
	public Signal applicationQuit = new Signal();
	public Signal<bool> applicationPause = new Signal<bool>();
	public Signal<bool> applicationFocus = new Signal<bool>();

	void OnApplicationQuit()
	{
		applicationQuit.Dispatch();
	}

	void OnApplicationPause(bool paused)
	{
		applicationPause.Dispatch(paused);
	}

	void OnApplicationFocus(bool focused)
	{
		applicationFocus.Dispatch(focused);
	}

	public async Task<bool> InternetReachable()
    {
		UnityWebRequest getRequest = UnityWebRequest.Get("www.google.com");
		await getRequest.SendWebRequest();
		return getRequest.result == UnityWebRequest.Result.Success;
	}
}