using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Voxelize.ExtensionMethods;

public static class Extents3dExtensions
{
	public static Point3d CenterPoint(this Extents3d exts) => exts.MinPoint + ((exts.MaxPoint - exts.MinPoint) / 2.0);

	public static double GetLengthX(this Extents3d extents) => extents.MaxPoint.X - extents.MinPoint.X;
	public static double GetLengthY(this Extents3d extents) => extents.MaxPoint.Y - extents.MinPoint.Y;
	public static double GetLengthZ(this Extents3d extents) => extents.MaxPoint.Z - extents.MinPoint.Z;

	public static double GetLongestEdge(this Extents3d extents3d) => Math.Max(Math.Max(extents3d.GetLengthX(), extents3d.GetLengthY()), Math.Max(extents3d.GetLengthY(), extents3d.GetLengthZ()));
}