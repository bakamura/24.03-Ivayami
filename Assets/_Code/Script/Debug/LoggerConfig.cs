using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Logger/Config")]
public class LoggerConfig : ScriptableObject {

    public List<LogType> typesToLog;

}
