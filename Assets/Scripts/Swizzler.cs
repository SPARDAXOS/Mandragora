using UnityEngine;

public static class Swizzler
{
    // Swizzling a Vector2
    public static Vector2 xy(this Vector2 input)
    {
        return input;
    }
    public static Vector2 yx(this Vector2 input)
    {
        return new Vector2(input.y, input.x);
    }

    // Swizzling a Vector3
    public static Vector3 xyz(this Vector3 input)
    {
        return input;
    }
    public static Vector3 yxz(this Vector3 input)
    {
        return new Vector3(input.y, input.x, input.z);
    }
    public static Vector3 yzx(this Vector3 input)
    {
        return new Vector3(input.y, input.z, input.x);
    }
    public static Vector3 xzy(this Vector3 input)
    {
        return new Vector3(input.x, input.z, input.y);
    }
    public static Vector3 zxy(this Vector3 input)
    {
        return new Vector3(input.z, input.x, input.y);
    }
    public static Vector3 zyx(this Vector3 input)
    {
        return new Vector3(input.z, input.y, input.x);
    }

    // Vector3 from swizzled Vector2
    public static Vector3 xyy(this Vector2 input)
    {
        return new Vector3(input.x, input.y, input.y);
    }
    public static Vector3 xyx(this Vector2 input)
    {
        return new Vector3(input.x, input.y, input.x);
    }
    public static Vector3 yxx(this Vector2 input)
    {
        return new Vector3(input.y, input.x, input.x);
    }
    public static Vector3 yyx(this Vector2 input)
    {
        return new Vector3(input.y, input.y, input.x);
    }
    public static Vector3 x0y(this Vector2 input)
    {
        return new Vector3(input.x, 0f, input.y);
    }
    public static Vector3 xy0(this Vector2 input)
    {
        return new Vector3(input.x, input.y, 0f);
    }

    public static Vector3 x0y(this Vector3 input)
    {
        return new Vector3(input.x, 0f, input.y);
    }
    public static Vector3 xy0(this Vector3 input)
    {
        return new Vector3(input.x, input.y, 0f);
    }

    // Vector2 from swizzled Vector3
    public static Vector2 xy(this Vector3 input)
    {
        return new Vector2(input.x, input.y);
    }
    public static Vector2 xz(this Vector3 input)
    {
        return new Vector2(input.x, input.z);
    }
}
