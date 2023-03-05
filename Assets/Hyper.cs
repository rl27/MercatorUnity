using System.Collections;
using System.Collections.Generic;
using static System.Math;
using UnityEngine;

public class Hyper
{
    // Evaluate on hyperboloid (negative of Minkowski quadratic form)
    public static double hypEval(Vector3d v)
    {
        return v[1] * v[1] - v[0] * v[0] - v[2] * v[2];
    }

    // Minkowski dot product (negative of Minkowski bilinear form)
    public static double minkDot(Vector3d a, Vector3d b)
    {
        // return (hypEval(a+b) - hypEval(a) - hypEval(b))/2
        return a[1] * b[1] - a[0] * b[0] - a[2] * b[2];
    }

    // Normalize to hyperboloid
    public static Vector3d hypNormalize(Vector3d v)
    {
        return v / Mathd.Sqrt(Mathd.Abs(hypEval(v)));
    }

    // Get direction (tangent at point a) of geodesic from a to b
    public static Vector3d getDir(Vector3d a, Vector3d b)
    {
        return hypNormalize(b - minkProjection(b, a));
    }

    // Hyperbolic midpoint of a and b
    public static Vector3d midpoint(Vector3d a, Vector3d b)
    {
        return hypNormalize((a + b) / 2.0);  // This "/ 2.0" isn't necessary
    }

    // Minkowski distance between a and b
    // https://en.wikipedia.org/wiki/Hyperboloid_model#Minkowski_quadratic_form
    public static double dist(Vector3d a, Vector3d b)
    {
        return Acosh(minkDot(a, b));
    }

    // Minkowski projection of a onto b
    public static Vector3d minkProjection(Vector3d a, Vector3d b)
    {
        return b * (minkDot(a, b) / minkDot(b, b)); // Can also do q(a,b) / q(b,b)
    }

    // Extend a through b to c, such that the midpoint of a and c is b.
    // https://en.wikipedia.org/wiki/Hyperboloid_model#Straight_lines
    public static Vector3d extend(Vector3d a, Vector3d b)
    {
        double w = dist(a, b);
        Vector3d comp = getDir(a, b);
        return a * Cosh(2 * w) + comp * Sinh(2 * w);
    }

    // Form line from a to b and return c, such that the dist from a to c is d.
    public static Vector3d line(Vector3d a, Vector3d b, double d)
    {
        Vector3d comp = getDir(a, b);
        return a * Cosh(d) + comp * Sinh(d);
    }

    // Travel d distance from a along dir.
    public static Vector3d lineDir(Vector3d a, Vector3d dir, double d)
    {
        return a * Cosh(d) + dir * Sinh(d);
    }

    // Given x and two points p and q, construct point y such that dist(x,p) = dist(y,q) and vice versa.
    // Essentially "reflects" across the line passing orthogonally through the midpoint of pq. Parallel?
    public static Vector3d symmetry(Vector3d x, Vector3d p, Vector3d q)
    {
        Vector3d u = (p - q) / Mathd.Sqrt(-hypEval(p - q));
        return x - 2 * (-minkDot(x, u)) * u;
    }

    // Get Poincare projection from hyperboloid through y=0 to (0,-1,0)
    public static Vector3d getPoincare(Vector3d v)
    {
        double y1 = v.y + 1;
        return new Vector3d(v.x / y1, 0, v.z / y1);
    }

    // Project from (0,-1,0) to hyperboloid through a point (a,b) within the unit circle.
    // Derivation: y^2 = 1 + x^2 + z^2, y = x/a - 1, y = z/c - 1, sub x and z in the first equation.
    public static Vector3d reversePoincare(double a, double b)
    {
        double d = Mathd.Pow(a, 2.0d) + Mathd.Pow(b, 2.0d);
        Debug.Assert(d < 1);
        double y = (1 + d) / (1 - d);
        return new Vector3d(a * (y + 1), y, b * (y + 1));
    }

