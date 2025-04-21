using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class RuntimeData
{
    [JsonIgnore]
    public object owner = null;
}
