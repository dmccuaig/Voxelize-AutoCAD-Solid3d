using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.Geometry;

namespace Voxelize.Model;

public class Vertex
{
	public readonly Point3d Point;
	public readonly int Xi;
	public readonly int Yi;
	public readonly int Zi;

	public bool IsInside { get; private set; }

	public Vertex(Point3d point, int xi, int yi, int zi)
	{
		Point = point;
		Xi = xi;
		Yi = yi;
		Zi = zi;
	}

	public void DetermineInside(Brep brep)
	{
		brep.GetPointContainment(Point, out var containment);
		IsInside = containment != PointContainment.Outside;
	}
}