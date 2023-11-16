
public interface IUIView
{
    event System.Action<IUIView> OnReady;
    event System.Action<IUIView> OnClosed;
    void Show(bool animate = true);
    void Hide(bool animate = true);
    void Init();
    bool IsFullScreen { get; set; }
}
