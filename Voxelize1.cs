using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(Voxelizer.VoxelizeCommand))]

namespace Voxelizer
{
    public class VoxelizeCommand
    {
        [CommandMethod("VOXELIZE2")]
        public void VoxelizeSolid2()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            // Prompt the user to select a solid
            PromptEntityOptions options = new PromptEntityOptions("\nSelect a 3D solid: ");
            options.SetRejectMessage("\nOnly 3D solids are allowed.");
            options.AddAllowedClass(typeof(Solid3d), true);

            PromptEntityResult result = ed.GetEntity(options);
            if (result.Status != PromptStatus.OK) return;

            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                Solid3d solid = tr.GetObject(result.ObjectId, OpenMode.ForRead) as Solid3d;

                if (solid == null)
                {
                    ed.WriteMessage("\nInvalid selection.");
                    return;
                }

                // Voxelize logic (example with bounding box)
                Extents3d extents = solid.GeometricExtents;
                double voxelSize = 1.0; // Size of each voxel

                Point3d min = extents.MinPoint;
                Point3d max = extents.MaxPoint;

                for (double x = min.X; x < max.X; x += voxelSize)
                {
                    for (double y = min.Y; y < max.Y; y += voxelSize)
                    {
                        for (double z = min.Z; z < max.Z; z += voxelSize)
                        {
                            Point3d voxelCenter = new Point3d(x + voxelSize / 2, y + voxelSize / 2, z + voxelSize / 2);
                            if (solid.IsPointInside(voxelCenter, Tolerance.Global))
                            {
                                // Create voxel as a cube
                                CreateVoxel(doc.Database, voxelCenter, voxelSize);
                            }
                        }
                    }
                }

                tr.Commit();
            }
        }

        private void CreateVoxel(Database db, Point3d center, double size)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (Solid3d voxel = new Solid3d())
                {
                    voxel.CreateBox(size, size, size);
                    voxel.TransformBy(Matrix3d.Displacement(center - Point3d.Origin));
                    btr.AppendEntity(voxel);
                    tr.AddNewlyCreatedDBObject(voxel, true);
                }

                tr.Commit();
            }
        }
    }

    public static class Extensions1
    {
	    public static bool IsPointInside(this Solid3d solid, Point3d point, Tolerance tol)
	    {
		    return false;
	    }
    }
}