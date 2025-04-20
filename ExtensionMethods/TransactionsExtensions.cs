using Autodesk.AutoCAD.DatabaseServices;

namespace Voxelize.ExtensionMethods
{
	public static class TransactionsExtensions
	{
		public static BlockTableRecord? GetModelSpace(this Transaction tr, Database db)
		{
			var acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
			if (acBlkTbl == null)
			{
				return null;
			}

			var acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
			return acBlkTblRec;
		}

	}
}
