using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Voxelize.ExtensionMethods;

namespace Voxelize.Model;

public class VoxelModel
{
	public readonly Extents3d VoxelExtents;
	public readonly int VoxelEdgeCount;
	public readonly double EdgeLength;

	public Vertex[,,] Vertices { get; }
	public IEnumerable<Vertex> AllVertices => Vertices.Enumerate3d();

	public Voxel[,,] Voxels { get; }
	public IEnumerable<Voxel> AllVoxels => Voxels.Enumerate3d();

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

		for (int xi = 0; xi < VoxelEdgeCount; xi++)
		{
			for (int yi = 0; yi < VoxelEdgeCount; yi++)
			{
				for (int zi = 0; zi < VoxelEdgeCount; zi++)
				{
					Voxel voxel = new Voxel(this, xi, yi, zi);
					voxels[xi, yi, zi] = voxel;
				}
			}
		}

		return voxels;
	}

	public void DetermineInside(Brep brep)
	{
		foreach (var vertex in AllVertices)
		{
			vertex.DetermineInside(brep);
		}

		foreach (var voxel in AllVoxels)
		{
			voxel.DetermineInside();
		}

	}
}