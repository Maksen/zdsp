using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;

public class ZealotEditorConfig
{
    static XmlDocument GetEditorConfig()
    {
        XmlDocument doc = new XmlDocument();
        try
        {
            if (File.Exists("./mastercd.config"))
                doc.Load("./mastercd.config");
            else if (File.Exists("./artcd.config"))
                doc.Load("./artcd.config");
            else
                return null;

            return doc;
        }
        catch
        {
            return null;
        }
    }

    public static XmlNodeList GetConfigNodeList(string nodepath)
    {
        XmlDocument doc = GetEditorConfig();
        return (doc == null) ? null : doc.DocumentElement.SelectNodes(nodepath);
    }

    public static XmlNode GetConfigNode(string nodepath)
    {
        XmlDocument doc = GetEditorConfig();
        return (doc == null) ? null : doc.SelectSingleNode(nodepath);
    }

    public static bool HasMasterCDConfig()
    {
        return File.Exists("./mastercd.config");
    }

    public static XmlDocument GetBuildConfig()
    {
        if(File.Exists("./build.config"))
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("./build.config");
                return doc;
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    public static string GetAssetBundlePath()
    {
        XmlDocument doc = GetBuildConfig();
        if (doc != null)
        {
            XmlNode node = doc.SelectSingleNode("/config/build/assetbundle");
            if (node != null)
                return node.Attributes["path"].Value;
        }
        return "AssetBundles";
    }
}
