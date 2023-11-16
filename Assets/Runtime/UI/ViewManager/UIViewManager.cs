using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;

/// <summary>
/// UIViewManager
///
/// Handles the instantiation of views through the Addressables system.
/// Will manage the lifecycle of views (screens or dialogs) including back button handling via 'PopViewFromStack'.
/// </summary>
public class UIViewManager : IDisposable
{
	public event Action<string> ViewLoadError;
	public event Action<IUIView> ViewOpened;
	public event Action<IUIView> ViewClosed;
	public IUICanvas UiCanvas { get => _uiCanvas; }

	private const string UI_CANVAS_KEY = "MainCanvas";
	private IUICanvas _uiCanvas;
	private LinkedList<string> _viewStack;
	private Dictionary<string, IUIView> _viewCache;
	private Dictionary<IUIView, List<GameObject>> _viewChildAssets;
	
	private readonly IObjectResolver _dependencyInjectionContainer;
	
	public UIViewManager(IObjectResolver container)
    {
		_dependencyInjectionContainer = container;
    }

	public async Task Init(IUICanvas uiCanvas = null)
    {
	    if (uiCanvas == null)
	    {
		    var handle = UnityEngine.AddressableAssets.Addressables.InstantiateAsync(UI_CANVAS_KEY);
		    await Task.Run(() => handle.Task);

		    if (handle.Status == AsyncOperationStatus.Failed)
		    {
			    Debug.LogError($"No {UI_CANVAS_KEY} found");
		    }
		    else
		    {
			    _uiCanvas = handle.Result.GetComponent<IUICanvas>();
		    }
	    }
	    else
	    {
		    _uiCanvas = uiCanvas;
	    }

		_viewStack = new LinkedList<string>();
		_viewCache = new Dictionary<string, IUIView>();
		_viewChildAssets = new Dictionary<IUIView, List<GameObject>>();
    }

	/// <summary>
	/// Get a cached view.
	/// <param name="assetName">Can be used to override the default asset name that is derived from Type name.</param>
	/// </summary>
	public IUIView GetCachedView(string assetName)
	{
		if (_viewCache.ContainsKey(assetName))
		{
			return _viewCache[assetName];
		}

		return null;
	}

	public T GetCachedView<T>()
	{
		return (T)GetCachedView(typeof(T).Name);
	}

	public T GetViewInHierarchy<T>(string assetName = null)
	{
		IUIView[] views = _uiCanvas.BaseContainer.GetComponentsInChildren<IUIView>(true);
		foreach (IUIView view in views)
		{
			if (view.GetType() == typeof(T) && (string.IsNullOrEmpty(assetName) || assetName == GetViewName(view)))
			{
				return (T)view;
			}
		}

		return default;
	}
	
	/// <summary>
	/// Show a view. Will instantiate and parent to the correct canvas transform. 'UIView.ifFullScreen=true' can be used for
	///    'fullscreen' Screens and 'UIView.ifFullScreen=false' will treat the view as a dialog.
	/// <param name="assetName">Asset name of view, will be used to lookup the Addressable asset.</param>
	/// <param name="animate">(optional, default 'true') Animate the view opening.</param>
	/// <param name="cache">(optional, default 'true') Should the view stay cached or destroyed when closed.</param>
	/// </summary>
	public async Task<IUIView> ShowViewAsync(string assetName, bool animate = true, bool cache = true)
	{
		Debug.Log($"UIViewManager::ShowViewAsync : Showing view {assetName}");
		
		if (string.IsNullOrEmpty(assetName))
		{
			Debug.LogError("UIViewManager::ShowViewAsync: Cannot show view, null or empty assetName.");
			return null;
		}
		
		IUIView newView = null;
		if (cache)
        {
			newView = GetCachedView(assetName);
		}

		if (newView == null)
		{
			AsyncOperationHandle<GameObject> handle = UnityEngine.AddressableAssets.Addressables.InstantiateAsync(assetName);
			GameObject viewGameObject = await LoadAssetAsync(handle);
			newView = viewGameObject.GetComponent<IUIView>();

			if (newView == null)
			{
				Debug.LogError($"UIViewManager :: View '{assetName}' does not have a component that implements IUIView so will not be added to the canvas.");
				UnityEngine.AddressableAssets.Addressables.ReleaseInstance(handle);
				return null;
			}

			Transform layer = _uiCanvas.DialogLayer;
			if (newView.IsFullScreen)
			{
				layer = _uiCanvas.ScreenLayer;
			}
			viewGameObject.transform.SetParent(layer, false);

			if (cache)
			{
				newView.OnClosed += HandleViewClosed;
				_viewCache[assetName] = newView;
			}
			else
			{
				newView.OnClosed += HandleTemporaryViewClosed;
			}
		}

		ShowView(newView, animate, cache);

		return newView;
	}
	
