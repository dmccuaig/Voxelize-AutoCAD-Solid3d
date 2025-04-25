using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Voxelize.ExtensionMethods;
using Voxelize.Model;

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

		var vm = new VoxelModel(voxelExtents, voxelEdgeCount, voxelEdgeLength);

		using Brep brep = new Brep(solid);
		vm.DetermineInside(brep);

		return vm;
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

}