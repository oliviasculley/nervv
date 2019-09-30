// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Xml.Serialization;

// Unity Engine
using UnityEngine;
using UnityEngine.Networking;

// NERVV
using NERVV;
using NERVV.XML.MTConnectStreams;

/// <summary>
/// MTConnect XML parsing InputSource. Connects to a specified URL
/// and then automatically sets Axis values for multiple machines
/// based on Machine and Axis IDs. Specify manual machine and axis
/// rules to adjust incoming values with adjustments.
/// </summary>
public class MTConnect : InputSource {
    #region MTConnect Settings
    [Tooltip("Used to specify adjustments to incoming values for individual axes"),
    Header("MTConnect Settings")]
    /// <summary>Used to specify adjustments to incoming values for individual axes</summary>
    public AxisValueAdjustment[] adjustments;

    [Tooltip("Current MTConnect data URL")]
    /// <summary>Current MTConnect data URL</summary>
    public string URL = "";

    [Tooltip("Interval in seconds to poll")]
    /// <summary>Interval in seconds to poll</summary>
    public float pollInterval = 0.1f;
    #endregion

    #region Vars
    IEnumerator fetchMTConnect;
    float timeToTrigger = 0.0f;
    #endregion

    #region Unity Methods
    /// <summary>Safety checks and initialize state</summary>
    /// <exception cref="ArgumentException">
    /// Thrown when MTConnectURL is null or empty
    /// </exception>
    protected override void OnEnable() {
        // Safety checks
        if (string.IsNullOrEmpty(URL))
            throw new ArgumentException("MTConnectURL is null or empty!");
        if (PrintDebugMessages && pollInterval == 0)
            Debug.LogWarning("Poll interval set to 0, will send GET request every frame!");

        base.OnEnable();

        // Init vars
        fetchMTConnect = null;
    }

    /// <summary>Check if need to trigger</summary>
    protected void Update() {
        if (InputEnabled) {
            // Check if time to trigger
            if (Time.time > timeToTrigger) {

                // Set new time to trigger
                timeToTrigger = Time.time + pollInterval;

                // Call GET request, get new coroutine
                if (fetchMTConnect != null) {
                    StopCoroutine(fetchMTConnect);
                    if (PrintDebugMessages)
                        Debug.LogWarning("Old HTTP fetch request, refetching...");
                }
                StartCoroutine(fetchMTConnect = FetchMTConnect());
            }
        } else {
            // Disable running coroutines
            if (fetchMTConnect != null)
                StopCoroutine(fetchMTConnect);
        }
    }
    #endregion

