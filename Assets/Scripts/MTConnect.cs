using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

public class MTConnect : MonoBehaviour
{
    [Header("Global")]
    public static MTConnect mtc;

    [Header("Properties")]
    public List<Machine> machines;

    [Header("Settings")]
    public string MTConnectURL;
    public float pollInterval;  // Interval in seconds to poll
    public char delim;
    public char[] trimChars;

    //[Header("References")]

    [Header("DEBUG")]
    public float[] debugAxes;
    public bool useDebugString;
    readonly string example = "\"0,0,0,83.6589661,-77.2593613,121.867325,-3.48977657E-4,-5.92440701E-5,1.44744035E-6,\"";

    // Private vars
    float timeToTrigger = 0.0f;

    // Convenience Function for MTConnect
    [Serializable]
    public class MTConnectKukaResp {
        public string resp;
    }

    private void Awake() {
        // Add static reference to self
        mtc = this;

        // Safety checks
        Debug.Assert(!string.IsNullOrEmpty(MTConnectURL), "MTConnectURL is null or empty!");
        if (pollInterval == 0)
            Debug.LogWarning("Poll interval set to 0, will send GET request every frame!");
    }

    private void Start() {
        // Init vals
        machines = new List<Machine>();
    }

    private void Update()
    {
        // Time to trigger
        if (Time.time > timeToTrigger) {

            // Set new time to trigger
            timeToTrigger += pollInterval;

            // Call GET request
            StartCoroutine(FetchMTConnect());
        }
    }

    /* Public Methods */
    
    /// <summary>
    /// Adds machine to be updated by MTConnect
    /// </summary>
    /// <param name="m"></param>
    public void AddMachine(Machine m) {
        if (!machines.Contains(m)) {
            machines.Add(m);
            Debug.Log("Added: " + m.name);
        } else {
            Debug.LogWarning("[MTConnect] Machine \"" + m.name + "\" already added, skipping...");
        }
    }

    /* Private Methods */

    /// <summary>
    /// Sends GET request to MTConnectURL
    /// </summary>
    /// <returns></returns>
    private IEnumerator FetchMTConnect() {

        WWWForm form = new WWWForm();
        using (UnityWebRequest www = UnityWebRequest.Get(MTConnectURL)) {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) {
                Debug.LogError("GET request returned error: " + www.error);
            } else {
                //Debug.Log("[INFO] GET request returned: " + www.downloadHandler.text);

                // Get raw string
                string raw;
                if (useDebugString) {
                    raw = "\"";
                    foreach (float f in debugAxes)
                        raw += f.ToString() + ",";
                    raw += "\"";
                } else {
                    raw = www.downloadHandler.text;
                }

                // Trim spaces and quotations
                raw = raw.Trim(trimChars);
                
                // Split on delimiter
                string[] split = raw.Split(delim);

                // Check for unavailable values
                for (int i = 0; i < split.Length; i++)
                    if (split[i].ToUpper() == "UNAVAILABLE")
                        Debug.LogWarning("Split val " + i + " is unavailable!");

                NumberStyles style = NumberStyles.AllowParentheses | NumberStyles.AllowTrailingSign | NumberStyles.Float | NumberStyles.AllowThousands;
                IFormatProvider provider = CultureInfo.CreateSpecificCulture("en-US");

                // Parse to temp array
                double[] temp = new double[split.Length];
                for (int i = 0; i < split.Length; i++)
                    if (!double.TryParse(split[i].ToUpper(), style, provider, out temp[i]))
                        Debug.LogError("[MTConnect] Could not parse string to double: \"" + split[i] + "\"");

                // As of current, send values for Kuka and CNC
                if (machines.Count >= 1 && machines[0] != null)
                    for (int i = 0; i < 3; i++)
                        machines[0].SetAxisAngle("A" + (i + 1), temp[i]);

                if (machines.Count >= 2 && machines[1] != null)
                    for (int i = 3; i < split.Length; i++)
                        machines[1].SetAxisAngle("A" + (i - 2), temp[i]);
            }
        }
    }
}
