namespace Voxelize.ExtensionMethods;

public static class ArrayExtensions
{
	public static IEnumerable<T> Enumerate3d<T>(this T[,,] array)
	{
		int xiCount = array.GetLength(0);
		int yiCount = array.GetLength(1);
		int ziCount = array.GetLength(2);

		for (int xi = 0; xi < xiCount; xi++)
		{
			for (int yi = 0; yi < yiCount; yi++)
			{
				for (int zi = 0; zi < ziCount; zi++)
				{
					yield return array[xi, yi, zi];
				}
			}
		}
	}
}