	public async Task<T> ShowViewAsync<T>(bool animate = true, bool cache = true, string assetName = null)
	{
		if (assetName == null)
		{
			assetName = typeof(T).Name;
		}

		return (T)await ShowViewAsync(assetName, animate, cache);
	}

	/// <summary>
	/// Show an already instantiated view. Will hide existing views as needed and move the view to the top of the stack.
	/// <param name="view">Instantiated view to show.</param>
	/// <param name="animate">(optional, default 'true') Animate the view hiding.</param>
	/// <param name="addToStack">(optional, default 'true') Add this to the stack for back navigation.</param>
	/// </summary>
	public void ShowView(IUIView view, bool animate = true, bool addToStack = true)
	{
		// If the new view is full screen, hide all the views beneath it.
		if (view.IsFullScreen)
		{
			HideAllViews(_uiCanvas.DialogLayer);
			HideAllViews(_uiCanvas.ScreenLayer);
		}
		GameObject viewGameObject = (view as MonoBehaviour).gameObject;
		viewGameObject.transform.SetAsLastSibling();

		// Only one copy of a view should be at the top of the stack at a time.
		if (addToStack)
		{
			string assetName = GetViewName(view);
			if (_viewStack.First?.Value != assetName)
			{
				_viewStack.AddFirst(assetName);
			}
		}

		view.Show(animate);
	}
	
	/// <summary>
	/// Hide a cached view.
	/// <param name="assetName">Asset name of view, will be used to lookup the asset from the cache.</param>
	/// <param name="animate">(optional, default 'true') Animate the view hiding.</param>
	/// </summary>
	public void HideView(string assetName, bool animate = true)
	{
		IUIView uiView = GetCachedView(assetName);

		if (uiView != null)
		{
			uiView.Hide(animate);
		}
	}

	public void HideView<T>(bool animate = true, string assetName = null)
    {
		if (assetName == null)
		{
			assetName = typeof(T).Name;
		}

		HideView(assetName, animate);
	}

	/// <summary>
	/// Show a non-screen/non-dialog view that needs dependency injection. Will not be added to view stack or
    ///    added to one of the standard layer transforms.
    /// SHOULD ONLY BE USED if the object is self contained, not requiring a Setup call
	/// <param name="assetName">Asset name.</param>
	/// <param name="parentView"> The parent view associated with this asset. If parent is destroyed, this asset will be as well.</param>
	/// <param name="container"> (optional) Transform we should load into. If left 'null' will NOT be added to any transform.</param>
	/// <param name="cancellationToken">(optional) Token to cancel the load.</param>
	/// </summary>
	public async Task<GameObject> InstantiateAndInjectAsset(string assetName, Transform container = null, IUIView parentView = null, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrEmpty(assetName))
        {
			Debug.LogError("UIViewManager::InstantiateAndInjectAsset: Cannot instantiate, null or empty assetName.");
			return null;
        }
		
		AsyncOperationHandle<GameObject> handle = UnityEngine.AddressableAssets.Addressables.InstantiateAsync(assetName, container);
		GameObject viewGameObject = await LoadAssetAsync(handle, cancellationToken);

		if (parentView != null)
		{
			if (!_viewChildAssets.ContainsKey(parentView))
			{
				_viewChildAssets[parentView] = new List<GameObject>();
			}
			_viewChildAssets[parentView].Add(viewGameObject);
		}
		
