using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Voxelize.ExtensionMethods;

namespace Voxelize;

public static class Voxelize
{
	public static VoxelModel? VoxelizeSolid(Solid3d solid, double recommendedVoxelSize)
	{
		if (solid.Bounds is Extents3d bounds == false)
		{
			return null;
		}

		// Get the best fit edge length using the suggested voxel size.
		GetOptimalVoxelEdgeLength(bounds, recommendedVoxelSize, out int voxelEdgeCount, out double voxelEdgeLength);
		Extents3d voxelExtents = GetVoxelExtents(bounds, voxelEdgeCount, voxelEdgeLength);

		return new VoxelModel(voxelExtents, voxelEdgeCount, voxelEdgeLength);
	}

	// Get the best fit edge length using the suggested voxel size.
	public static void GetOptimalVoxelEdgeLength(Extents3d solidBounds, double recommendedVoxelSize, out int edgeCount, out double edgeLength)
	{
		double longestEdge = solidBounds.GetLongestEdge();

		edgeCount = (int)Math.Ceiling(longestEdge / recommendedVoxelSize);
		edgeLength = longestEdge / edgeCount;
	}

	public static Extents3d GetVoxelExtents(Extents3d solidBounds, int voxelEdgeCount, double voxelEdgeLength)
	{
		double halfEdgeLength = voxelEdgeCount * voxelEdgeLength / 2.0;
		var centeredBoxMid = solidBounds.CenterPoint();

		var centeredBoxMinPoint = new Point3d(centeredBoxMid.X - halfEdgeLength, centeredBoxMid.Y - halfEdgeLength, centeredBoxMid.Z - halfEdgeLength);
		var centeredBoxMaxPoint = new Point3d(centeredBoxMid.X + halfEdgeLength, centeredBoxMid.Y + halfEdgeLength, centeredBoxMid.Z + halfEdgeLength);

		Extents3d centeredBox = new Extents3d(centeredBoxMinPoint, centeredBoxMaxPoint);

		return centeredBox;
	}

	public static VoxelModel CreateVoxelModel(Extents3d voxelExtents, int voxelEdgeCount, double voxelEdgeLength)
	{
		var model = new VoxelModel(voxelExtents, voxelEdgeCount, voxelEdgeLength);

		return model;
	}
}

public class VoxelModel
{
	public readonly Extents3d VoxelExtents;
	public readonly int VoxelEdgeCount;
	public readonly double EdgeLength;

	public Vertex[,,] Vertices { get; }
	public Voxel[,,] Voxels { get; }

	public VoxelModel(Extents3d voxelExtents, int voxelEdgeCount, double edgeLength)
	{
		VoxelExtents = voxelExtents;
		VoxelEdgeCount = voxelEdgeCount;
		EdgeLength = edgeLength;

		Vertices = CreateVertices();
		Voxels = CreateVoxels();
	}

	private Vertex[,,] CreateVertices()
	{
		Vertex[,,] vertices = new Vertex[VoxelEdgeCount + 1, VoxelEdgeCount + 1, VoxelEdgeCount + 1];

		int xi, yi, zi;
		double x, y, z;

		for (xi = 0, x = VoxelExtents.MinPoint.X; xi <= VoxelEdgeCount; xi++, x += EdgeLength)
		{
			for (yi = 0, y = VoxelExtents.MinPoint.Y; yi <= VoxelEdgeCount; yi++, y += EdgeLength)
			{
				for (zi = 0, z = VoxelExtents.MinPoint.Z; zi <= VoxelEdgeCount; zi++, z += EdgeLength)
				{
					Point3d pt = new Point3d(x, y, z);
					Vertex vertex = new Vertex(pt, xi, yi, zi);
					vertices[xi, yi, zi] = vertex;
				}
			}
		}

		return vertices;
	}

	private Voxel[,,] CreateVoxels()
	{

		Voxel[,,] voxels = new Voxel[VoxelEdgeCount, VoxelEdgeCount, VoxelEdgeCount];

		for (int xi = 0;  xi < VoxelEdgeCount; xi++ )
		{
			for (int yi = 0; yi < VoxelEdgeCount; yi++)
			{
				for (int zi = 0; zi < VoxelEdgeCount; zi++)
				{
					Voxel voxel = new Voxel(this, xi, yi, zi);
					voxels[xi,yi,zi] = voxel;
				}
			}
		}

		return voxels;

	}

	public class Vertex
	{
		public readonly Point3d Point;
		public readonly int Xi;
		public readonly int Yi;
		public readonly int Zi;

		public Vertex(Point3d point, int xi, int yi, int zi)
		{
			Point = point;
			Xi = xi;
			Yi = yi;
			Zi = zi;
		}
	}

	public class Voxel
		{
			public readonly Extents3d Extents;

			public Vertex V000;
			public Vertex V001;
			public Vertex V101;
			public Vertex V100;
			public Vertex V010;
			public Vertex V011;
			public Vertex V111;
			public Vertex V110;

			public Point3d MinPoint => V000.Point;
			public Point3d MaxPoint => V111.Point;

			public Voxel(VoxelModel vm, int xi, int yi, int zi)
			{
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

		}
	}