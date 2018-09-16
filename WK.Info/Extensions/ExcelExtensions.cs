using NPOI.SS.UserModel;

namespace WK.Info
{
	public static class ExcelExtensions
	{
		public static ICell CreateNumericCell(this IRow row, int column)
		{
			var cell = row.CreateCell(column);
			cell.SetCellType(CellType.Numeric);
			return cell;
		}
	}
}
