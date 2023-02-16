using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using URandom = UnityEngine.Random;

public static class ExtensionMethods
{
    #region LocationInfo

    /// <summary>
    /// Compares all properties of <see cref="LocationInfo"/>.
    /// </summary>
    /// <param name="other">The <see cref="LocationInfo"/> to compare with.</param>
    /// <returns>Returns true if locations are identical.</returns>
    public static bool CompareLocationInfo(this LocationInfo current, LocationInfo other)
    {
        if ((current.altitude == other.altitude) && (current.latitude == other.latitude) && (current.longitude == other.longitude)
            && (current.horizontalAccuracy == other.horizontalAccuracy) && (current.verticalAccuracy == other.verticalAccuracy)
            && (current.timestamp == other.timestamp))
            return true;

        return false;
    }

    /// <summary>
    /// Calculates the distance of 2 real world coordinates.
    /// </summary>
    /// <returns>The distance in metres</returns>
    public static float CalculateDistance(this LocationInfo info, LocationInfo other)
    {
        return CalculateDistance(info, other.latitude, other.longitude);
    }

    /// <summary>
    /// Calculates the distance of 2 real world coordinates.
    /// </summary>
    /// <returns>The distance in metres</returns>
    public static float CalculateDistance(this LocationInfo info, float latitude, float longitude)
    {
        float R = 6371000; // metres
        float omega1 = ((info.latitude / 180) * Mathf.PI);
        float omega2 = ((latitude / 180) * Mathf.PI);
        float variacionomega1 = (((latitude - info.latitude) / 180) * Mathf.PI);
        float variacionomega2 = (((longitude - info.longitude) / 180) * Mathf.PI);
        float a = Mathf.Sin(variacionomega1 / 2) * Mathf.Sin(variacionomega1 / 2) +
                    Mathf.Cos(omega1) * Mathf.Cos(omega2) *
                    Mathf.Sin(variacionomega2 / 2) * Mathf.Sin(variacionomega2 / 2);
        float c = 2 * Mathf.Asin(Mathf.Sqrt(a));

        float d = R * c;
        return d;
    }

    public static bool IsZero(this LocationInfo info)
    {
        Debug.LogWarning($"IsZero: {info.latitude}, {info.longitude}");
        return info.latitude == 0 && info.longitude == 0; // returns true if both values are 0.
    }

    #endregion

    #region Vector

    /// <summary>
    /// Returns a random point on a circle around current vector.
    /// </summary>
    /// <param name="point">The origin point</param>
    /// <param name="distance">Desired distance from origin point</param>
    /// <returns></returns>
    public static Vector2 GetRandomPointOnCircle(this Vector2 point, float distance)
    {
        var randomPointOnCircle = URandom.insideUnitCircle.normalized;
        int counter = 10000;

        while(randomPointOnCircle == Vector2.zero && counter > 0)
        {
            randomPointOnCircle = URandom.insideUnitCircle.normalized;
        }

        //P=(pos)+(normalized direction)*distance
        Vector2 newPoint = point + (randomPointOnCircle * distance);
        return newPoint;
    }

    /// <summary>
    /// Returns a random point on a circle around current vector's y axis.
    /// </summary>
    /// <param name="point">The origin point</param>
    /// <param name="distance">Desired distance from origin point</param>
    /// <returns></returns>
    public static Vector3 GetRandomPointOnHorizontalCircle(this Vector3 point, float distance)
    {
        var randomPointOnCircle = URandom.onUnitSphere * distance;
        Vector3 randomPoint = point + randomPointOnCircle;
        Vector3 newPoint = new Vector3(randomPoint.x, point.y, randomPoint.z);

        return newPoint;
    }

    #endregion

    #region Enums

    /// <summary>
    /// Matches enum values with comparing enum.
    /// </summary>
    /// <param name="value">enum to match</param>
    /// <param name="values">enums to match with compared enum</param>
    /// <returns>true if all values match, otherwise false.</returns>
    public static bool EqualsAll<T>(this T value, params T[] values) where T : Enum
    {
        if (values.Any(s => s.GetType() != value.GetType()))
            Debug.LogWarning("WARNING! One or more of the provided Enums is not of the same type as the compared Enum!");

        bool result = values.All(s => EqualityComparer<T>.Default.Equals(s, value));
        return result;
    }

    /// <summary>
    /// Matches enum values with comparing enum.
    /// </summary>
    /// <param name="value">enum to match</param>
    /// <param name="values">enums to match with compared enum</param>
    /// <returns>true if any values match, otherwise false.</returns>
    public static bool EqualsAny<T>(this T value, params T[] values) where T : Enum
    {
        if (values.Any(s => s.GetType() != value.GetType()))
            Debug.LogWarning("WARNING! One or more of the provided Enums is not of the same type as the compared Enum!");

        bool result = values.Any(s => EqualityComparer<T>.Default.Equals(s, value));
        return result;
    }

    #endregion
}
