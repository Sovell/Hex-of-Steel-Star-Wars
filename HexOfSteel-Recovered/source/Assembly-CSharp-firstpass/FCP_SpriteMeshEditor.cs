using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class FCP_SpriteMeshEditor : MonoBehaviour
{
	public enum MeshType
	{
		CenterPoint,
		forward,
		backward
	}

	public int x;

	public int y;

	public MeshType meshType;

	public Sprite sprite;

	private int bufferedHash;

	private void Update()
	{
		int settingHash = GetSettingHash();
		if (settingHash != 0 && settingHash != bufferedHash)
		{
			MakeMesh(sprite, x, y, meshType);
			Image component = GetComponent<Image>();
			if ((bool)component)
			{
				component.useSpriteMesh = false;
				component.useSpriteMesh = true;
			}
			bufferedHash = settingHash;
		}
	}

	private int GetSettingHash()
	{
		if (sprite == null || x <= 0 || y <= 0)
		{
			return 0;
		}
		return sprite.GetHashCode() * (x ^ 0x88) * (y ^ 0x53E) * (int)((meshType + 1) ^ (MeshType)0x1869F);
	}

	private void MakeMesh(Sprite sprite, int x, int y, MeshType meshtype)
	{
		bool flag = meshType == MeshType.CenterPoint;
		int num = x + 1;
		int num2 = y + 1;
		int num3 = num * num2;
		Vector2[] array;
		ushort[] array2;
		if (flag)
		{
			array = new Vector2[num3 + x * y];
			array2 = new ushort[x * y * 12];
		}
		else
		{
			array = new Vector2[num3];
			array2 = new ushort[x * y * 6];
		}
		for (int i = 0; i < num; i++)
		{
			float num4 = (float)i / (float)x;
			for (int j = 0; j < num2; j++)
			{
				float num5 = (float)j / (float)y;
				array[num * j + i] = new Vector2(num4, num5);
			}
		}
		if (flag)
		{
			for (int k = 0; k < x; k++)
			{
				float num6 = ((float)k + 0.5f) / (float)x;
				for (int l = 0; l < y; l++)
				{
					float num7 = ((float)l + 0.5f) / (float)y;
					array[l * x + k + num3] = new Vector2(num6, num7);
				}
			}
			for (int m = 0; m < x; m++)
			{
				for (int n = 0; n < y; n++)
				{
					int num8 = 12 * (n * x + m);
					int num9 = n * num + m;
					ushort num10 = (ushort)(n * x + m + num3);
					array2[num8 + 11] = (array2[num8] = (ushort)num9);
					array2[num8 + 3] = (array2[num8 + 2] = (ushort)(num9 + 1));
					array2[num8 + 6] = (array2[num8 + 5] = (ushort)(num9 + num + 1));
					array2[num8 + 9] = (array2[num8 + 8] = (ushort)(num9 + num));
					array2[num8 + 1] = (array2[num8 + 4] = (array2[num8 + 7] = (array2[num8 + 10] = num10)));
				}
			}
		}
		else if (meshtype == MeshType.forward)
		{
			for (int num11 = 0; num11 < x; num11++)
			{
				for (int num12 = 0; num12 < y; num12++)
				{
					int num13 = 6 * (num12 * x + num11);
					int num14 = num12 * num + num11;
					array2[num13 + 5] = (array2[num13 + 1] = (ushort)num14);
					array2[num13] = (ushort)(num14 + 1);
					array2[num13 + 4] = (array2[num13 + 2] = (ushort)(num14 + num + 1));
					array2[num13 + 3] = (ushort)(num14 + num);
				}
			}
		}
		else if (meshType == MeshType.backward)
		{
			for (int num15 = 0; num15 < x; num15++)
			{
				for (int num16 = 0; num16 < y; num16++)
				{
					int num17 = 6 * (num16 * x + num15);
					int num18 = num16 * num + num15;
					array2[num17] = (ushort)num18;
					array2[num17 + 4] = (array2[num17 + 2] = (ushort)(num18 + 1));
					array2[num17 + 3] = (ushort)(num18 + num + 1);
					array2[num17 + 5] = (array2[num17 + 1] = (ushort)(num18 + num));
				}
			}
		}
		sprite.OverrideGeometry(array, array2);
	}
}