    // Get Beltrami-Klein projection from hyperboloid through y=1 to (0,0,0)
    public static Vector3d getBeltrami(Vector3d v)
    {
        return new Vector3d(v.x / v.y, 0, v.z / v.y);
    }

    // Euclidean radius of circumscribed circle, given sides per polygon and polygons per vertex.
    public static double circleRadius(int n, int k)
    {
        // return Mathd.Sqrt((Mathd.Tan(Mathd.PI / 2 - Mathd.PI / k) - Mathd.Tan(Mathd.PI / n)) / (Mathd.Tan(Mathd.PI / 2 - Mathd.PI / k) + Mathd.Tan(Mathd.PI_PRECISE / n)));
        return Mathd.Sqrt((Mathd.Tan(Mathd.PI_PRECISE / 2 - Mathd.PI_PRECISE / k) - Mathd.Tan(Mathd.PI_PRECISE / n)) / (Mathd.Tan(Mathd.PI_PRECISE / 2 - Mathd.PI / k) + Mathd.Tan(Mathd.PI / n)));
    }

    // Returns x, where (x, sqrt(x^2 + 1), 0) is a point on the hyperboloid representing a vertex of the origin polygon.
    public static double firstVertex(int n, int k)
    {
        return Mathd.Sqrt(-1 + 1 / (Mathd.Pow(Mathd.Tan(Mathd.PI_PRECISE / n), 2.0d) * Mathd.Pow(Mathd.Tan(Mathd.PI_PRECISE / k), 2.0d)));
    }

    /*******************
    * Note on translations: do in reverse order. I.e. to get RUL, do left -> up -> right translations.
    ********************/

    // Translate vector in x-direction
    public static Vector3d translateX(Vector3d v, double dist)
    {
        double co = Cosh(dist);
        double si = Sinh(dist);
        return new Vector3d(co * v.x + si * v.y, si * v.x + co * v.y, v.z);
    }

    // Translate vector in z-direction
    public static Vector3d translateZ(Vector3d v, double dist)
    {
        double co = Cosh(dist);
        double si = Sinh(dist);
        return new Vector3d(v.x, si * v.z + co * v.y, co * v.z + si * v.y);
    }

    // To find a symmetric translation, from (0,1,0), find x->z, and find z->x, for any variable x,z.
    // Since one value must be modified, let it be f(x) or f(z). Then set the values 
    // equal to each other and solve.
    // 
    // Previous symmetric translation that modified the second value:
    //     v = translateX(v, xdist);
    //     v = translateZ(v, asinh(sinh(zdist)/cosh(xdist)));
    // 
    // Symmetric translation that modifies first value:
    //   sinh(f(x)) = sinh(x) * cosh(asinh(sinh(z) * cosh(f(x))))
    //              = sinh(x) * sqrt( 1 + (sinh(z) * cosh(f(x)))^2 )
    // Note that cosh(asinh(w)) = sqrt(1 + w^2)
    // Solve to get f(x) = acosh(sqrt((1+a)/(1-ab))), where a = (sinhx)^2  and  b = (sinhz)^2

    // Translate vector in both directions
    public static Vector3d translateXZ(Vector3d v, double xdist, double zdist)
    {
        double a = Mathd.Pow(Sinh(xdist), 2.0f);
        double b = Mathd.Pow(Sinh(zdist), 2.0f);
        double fx = Acosh(Mathd.Sqrt((1 + a) / (1 - a * b)));
        if (xdist > 0)
            v = translateX(v, fx);
        else
            v = translateX(v, -fx);
        v = translateZ(v, zdist);
        return v;
    }

