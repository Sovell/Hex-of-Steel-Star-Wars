using System.IO;
using Steamworks;

public class ModItem
{
	public string name;

	public string installPath;

	public DirectoryInfo directoryInfo;

	public float installSize;

	public bool isSteam;

	public PublishedFileId_t steamItemID;

	public ModItem(DirectoryInfo p_directoryInfo)
	{
		name = p_directoryInfo.Name;
		directoryInfo = p_directoryInfo;
		installPath = p_directoryInfo.FullName;
		installSize = Utils.Truncate((float)Utils.GetDirectorySize(installPath), 1);
	}

	public ModItem(DirectoryInfo p_directoryInfo, PublishedFileId_t p_publishedFileId_T)
		: this(p_directoryInfo)
	{
		isSteam = true;
		steamItemID = p_publishedFileId_T;
	}
}