    #region Methods
    /// <summary>Sends GET request to MTConnectURL</summary>
    /// <returns>Unity Coroutine</returns>
    IEnumerator FetchMTConnect() {
        WWWForm form = new WWWForm();
        using (UnityWebRequest www = UnityWebRequest.Get(URL)) {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) {
                if (PrintDebugMessages)
                    Debug.LogError("GET request returned error: " + www.error);
            } else {
                //Debug.Log("[INFO] GET request returned: " + www.downloadHandler.text);

                // DEBUG: Time how long it takes
                //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                //sw.Start();
                ParseXML(www.downloadHandler.text);
                //sw.Stop();
                //Debug.Log("Parsed XML in " + sw.ElapsedMilliseconds + " ms");
            }
        }
        fetchMTConnect = null;
    }

    /// <summary>Parses XML Object</summary>
    /// <exception cref="NotSupportedException">
    /// Thrown if AxisType is not recognized</exception>
    void ParseXML(string input) {
        XmlSerializer serializer = new XmlSerializer(typeof(MTConnectStreams));
        TextReader reader = new StringReader(input);
        MTConnectStreams xmlData = (MTConnectStreams)serializer.Deserialize(reader);

        // For each device
        foreach (DeviceStream ds in xmlData.Streams.DeviceStream) {
            IMachine m = MachineManager.Instance.Machines.Find(
                x => x.UUID == ds.Uuid
            );
            if (m == null) {
                if (PrintDebugMessages)
                    Debug.LogWarning("Did not find matching machine: " + ds.Uuid);
                continue;
            }

            // Go through each component
            foreach (ComponentStream cs in ds.ComponentStream) {
                Machine.Axis a = m.Axes.Find(x => x.ID == cs.ComponentId);
                if (a == null) {
                    if (PrintDebugMessages)
                        Debug.Log("Did not find matching Axis: " + cs.ComponentId);
                    continue;
                }

                // Depending on axis type
                switch (a.Type) {
                    case Machine.Axis.AxisType.Linear:
                        // Linear axis, get latest position
                        if (cs.Samples.Position.Count == 0)
                            break;
                        cs.Samples.Position.Sort(new PositionTimeStampCompare());
                        Position p = cs.Samples.Position[cs.Samples.Position.Count - 1];
                        if (p.Text == "UNAVAILABLE") break;

                        // Set axis
                        if (PrintDebugMessages)
                            Debug.Log("[MTConnect] Set axis " + a.Name + "'s ExternalValue: " + p.Text);
                        a.ExternalValue = float.Parse(p.Text, CultureInfo.InvariantCulture);
                        break;

                    case Machine.Axis.AxisType.Rotary:
                        // Get latest rotary angle
                        if (cs.Samples.Angle.Count > 0) {
                            cs.Samples.Angle.Sort(new AngleTimeStampCompare());
                            Angle angle = cs.Samples.Angle[cs.Samples.Angle.Count - 1];
                            if (angle.Text == "UNAVAILABLE") break;

                            // Set axis
                            if (PrintDebugMessages)
                                Debug.Log("[MTConnect] Set axis " + a.Name + "'s angle: " + angle.Text);
                            a.ExternalValue = float.Parse(angle.Text, CultureInfo.InvariantCulture);
                        }

                        // Get latest torque
                        if (cs.Samples.Torque.Count > 0) {
                            cs.Samples.Torque.Sort(new TorqueTimeStampCompare());
                            Torque torque = cs.Samples.Torque[cs.Samples.Torque.Count - 1];
                            if (torque.Text == "UNAVAILABLE") break;

                            if (PrintDebugMessages)
                                Debug.Log("[MTConnect] Set axis " + a.Name + "'s torque: " + torque.Text);
                            a.Torque = float.Parse(torque.Text, CultureInfo.InvariantCulture);
                        }
                        break;

                    default:
                        throw new NotSupportedException("Invalid AxisType!");
                }
            }
        }
    }
    #endregion

    #region IComparer Helpers
    class PositionTimeStampCompare : IComparer<Position> {
        public int Compare(Position x, Position y) {
            if (x == null || y == null)
                return 0;
            //Debug.Log("xT: " + x.Timestamp + ", " +
            //    "yT: " + y.Timestamp + ", " +
            //    x.Timestamp.CompareTo(y.Timestamp));
            return x.Timestamp.CompareTo(y.Timestamp);
        }
    }

    class AngleTimeStampCompare : IComparer<Angle> {
        public int Compare(Angle x, Angle y) {
            if (x == null || y == null)
                return 0;
            //Debug.Log("xT: " + x.Timestamp + ", " +
            //    "yT: " + y.Timestamp + ", " +
            //    x.Timestamp.CompareTo(y.Timestamp));
            return x.Timestamp.CompareTo(y.Timestamp);
        }
    }

    class TorqueTimeStampCompare : IComparer<Torque> {
        public int Compare(Torque x, Torque y) {
            if (x == null || y == null)
                return 0;
            //Debug.Log("xT: " + x.Timestamp + ", " +
            //    "yT: " + y.Timestamp + ", " +
            //    x.Timestamp.CompareTo(y.Timestamp));
            return x.Timestamp.CompareTo(y.Timestamp);
        }
    }
    #endregion

    #region AxisValueAdjustment class
    [System.Serializable]
    public class AxisValueAdjustment {
        /// <summary>Machine to adjust axes for</summary>
        [Tooltip("Machine to adjust axes for")]
        public Machine Machine;

        /// <summary>ID of Axis to map to</summary>
        [Tooltip("ID of Axis to map to")]
        public string ID;

        /// <summary>
        /// Offset used to correct between particular input's
        /// worldspace to chosen external worldspace
        /// </summary>
        [Tooltip(
            "Offset used to correct between particular input's" +
            "worldspace to chosen external worldspace")]
        public float Offset;

        /// <summary>
        /// Scale factor used to correct between particular input's
        /// worldspace to chosen external worldspace
        /// </summary>
        [Tooltip(
            "Scale factor used to correct between particular " +
            "input's worldspace to chosen external worldspace")]
        public float ScaleFactor;
    }
    #endregion
}