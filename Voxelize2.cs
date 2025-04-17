using System.Diagnostics.CodeAnalysis;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.BoundaryRepresentation;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

// This is the main entry point for the AutoCAD add-in.
// It defines the commands that will be available in AutoCAD.
namespace VoxelizerAddin
{
    // This class defines the commands for the add-in.
    public class Commands
    {
        // This is the command that will voxelize a solid.
        // It is decorated with the CommandMethod attribute, which tells AutoCAD
        // that this method is a command.
        // The command name is "VoxelizeSolid".
        [CommandMethod("VoxelizeSolid1")]
        public static void VoxelizeSolidCommand1()
        {
            // Get the active document and database.
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // Prompt the user to select a solid.
            PromptEntityOptions options = new PromptEntityOptions("Select a 3D solid to voxelize: ");
            options.AddAllowedClass(typeof(Solid3d), true); // Ensure only 3D solids can be selected.
            PromptEntityResult result = ed.GetEntity(options);

            // If the user cancels, return.
            if (result.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nCommand cancelled.");
                return;
            }

            // Get the object ID of the selected solid.
            ObjectId solidId = result.ObjectId;

            // Prompt the user for the voxel size.
            PromptDoubleOptions voxelSizeOptions = new PromptDoubleOptions("Enter the voxel size: ");
            voxelSizeOptions.DefaultValue = 1.0; // Set a default value.
            //voxelSizeOptions.LowerLimit = 0.0001; // Prevent zero or negative voxel sizes.
            voxelSizeOptions.Message = "\nEnter voxel size: ";
            PromptDoubleResult voxelSizeResult = ed.GetDouble(voxelSizeOptions);

            // If the user cancels, return.
            if (voxelSizeResult.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nCommand cancelled.");
                return;
            }

            // Get the voxel size.
            double voxelSize = voxelSizeResult.Value;

            try
            {
                // Perform the voxelization.  This is where the main logic resides.
                List<Solid3d> voxels = VoxelizeSolid(db, solidId, voxelSize);

                // Add the voxels to the database.
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    // Add each voxel to the Model Space.
                    foreach (Solid3d voxel in voxels)
                    {
                        btr.AppendEntity(voxel);
                        tr.AddNewlyCreatedDBObject(voxel, true);
                    }

                    tr.Commit();
                }
                ed.WriteMessage($"\nSuccessfully voxelized the solid into {voxels.Count} voxels.");
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during the voxelization process.
                ed.WriteMessage($"\nError: {ex.Message}");
            }
        }

        // This method performs the voxelization of the solid.
        // It takes the database, the ID of the solid to voxelize, and the voxel size as input.
        // It returns a list of Solid3d objects representing the voxels.
        private static List<Solid3d> VoxelizeSolid(Database db, ObjectId solidId, double voxelSize)
        {
	        List<Solid3d> voxels = null;

            // Open the solid for reading.  Use a Transaction.
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Solid3d solid = (Solid3d)tr.GetObject(solidId, OpenMode.ForRead);

                voxels = Solid3ds(voxelSize, solid);
                tr.Commit();
            } // Transaction is disposed here

            return voxels;
        }

        private static List<Solid3d> Solid3ds(double voxelSize, Solid3d solid)
        {
	        // Get the bounding box of the solid.
	        Extents3d? extents = solid.Bounds;
	        if (extents.HasValue == false)
	        {
		        return new List<Solid3d>();
	        }

	        // Get the boundary representation of the solid.
	        using Brep brep = new Brep(solid);
	        return VoxelizeSolid(brep, extents.Value, voxelSize);
        }

        private static List<Solid3d> VoxelizeSolid(Brep brep, Extents3d extents, double voxelSize)
        {
	        List<Solid3d> voxels = new List<Solid3d>();

	        // Calculate the minimum and maximum points of the bounding box.
	        Point3d minPt = extents.MinPoint;
	        Point3d maxPt = extents.MaxPoint;

	        // Calculate the number of voxels in each direction.
	        int numX = (int)Math.Ceiling((maxPt.X - minPt.X) / voxelSize);
	        int numY = (int)Math.Ceiling((maxPt.Y - minPt.Y) / voxelSize);
	        int numZ = (int)Math.Ceiling((maxPt.Z - minPt.Z) / voxelSize);

	        // Iterate over the voxels.
	        for (int x = 0; x < numX; x++)
	        {
		        for (int y = 0; y < numY; y++)
		        {
			        for (int z = 0; z < numZ; z++)
			        {
				        // Calculate the coordinates of the voxel.
				        double xMin = minPt.X + x * voxelSize;
				        double yMin = minPt.Y + y * voxelSize;
				        double zMin = minPt.Z + z * voxelSize;
				        double xMax = xMin + voxelSize;
				        double yMax = yMin + voxelSize;
				        double zMax = zMin + voxelSize;

				        // Create the voxel as a Solid3d.
				        Solid3d voxel = new Solid3d();
				        Point3d origin = new Point3d(xMin, yMin, zMin);
				        voxel.SetBox(xMax - xMin, yMax - yMin, zMax - zMin, origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis);

				        // Check for interference using AcDb.PlanarRegion.overlap
				        if (IsVoxelIntersectingSolid(brep, voxel))
				        {
					        voxels.Add(voxel); // Add only intersecting voxels.
				        }
				        else
				        {
					        voxel.Dispose(); // Dispose non-intersecting voxels to prevent memory leaks
				        }
			        }
		        }
	        }

	        return voxels;
        }

        // This method checks if a voxel intersects with the solid.
        private static bool IsVoxelIntersectingSolid(Brep brep, Solid3d voxel)
        {
            // Get the boundary representation of the voxel.
            using (Brep voxelBrep = new Brep(voxel))
            {
                // Perform the interference check.
                return brep.HasIntersection(voxelBrep);
            }
        }
    }

    public static class Extensions
    {
	    public static bool HasIntersection(this Brep brep, Brep other)
	    {
		    return false;
	    }

      public static void SetBox(this Solid3d solid, double xWidth, double yWidth, double zWidth, Point3d origin, Vector3d xAxis, Vector3d yAxis, Vector3d zAxis)
      {
	      throw new NotImplementedException();
      }
    }
}