    // Reverse translateXZ
    public static Vector3d reverseXZ(Vector3d v, double xdist, double zdist)
    {
        double a = Mathd.Pow(Sinh(xdist), 2.0f);
        double b = Mathd.Pow(Sinh(zdist), 2.0f);
        double fx = Acosh(Mathd.Sqrt((1 + a) / (1 - a * b)));
        v = translateZ(v, -zdist);
        if (xdist > 0)
            v = translateX(v, -fx);
        else
            v = translateX(v, fx);
        return v;
    }

    // Get x and z for translateXZ from vector v. translateXZ(origin, x, z) will give v.
    public static Vector3d getXZ(Vector3d v)
    {
        double fx = Asinh(v.x);
        double zdist = Asinh(v.z / Cosh(fx));

        double left = Mathd.Pow(Cosh(fx), 2.0f);
        double a = (left - 1) / (1 + left * Mathd.Pow(Sinh(zdist), 2.0f));
        double xdist = 0;
        if (fx > 0)
            xdist = Asinh(Mathd.Sqrt(a));
        else
            xdist = -Asinh(Mathd.Sqrt(a));

        return new Vector3d(xdist, 0, zdist);
    }

    // XZ translation that preserves x and z. I.e. translating x and z from the origin gets (x, _, z).
    public static Vector3d translateXZ2(Vector3d v, double x, double z)
    {
        double xdist = Asinh(x);
        v = translateX(v, xdist);
        double zdist = Asinh(z / Cosh(xdist));
        v = translateZ(v, zdist);
        return v;
    }

    // Reverse of XZ2. Will reverse a translateXZ2 call given the same x and z.
    public static Vector3d reverseXZ2(Vector3d v, double x, double z)
    {
        double xdist = Asinh(x);
        double zdist = Asinh(z / Cosh(xdist));
        v = translateZ(v, -zdist);
        v = translateX(v, -xdist);
        return v;
    }

    // Get hyperboloid coordinates from x/z pair
    public static Vector3d fromOrigin(double x, double z)
    {
        if (x == 0 && z == 0)
            return new Vector3d(0, 1, 0);

        double dist = Mathd.Sqrt(Mathd.Pow(x, 2.0f) + Mathd.Pow(z, 2.0f));
        double y = Cosh(dist);
        double ratio = Mathd.Sqrt((Mathd.Pow(y, 2) - 1) / (Mathd.Pow(dist, 2)));

        return new Vector3d(ratio * x, y, ratio * z);
    }

    // Check if two coords are very close
    public static bool close(Vector3d a, Vector3d b)
    {
        double dist = Mathd.Pow(a[0] - b[0], 2) + Mathd.Pow(a[1] - b[1], 2) + Mathd.Pow(a[2] - b[2], 2);
        if (dist < 0.1)
            return true;
        return false;
    }

    // Counter-clockwise rotation, preserving y
    public static Vector3d rotate(Vector3d v, double angle)
    {
        return new Vector3d(v.x * Mathd.Cos(angle) - v.z * Mathd.Sin(angle), v.y, v.x * Mathd.Sin(angle) + v.z * Mathd.Cos(angle));
    }

    // https://en.wikipedia.org/wiki/Rodrigues%27_rotation_formula
    // Clockwise rotation around an axis --- v is the vector, n is the axis
    // This becomes a CCW rotation if you swap y and z in v and n, then swap back after rotating'
    // NOTE: The original rotation preserves Euclidean norm, but not hyperbolic quadratic form.
    // I added a normalize so the quadratic form is preserved.
    public static Vector3d rotateAxis(Vector3d v, Vector3d n, double t)
    {
        n = Vector3d.Normalize(n);
        // Third part of the formula should be unnecessary if doing hypernorm.
        return hypNormalize(v * Mathd.Cos(t) + Vector3d.Cross(n, v) * Mathd.Sin(t) + n * (Vector3d.Dot(n, v)) * (1 - Mathd.Cos(t)));
    }

    // Gradient on hyperboloid
    public static Vector3d gradient(Vector3d v)
    {
        return new Vector3d(2 * v.x, -2 * v.y, 2 * v.z);
    }
}
