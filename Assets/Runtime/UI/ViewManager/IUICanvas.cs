using UnityEngine;

public interface IUICanvas
{
    RectTransform ScreenLayer { get; }
    RectTransform DialogLayer { get; }
    Transform BaseContainer { get; }
}