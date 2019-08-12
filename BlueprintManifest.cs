using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcaneTide
{
    public class BlueprintManifest : ScriptableObject
    {
        public UnityEngine.Object[] Parents;
        public string[] Fields;
        public void SetExistingBlueprints(IEnumerable<Tuple<UnityEngine.Object, string>> data)
        {
            Parents = data.Select(t => t.Item1).ToArray();
            Fields = data.Select(t => t.Item2).ToArray();
        }
    }
}