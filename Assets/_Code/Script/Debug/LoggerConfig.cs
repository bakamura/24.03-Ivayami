using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ivayami/Debug/LoggerConfig")]
public class LoggerConfig : ScriptableObject {

    public List<LogType> typesToLog;

}
