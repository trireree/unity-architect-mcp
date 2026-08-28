#pragma warning disable CS0618, CS0619
using System;
using System.IO;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.City
{
    public static class KenneyCityGenerator
    {
        public static McpResponse BuildCity()
        {
            try
            {
                var allTypes = TypeCache.GetTypesDerivedFrom<object>();
                var builderType = allTypes.FirstOrDefault(t => t.Name == "KenneyRoadCityBuilder");
                if (builderType != null)
                {
                    var m = builderType.GetMethod("BuildFullCity");
                    if (m != null)
                    {
                        m.Invoke(null, null);
                        return McpResponse.Success("Kenney Road City built successfully via KenneyRoadCityBuilder!");
                    }
                }
                return McpResponse.Error("KenneyRoadCityBuilder type not found");
            }
            catch (Exception ex)
            {
                return McpResponse.Error($"Build Kenney City Failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
