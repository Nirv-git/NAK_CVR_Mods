using System.Reflection;
using UnityEngine;
using System.Linq;
using System.Net;
using System;
using System.Collections.Generic;
using MelonLoader;
using System.IO;



namespace NAK.Blackout;


internal static class AssetsHandler
{

    public static AssetBundle assetBundle;
    public static GameObject fluxPrefab;

    public static void loadAssets()
    {//https://github.com/ddakebono/BTKSASelfPortrait/blob/master/BTKSASelfPortrait.cs
        Assembly l_assembly = Assembly.GetExecutingAssembly();
        string l_assemblyName = l_assembly.GetName().Name;
        using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(l_assemblyName + ".resources." + "blackout_flux"))
        {
            using (var tempStream = new MemoryStream((int)assetStream.Length))
            {
                assetStream.CopyTo(tempStream);
                assetBundle = AssetBundle.LoadFromMemory(tempStream.ToArray(), 0);
                assetBundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            }
        }
        if (assetBundle != null)
        {
            fluxPrefab = assetBundle.LoadAsset<GameObject>("FluxObj_Blackout");
            fluxPrefab.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        }
        else Blackout.Logger.Error("Bundle was null");
    }
}