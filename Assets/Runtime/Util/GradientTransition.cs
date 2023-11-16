using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientTransition : MonoBehaviour
{
    private float duration = 1.0f;
    public SpriteRenderer sRender;
    // Update is called once per frame
    Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }


    void Update()
    {
        float lerp = Mathf.PingPong(Time.time, duration) / duration;
        sRender.material.SetColor("_Color", Color.Lerp(HexToColor("fe805e"), HexToColor("fa8856"), lerp)); //bottom
        sRender.material.SetColor("_Color2", Color.Lerp(HexToColor("527fc1"), HexToColor("7ae0ec"), lerp)); //top
    }
}
