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
	public Point3d[,,] Vertices { get; private set; }
	public VoxelModel[,,] Voxels { get; private set; }

	public VoxelModel(Extents3d voxelExtents, int voxelEdgeCount, double edgeLength)
	{
		Vertices = new Point3d[voxelEdgeCount + 1, voxelEdgeCount + 1, voxelEdgeCount + 1];
		Voxels = new VoxelModel[voxelEdgeCount, voxelEdgeCount, voxelEdgeCount];

		int xi, yi, zi;
		double x, y, z;

		for (xi = 0, x = voxelExtents.MinPoint.X; xi <= voxelEdgeCount; xi++, x += edgeLength)
		{
			for (yi = 0, y = voxelExtents.MinPoint.Y; yi <= voxelEdgeCount; yi++, y += edgeLength)
			{
				for (zi = 0, z = voxelExtents.MinPoint.Z; zi <= voxelEdgeCount; zi++, z += edgeLength)
				{
					Vertices[xi, yi, zi] = new Point3d(x, y, z);
				}
			}
		}
	}
}