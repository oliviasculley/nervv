// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[InitializeOnLoad]
public class PrettyDebug : ILogHandler {

    public PrettyDebug() {
        Debug.unityLogger.logHandler = this;
    }

    #region Instance Methods
    public void LogException(Exception exception, UnityEngine.Object context) {
        Debug.LogException(exception, context);
    }

    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args) {

        string s = "[" + context.name + "] " + format;

        switch (logType) {
            case LogType.Assert:
                Debug.LogAssertionFormat(context, s, args);
                return;

            case LogType.Error:
                Debug.LogErrorFormat(context, s, args);
                return;

            case LogType.Exception:
                Debug.LogErrorFormat(context, s, args);
                return;

            case LogType.Log:
                Debug.LogFormat(context, s, args);
                return;

            case LogType.Warning:
                Debug.LogWarningFormat(context, s, args);
                return;

            default:
                Debug.LogWarning("[Pretty Debug] Could not find handler for LogType!");
                return;
        }
    }
    #endregion
}

#endif