		return viewGameObject;
	}

	/// <summary>
	/// Show a non-screen/non-dialog view that needs dependency injection. Will not be added to view stack to
	///    added to one of the standard layer transforms.
	/// SHOULD ONLY BE USED if the object is self contained, not requiring a Setup call
	/// </summary>
	/// <param name="container"></param>
	/// <param name="parentView"></param>
	/// <param name="assetName"></param>
	/// <param name="cancellationToken"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public async Task<T> InstantiateAndInjectAsset<T>(Transform container = null, IUIView parentView = null, string assetName = null, CancellationToken cancellationToken = default)
	{
		if (assetName == null)
		{
			assetName = typeof(T).Name;
		}

		GameObject viewGameObject = await InstantiateAndInjectAsset(assetName, container, parentView, cancellationToken); ;

		return viewGameObject.GetComponent<T>();
	}

	/// <summary>
	/// Pops the topmost screen or dialog view from the stack in the order it was added.
	///    If the next view down the stack is 'UIView.IsFullscreen=true' only that view will be shown.
	///    If the next view down the stack is 'UIView.IsFullscreen=false' that view will be shown and it will continue
    ///       going down the stack showing views until it hits the next fullscreen view where it will stop.  This will allow
    ///       all of the dialogs a screen opened to be shown when it is gone back to.
	/// <param name="animate">(optional, default 'true') Animate the opening and closing of views.</param>
	/// </summary>
	public void PopViewFromStack(bool animate = true)
    {
		if (_viewStack.Count < 2)
        {
			return;
        }

		IUIView viewToHide = GetCachedView(_viewStack.First.Value);
		viewToHide.Hide(animate);
		_viewStack.RemoveFirst();

		// If the current view is full screen, reactivate all the views lower in the stack until we hit another fullscreen view.
		//   This allows the next fullscreen view plus all its 'child' views to be visible again.
		if (viewToHide.IsFullScreen)
		{
			foreach (string uiViewAssetName in _viewStack)
			{
				IUIView uiView = GetCachedView(uiViewAssetName);
				uiView.Show(animate);
				if (uiView.IsFullScreen)
				{
					break;
				}
			}
		}
		else
        {
			IUIView uiView = GetCachedView(_viewStack.First.Value);
			uiView.Show(animate);
		}
	}

	/// <summary>
	/// Clear the view stack (history of views opened.)
	/// </summary>
	public void ClearViewStack()
    {
		_viewStack.Clear();
    }

	/// <summary>
	/// Removes a specific view from the view stack.
	/// <param name="assetName">Asset name.</param>
	/// </summary>
	public void RemoveViewFromStack(string assetName)
    {
		_viewStack.Remove(assetName);
		// Make sure there are no successive duplicate views in the stack after removing.
		_viewStack = RemoveSuccessiveDuplicates(_viewStack);
	}

	public void RemoveViewFromStack<T>()
	{
		RemoveViewFromStack(typeof(T).Name);
	}

	/// <summary>
	/// Add a custom view to the stack. This allows for back button support and caching of custom views or non-MonoBehaviours that implement IUIView.
	/// <param name="view"> An instance of a view to add to the view stack and Show. If the IUIView.IsFullscreen=true it will hide other views.</param>
	/// <param name="animate"> Whether to animate the views Show transition.</param>
	/// </summary>
	public T AddViewToStack<T>(T view = default, bool animate = true) where T : IUIView
	{
		string viewName = typeof(T).Name;

		// Check if an instance has been passed in and if not, instantiate the view (only works for non-MonoBehaviours, otherwise an instance
        //     MUST be passed in to be added to the view stack.
		if (EqualityComparer<T>.Default.Equals(view, default))
		{
			if (view is MonoBehaviour)
            {
				Debug.LogError("UIViewManager :: AddViewToStack cannot instantiate a MonoBehaviour, an instance must be passed in.");
				return default(T);
            }

			IUIView newView = GetCachedView<T>();

			if (newView == null)
            {
				view = (T)Activator.CreateInstance(typeof(T));
				InjectDependencies(view);
				view.Init();
			}
			else
            {
				view = (T)newView;
			}
		}

		if (view.IsFullScreen)
		{
			HideAllViews(_uiCanvas.DialogLayer);
			HideAllViews(_uiCanvas.ScreenLayer);
		}

		view.Show(animate);

		_viewCache[viewName] = view;

		if (_viewStack.First?.Value != viewName)
		{
			_viewStack.AddFirst(viewName);
		}

		return view;
	}

	public IUIView AddViewToStack(IUIView view, bool animate = true)
	{
		return AddViewToStack<IUIView>(view, animate);
	}

	/// <summary>
	/// Destroy all views on the dialog layer.
	/// </summary>
	public void DestroyAllDialogs()
    {
		HideAllViews(_uiCanvas.DialogLayer, true);
    }

	/// <summary>
	/// Hide all views.
	/// <param name="layer">(optional, defaults to 'all' layers) A transform indicating the layer to hide views on.</param>
	/// <param name="destroy">(optional, defaults to 'false') Should the view be destroyed or left in the cache?</param>
	/// </summary>
	public void HideAllViews(Transform layer = null, bool destroy = false)
	{
		if (layer == null)
		{
			if (_uiCanvas != null && _uiCanvas.BaseContainer != null)
			{
				layer = _uiCanvas.BaseContainer.transform;
			}
			else
			{
				return;
			}
		}

		IUIView[] views = layer.GetComponentsInChildren<IUIView>(true);
		foreach (IUIView view in views)
		{
			if (destroy)
            {
				DisposeView(view);
			}
			else
            {
				view.Hide(false);
            }
		}
	}

	/// <summary>
	/// Destroy a specific view based on type T.
	/// </summary>
	public void DisposeView<T>()
	{
		IUIView cachedView = GetCachedView<T>() as IUIView;
		if (cachedView != null)
        {
			DisposeView(cachedView);
			return;
		}

		IUIView[] views = _uiCanvas.BaseContainer.GetComponentsInChildren<IUIView>(true);
		foreach (IUIView view in views)
		{
			if (view.GetType() == typeof(T))
            {
				DisposeView(view);
			}
		}
	}

	/// <summary>
	/// Cleans up a specific view. It will either be released if an addressable or destroyed if not.
	/// </summary>
	public void DisposeView(IUIView view)
    {
		view.OnClosed -= HandleViewClosed;

		string assetName = view.GetType().Name;
		
		RemoveViewFromStack(assetName);
		if (_viewCache.ContainsKey(assetName))
		{
			_viewCache.Remove(assetName);
		}
		
		if (view is MonoBehaviour behaviour)
		{
			if (_viewChildAssets.ContainsKey(view))
			{
				foreach (GameObject childAsset in _viewChildAssets[view])
				{
					DisposeGameObject(childAsset);
				}
				_viewChildAssets.Remove(view);
			}
			DisposeGameObject(behaviour.gameObject);
		}
    }

	/// <summary>
	/// Check if any views are active on the dialog layer.
	/// </summary>
	public bool IsDialogActive()
	{
		for (int n = 0; n < _uiCanvas.DialogLayer.childCount; n++)
		{
			Transform transform = _uiCanvas.ScreenLayer.GetChild(n);
			if (transform.gameObject.activeInHierarchy)
            {
				return true;
            }
		}

		return false;
	}

	private void DisposeGameObject(GameObject gameObject)
	{
		// If a GameObject is loaded via Addressables, ReleaseInstance returns 'true' and will destroy it and decrement the reference count.
		//   If it returns 'false' it needs to be Destroyed manually.
		if (!UnityEngine.AddressableAssets.Addressables.ReleaseInstance(gameObject))
		{
			Debug.LogWarning($"[UIViewManager] DisposeGameObject is being called on a non-addressable asset. {gameObject.name}.");
			GameObject.Destroy(gameObject);
		}
	}
	
	private async Task<GameObject> LoadAssetAsync(AsyncOperationHandle<GameObject> handle, CancellationToken cancellationToken = default)
	{
		// It is possible this task is already done due to the asset being loaded previously. If so, do not attempt to run it again as
		//   it causes an assert.
		if (handle.Task.Status != TaskStatus.RanToCompletion)
		{
			try
			{
				await Task.Run(() => handle.Task, cancellationToken);
			}
			catch (Exception e)
			{
				Debug.LogError($"[UIViewManager] LoadAssetAsync : Exception while instantiating view '{e.Message}'");
				return null;
			}
		}

		if (handle.Status == AsyncOperationStatus.Failed || handle.Result == null)
		{
			Debug.LogError($"[UIViewManager] Failed to instantiate asset {handle.DebugName}.");
			ViewLoadError?.Invoke($"[UIViewManager] Failed to instantiate asset {handle.DebugName}.");
			return null;
		}

		if (cancellationToken.IsCancellationRequested)
		{
			UnityEngine.AddressableAssets.Addressables.ReleaseInstance(handle);
			return null;
		}

		InjectDependencies(handle.Result);

		IUIView viewComponent = handle.Result.GetComponent<IUIView>();
		if (viewComponent != null)
		{
			try
			{
				viewComponent.Init();
			}
			catch (Exception e)
			{
				Debug.LogError($"[UIViewManager] LoadAssetAsync : Exception while initializing view '{e.Message}'");
				return null;
			}
			ViewOpened?.Invoke(viewComponent);
		}

		return handle.Result;
	}

	private void InjectDependencies<T>(T target)
	{
		if (_dependencyInjectionContainer != null)
		{
			try
			{
				if (target is GameObject gameObject)
				{
					_dependencyInjectionContainer.InjectGameObject(gameObject);
				}
				else
				{
					_dependencyInjectionContainer.Inject(target);
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"[UIViewManager] InjectDependencies : Exception while injecting dependencies '{e.Message}'");
			}
		}
		else
		{
			Debug.LogError("[UIViewManager] InjectDependencies : Container not available for injection.");
		}
	}

	private void HandleViewClosed(IUIView view)
    {
		ViewClosed?.Invoke(view);
		view.OnClosed -= HandleViewClosed;
	}

	private void HandleTemporaryViewClosed(IUIView view)
	{
		ViewClosed?.Invoke(view);
		view.OnClosed -= HandleTemporaryViewClosed;
		DisposeView(view);
	}

	private LinkedList<string> RemoveSuccessiveDuplicates(LinkedList<string> list)
    {
		LinkedList<string> results = new LinkedList<string>();
		foreach (string element in list)
		{
			if (results.Count == 0 || results.Last.Value != element)
				results.AddLast(element);
		}
		return results;
	}

    /// <summary>
    /// Implementation of disposable pattern
    /// https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
    /// </summary>
    private bool _disposed = false;

    ~UIViewManager() => Dispose(false);

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing && _uiCanvas != null)
			{
				HideAllViews();
				
				MonoBehaviour canvasComponent = _uiCanvas as MonoBehaviour;
				if (canvasComponent != null && canvasComponent.gameObject != null)
				{
					DisposeGameObject(canvasComponent.gameObject);
				}
				_uiCanvas = null;
			}

			// unmanaged resources and fields that should be nulled
			if (_viewStack != null)
            {
				_viewStack.Clear();
				_viewStack = null;
			}

			if (_viewCache != null)
            {
				_viewCache.Clear();
				_viewCache = null;

			}

			ViewLoadError = null;
			ViewOpened = null;
			ViewClosed = null;

			_disposed = true;
		}
	}

	private string GetViewName(IUIView view)
	{
		string viewName = null;
		
		MonoBehaviour comp = view as MonoBehaviour;
		if (comp != null && comp.gameObject != null)
		{
			string removeString = "(Clone)";
			string sourceString = comp.gameObject.name;
			int index = sourceString.IndexOf(removeString, StringComparison.Ordinal);
			viewName = index < 0 ? sourceString : sourceString.Remove(index, removeString.Length);
		}

		return viewName;
	}
}

public class UICanvasWrapper : IUICanvas
{
	public RectTransform ScreenLayer { get; set; }
	public RectTransform DialogLayer { get; set; }
	public Transform BaseContainer { get; set;}
}
