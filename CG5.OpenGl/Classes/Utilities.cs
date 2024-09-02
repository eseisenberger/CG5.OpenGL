using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace CG5.OpenGL.Classes;

public static class Utilities
{
    public static Vector2 PolarToCartesian(Vector2 center, float radius, float angle)
    {
        var x = center.X + radius * MathF.Cos(angle);
        var y = center.Y + radius * MathF.Sin(angle);
        
        return new Vector2(x, y);
    }
    
    public static Vector3 CylindricalToCartesian(Vector3 cylindrical)
    {
        var r = cylindrical.X;
        var theta = cylindrical.Z;
        var y = cylindrical.Y;
        
        var x = r * MathF.Cos(theta);
        var z = r * MathF.Sin(theta);

        return new Vector3(x, y, z);
    }
    
    public static float DegToRad(float degrees) => (float)(Math.PI * degrees / 100f);
}