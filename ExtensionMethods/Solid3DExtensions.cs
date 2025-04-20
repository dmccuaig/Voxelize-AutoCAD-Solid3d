using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Voxelize.ExtensionMethods;

public static class Solid3DExtensions
{
	/// <summary>
	///     TODO
	/// </summary>
	/// <param name="acSol"></param>
	/// <param name="from"></param>
	/// <param name="to"></param>
	public static void Move(this Solid3d acSol, Point3d from, Point3d to)
	{
		acSol.TransformBy(Matrix3d.Displacement(from.GetVectorTo(to)));
	}


	public static bool IsInside(this Extents3d thisExt, Extents3d overExt, Tolerance tol)
	{
		if ((thisExt.MinPoint.X < overExt.MinPoint.X || thisExt.MinPoint.Y < overExt.MinPoint.Y ||
		     thisExt.MinPoint.Z < overExt.MinPoint.Z) &&
		    !thisExt.MinPoint.IsEqualTo(overExt.MinPoint, tol)) return false;
		if (thisExt.MaxPoint.X > overExt.MaxPoint.X || thisExt.MaxPoint.Y > overExt.MaxPoint.Y ||
		    thisExt.MaxPoint.Z > overExt.MaxPoint.Z) return thisExt.MaxPoint.IsEqualTo(overExt.MaxPoint, tol);
		return true;
	}

	public static bool Intersects(this Extents2d thisExt, Extents2d overExt, Tolerance tol)
	{
		if (thisExt.MaxPoint.X < overExt.MinPoint.X - tol.EqualPoint ||
		    thisExt.MinPoint.X > overExt.MaxPoint.X + tol.EqualPoint) return false;
		return thisExt.MaxPoint.Y >= overExt.MinPoint.Y - tol.EqualPoint &&
		       thisExt.MinPoint.Y <= overExt.MaxPoint.Y + tol.EqualPoint;
	}

	public static bool Intersects(this Extents3d thisExt, Extents3d overExt, Tolerance tol)
	{
		if (thisExt.MaxPoint.X < overExt.MinPoint.X - tol.EqualPoint ||
		    thisExt.MinPoint.X > overExt.MaxPoint.X + tol.EqualPoint) return false;
		if (thisExt.MaxPoint.Y < overExt.MinPoint.Y - tol.EqualPoint ||
		    thisExt.MinPoint.Y > overExt.MaxPoint.Y + tol.EqualPoint) return false;
		return thisExt.MaxPoint.Z >= overExt.MinPoint.Z - tol.EqualPoint &&
		       thisExt.MinPoint.Z <= overExt.MaxPoint.Z + tol.EqualPoint;
	}


}