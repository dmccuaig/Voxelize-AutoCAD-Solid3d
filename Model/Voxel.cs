using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Voxelize.Model;

public class Voxel
{
	public readonly int Xi;
	public readonly int Yi;
	public readonly int Zi;
	public readonly Extents3d Extents;
	public bool Intersects { get; private set; }

	public Vertex V000;
	public Vertex V001;
	public Vertex V101;
	public Vertex V100;
	public Vertex V010;
	public Vertex V011;
	public Vertex V111;
	public Vertex V110;

	public IEnumerable<Vertex> Vertices
	{
		get
		{
			yield return V000;
			yield return V001;
			yield return V101;
			yield return V100;
			yield return V010;
			yield return V011;
			yield return V111;
			yield return V110;
		}
	}

	public Point3d MinPoint => V000.Point;
	public Point3d MaxPoint => V111.Point;

	public Voxel(VoxelModel vm, int xi, int yi, int zi)
	{
		Xi = xi;
		Yi = yi;
		Zi = zi;
		V000 = vm.Vertices[xi, yi, zi];
		V001 = vm.Vertices[xi, yi, zi + 1];
		V101 = vm.Vertices[xi + 1, yi, zi + 1];
		V100 = vm.Vertices[xi + 1, yi, zi];
		V010 = vm.Vertices[xi, yi + 1, zi];
		V011 = vm.Vertices[xi, yi + 1, zi + 1];
		V111 = vm.Vertices[xi + 1, yi + 1, zi + 1];
		V110 = vm.Vertices[xi + 1, yi + 1, zi];

		Extents = new Extents3d(V000.Point, V111.Point);
	}

	public void DetermineInside()
	{
		Intersects = Vertices.Any(v => v.IsInside);
	}
}