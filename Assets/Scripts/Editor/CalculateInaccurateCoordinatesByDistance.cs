using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <remarks>WARNING!<br/>
/// This method uses <see cref="float"/> instead of <see cref="double"/>, which can decrease accuracy remarkly.
/// </remarks>
/// <summary>
/// Go to <see href="http://www.movable-type.co.uk/scripts/latlong.html"/> to compare coordinates on a map.
/// </summary>
public class CalculateInaccurateCoordinatesByDistance : EditorWindow
{
    private static readonly Vector2Int size = new Vector2Int(250, 200);
    private readonly float r_earth = 6378.137f;

    private float latitude;
    private float longitude;
    private float dx;
    private float dy;

    [MenuItem("Extensions/Calculate Inaccurate Coordinates By Distance")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow<CalculateInaccurateCoordinatesByDistance>();
        window.minSize = size;
        window.maxSize = size;
    }

    private void OnGUI()
    {
        latitude = EditorGUILayout.FloatField("latitude", latitude);
        longitude = EditorGUILayout.FloatField("longitude", longitude);
        dx = EditorGUILayout.FloatField("Metres in x", dx);
        dy = EditorGUILayout.FloatField("Metres in y", dy);
        float dxReal = dx / 1000;
        float dyReal = dy / 1000;

        var pi = Mathf.PI;

        EditorGUILayout.LabelField("New Latitude");
        EditorGUILayout.FloatField(latitude + (dyReal / r_earth) * (180 / pi));
        EditorGUILayout.LabelField("New Longitude");
        EditorGUILayout.FloatField(longitude + (dxReal / r_earth) * (180 / pi) / Mathf.Cos(latitude * pi / 180));
    }
}