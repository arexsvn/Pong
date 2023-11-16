using signals;
using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

public class RemoteAssetLoader
{
    readonly CoroutineRunner _coroutineRunner;
	readonly RemoteAssetVO _remoteAssetVO;
	readonly AssetsManager _assetsManager;
	private const string LOCAL_FILE_PREFIX = "file://";
	public const string ASSETS_FOLDER = "assets";

	public RemoteAssetLoader(CoroutineRunner coroutineRunner, RemoteAssetVO remoteAssetVO, AssetsManager assetsManager)
	{
		_coroutineRunner = coroutineRunner;
		_remoteAssetVO = remoteAssetVO;
		_assetsManager = assetsManager;
	}
		
    public void init()
    {
        createFolderIfNonexistent(Application.persistentDataPath + "/" + ASSETS_FOLDER);
    }

	public bool assetExists(string url)
	{
		return _assetsManager.getUrl(url) != null;
	}

	public void sendRequest(RemoteAssetRequest bpRemoteAssetRequest)
	{
		string fullUrl = "";

		if (bpRemoteAssetRequest.assetId != null)
		{
			bpRemoteAssetRequest.baseUrl = _assetsManager.getUrl(bpRemoteAssetRequest.assetId);
			fullUrl = _remoteAssetVO.cdnurl + ASSETS_FOLDER + "/" + bpRemoteAssetRequest.baseUrl;
		}
		else if(bpRemoteAssetRequest.fullUrlOverride != null)
		{
			bpRemoteAssetRequest.baseUrl = getBaseUrl(bpRemoteAssetRequest.fullUrlOverride);
			fullUrl = bpRemoteAssetRequest.fullUrlOverride;
		}

		if (fullUrl == null || bpRemoteAssetRequest.baseUrl == null)
		{
			bpRemoteAssetRequest.error.Dispatch("Asset Path is Null.");

			#if UNITY_EDITOR
			throw new System.Exception("RemoteAssetLoader :: Asset Path is Null.");
			#endif
		}
		else
		{
			bpRemoteAssetRequest.fullUrl = fullUrl;
			_coroutineRunner.run(asyncRequest(bpRemoteAssetRequest));
		}
	}

	private void createFolderIfNonexistent(string path)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(path);

		if (!directoryInfo.Exists)
		{
			Directory.CreateDirectory(path);
		}
	}

	private IEnumerator asyncRequest(RemoteAssetRequest remoteAssetRequest)
	{
		string requestUrl = remoteAssetRequest.fullUrl;
		bool cacheHit = false;

		if (remoteAssetRequest.allowCaching)
		{
			string cacheUrl = getCachePath(remoteAssetRequest.baseUrl);

			if (File.Exists(cacheUrl) && new FileInfo(cacheUrl).Length > 0)
			{
				requestUrl = LOCAL_FILE_PREFIX + cacheUrl;
				cacheHit = true;
			}
		}

		remoteAssetRequest.request = UnityWebRequestTexture.GetTexture(requestUrl);//new UnityWebRequest(requestUrl);
		yield return remoteAssetRequest.request;

		if (!cacheHit && remoteAssetRequest.allowCaching)
		{
			cacheContent(remoteAssetRequest);
		}

		remoteAssetRequest.requestComplete();
	}

	private void cacheContent(RemoteAssetRequest remoteAssetRequest)
	{
		// If a previous version of this file exists, delete it.
		deleteExistingVersionOfFile(remoteAssetRequest.baseUrl);
		createFolderIfNonexistent(Application.persistentDataPath + "/" + ASSETS_FOLDER + getFolderPath(remoteAssetRequest.baseUrl));
		File.WriteAllBytes(getCachePath(remoteAssetRequest.baseUrl), DownloadHandlerTexture.GetContent(remoteAssetRequest.request).GetRawTextureData()/*remoteAssetRequest.request.bytes*/);
	}

	private string getCachePath(string url)
	{
		return Application.persistentDataPath + "/" + ASSETS_FOLDER + url;
	}
		
	private string getFolderPath(string url)
	{
		return url.Substring(0, url.LastIndexOf("/"));
	}

	private string getFileNameWithoutVersion(string url)
	{
		string fileNameWithVersion = url.Substring(url.LastIndexOf("/") + 1);
		string extension = fileNameWithVersion.Substring(fileNameWithVersion.LastIndexOf(".") + 1);

		return fileNameWithVersion.Substring(0, fileNameWithVersion.LastIndexOf("-")) + "." + extension;
	}
		
	private string getFileNameWithVersionWildcard(string fileName)
	{
		string extension = fileName.Substring(fileName.LastIndexOf(".") + 1);

		return fileName.Substring(0, fileName.LastIndexOf(".")) + "*." + extension;
	}

	private void deleteExistingVersionOfFile(string url)
	{
		string fileName = getFileNameWithoutVersion(url);
		string parentFolder = getFolderPath(url);
		DirectoryInfo directoryInfo = new DirectoryInfo(getCachePath(parentFolder));

		if (directoryInfo.Exists)
		{
			// get a list of all files in this folder with matching names.
			FileInfo[] fileInfos = directoryInfo.GetFiles(getFileNameWithVersionWildcard(fileName));

			foreach (FileInfo fileInfo in fileInfos)
			{
				File.Delete(fileInfo.FullName);
			}
		}
	}

	private string getFileNameWithoutExtension(string fileName)
	{
		return fileName.Substring(0, fileName.LastIndexOf("."));
	}

	private string getBaseUrl(string url)
	{
		if (url.IndexOf(_remoteAssetVO.cdnurl + ASSETS_FOLDER) > -1)
		{
			url = url.Substring((_remoteAssetVO.cdnurl + ASSETS_FOLDER).Length);
		}
		else if (url.IndexOf(_remoteAssetVO.cdnurl) > -1)
		{
			url = url.Substring(_remoteAssetVO.cdnurl.Length);
		}
			
		return url;
	}
}

public class RemoteAssetRequest
{
	public Signal<UnityWebRequest> complete;
	public Signal<string> error;
	public UnityWebRequest request;
	public string assetId;
	public string fullUrlOverride;
	public string fullUrl;
	public string baseUrl;
	public bool allowCaching = true;

	public RemoteAssetRequest()
	{
		this.complete = new Signal<UnityWebRequest>();
		this.error = new Signal<string>();
	}

	public void requestComplete()
	{
		this.complete.Dispatch(request);
	}
}

public class RemoteAssetVO
{
    public string cdnurl;
}