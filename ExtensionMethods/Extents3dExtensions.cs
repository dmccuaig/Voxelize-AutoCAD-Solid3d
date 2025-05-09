﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Voxelize.ExtensionMethods;

public static class Extents3dExtensions
{
	public static Point3d CenterPoint(this Extents3d exts) => exts.MinPoint + (exts.MaxPoint - exts.MinPoint) / 2.0;

	public static double GetLengthX(this Extents3d extents) => extents.MaxPoint.X - extents.MinPoint.X;
	public static double GetLengthY(this Extents3d extents) => extents.MaxPoint.Y - extents.MinPoint.Y;
	public static double GetLengthZ(this Extents3d extents) => extents.MaxPoint.Z - extents.MinPoint.Z;

	public static double GetLongestEdge(this Extents3d extents3d) => Math.Max(Math.Max(extents3d.GetLengthX(), extents3d.GetLengthY()), Math.Max(extents3d.GetLengthY(), extents3d.GetLengthZ()));

	public static Solid3d CreateBox(this Extents3d extents)
	{
		Solid3d box = new();
		box.CreateBox(extents.GetLengthX(), extents.GetLengthY(), extents.GetLengthZ());
		box.TransformBy(Matrix3d.Displacement(Point3d.Origin.GetVectorTo(extents.CenterPoint())));
		return box;
	}
}