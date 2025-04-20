using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Voxelize.ExtensionMethods;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(Voxelize.Commands))]

namespace Voxelize;

public static class Commands
{
	public static Document CurrentDocument => AcadApp.DocumentManager.MdiActiveDocument;
	public static Database CurrentDatabase => CurrentDocument.Database;
	public static Editor CurrentEditor => CurrentDocument.Editor;

	[CommandMethod("VoxelizeSolid")]
	public static void VoxelizeSolidCommand()
	{
		// Prompt the user to select a solid
		var options = new PromptEntityOptions("\nSelect a 3D solid: ");
		options.SetRejectMessage("\nOnly 3D solids are allowed.");
		options.AddAllowedClass(typeof(Solid3d), true);

		PromptEntityResult result = CurrentEditor.GetEntity(options);
		if (result.Status != PromptStatus.OK) return;

		var voxelSizeOptions = new PromptDoubleOptions("\nVoxel size estimate: ")
		{
			DefaultValue = 1.0,
			AllowNegative = false,
			AllowZero = false,
			AllowNone = false
		};

		PromptDoubleResult voxelSizeResult = CurrentEditor.GetDouble(voxelSizeOptions);
		if (voxelSizeResult.Status != PromptStatus.OK) return;

		// Get the voxel size.
		double voxelSize = voxelSizeResult.Value;


		using Transaction tr = CurrentDocument.Database.TransactionManager.StartTransaction();
		using Solid3d? solid = tr.GetObject(result.ObjectId, OpenMode.ForRead) as Solid3d;
		if (solid == null) return;

		var voxelModel = Voxelize.VoxelizeSolid(solid, voxelSize);
		if (voxelModel is null) return;

		var acBlkTblRec = tr.GetModelSpace(CurrentDatabase);
		if (acBlkTblRec is null) return;

		for(int xi = 0; xi < voxelModel.Voxels.GetLength(0); xi++)
		{
			for(int yi = 0; yi < voxelModel.Voxels.GetLength(1); yi++)
			{
				for (int zi = 0; zi < voxelModel.Voxels.GetLength(2); zi++)
				{
					var voxel = voxelModel.Voxels[xi, yi, zi];
					Solid3d boxel = new Solid3d();

					boxel.CreateBox(voxel.Extents.GetLengthX(), voxel.Extents.GetLengthY(), voxel.Extents.GetLengthZ());
					boxel.TransformBy(Matrix3d.Displacement(Point3d.Origin.GetVectorTo(voxel.Extents.CenterPoint())));

					acBlkTblRec.AppendEntity(boxel);
					tr.AddNewlyCreatedDBObject(boxel, true);
				}

			}

		}

		tr.Commit();

	}

	private static void DrawBox(VoxelModel.Voxel voxel)
	{

	}

}