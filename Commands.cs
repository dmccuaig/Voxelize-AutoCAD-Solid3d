using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
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

	[CommandMethod(nameof(VoxelizeSolid))]
	public static void VoxelizeSolid()
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

		VoxelizeSolid(result.ObjectId, voxelSizeResult.Value);
	}

	private static void VoxelizeSolid(ObjectId solidId, double voxelSize)
	{
		using Transaction tr = CurrentDocument.Database.TransactionManager.StartTransaction();
		var modelSpaceBtr = tr.GetModelSpace(CurrentDatabase);
		if (modelSpaceBtr is null) return;

		using Solid3d? solid = tr.GetObject(solidId, OpenMode.ForRead) as Solid3d;
		if (solid == null) return;

		VoxelizeSolid(solid, voxelSize, tr, modelSpaceBtr);

		tr.Commit();
	}

	private static void VoxelizeSolid(Solid3d solid, double voxelSize, Transaction tr, BlockTableRecord modelSpaceBtr)
	{
		var voxelModel = Voxelize.VoxelizeSolid(solid, voxelSize);
		if (voxelModel is null) return;

		foreach (var voxel in voxelModel.AllVoxels)
		{
			if (voxel.Intersects == false)
			{
				continue;
			}

			Solid3d boxel = voxel.Extents.CreateBox();

			modelSpaceBtr.AppendEntity(boxel);
			tr.AddNewlyCreatedDBObject(boxel, true);
		}
	}

}