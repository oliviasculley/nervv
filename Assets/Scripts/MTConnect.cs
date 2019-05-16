using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;                  // Unity
using UnityEngine.Networking;
using System.Xml.Serialization;     // XML Parsing

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
    public bool useDebugString,
                useOpenHaptics;

    // Private vars
    float timeToTrigger = 0.0f;

    private void Awake() {
        // Add static reference to self
        if (mtc != null)
            Debug.LogWarning("[MTConnect] Static ref to self was not null!\nOverriding...");
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
        if (Time.time > timeToTrigger && !useOpenHaptics) {

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
            Debug.Log("[MTConnect] Added: " + m.name);
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
                float[] temp = new float[split.Length];
                for (int i = 0; i < split.Length; i++)
                    if (!float.TryParse(split[i].ToUpper(), style, provider, out temp[i]))
                        Debug.LogError("[MTConnect] Could not parse string to float: \"" + split[i] + "\"");

                // As of current, send values for Kuka and Shark
                if (machines.Count >= 1 && machines[0] != null)
                    for (int i = 0; i < 3; i++)
                        machines[0].SetAxisAngle("A" + (i + 1), temp[i]);

                if (machines.Count >= 2 && machines[1] != null)
                    for (int i = 3; i < split.Length; i++)
                        machines[1].SetAxisAngle("A" + (i - 2), temp[i]);
            }
        }
    }

    /* XML Serialization Classes */
    /* Converted from https://xmltocsharp.azurewebsites.net/ */

    [XmlRoot(ElementName = "Header", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class Header
    {
        [XmlAttribute(AttributeName = "creationTime")]
        public string CreationTime { get; set; }
        [XmlAttribute(AttributeName = "sender")]
        public string Sender { get; set; }
        [XmlAttribute(AttributeName = "instanceId")]
        public string InstanceId { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "bufferSize")]
        public string BufferSize { get; set; }
        [XmlAttribute(AttributeName = "nextSequence")]
        public string NextSequence { get; set; }
        [XmlAttribute(AttributeName = "firstSequence")]
        public string FirstSequence { get; set; }
        [XmlAttribute(AttributeName = "lastSequence")]
        public string LastSequence { get; set; }
    }

    [XmlRoot(ElementName = "Torque", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class Torque
    {
        [XmlAttribute(AttributeName = "dataItemId")]
        public string DataItemId { get; set; }
        [XmlAttribute(AttributeName = "timestamp")]
        public string Timestamp { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "sequence")]
        public string Sequence { get; set; }
        [XmlAttribute(AttributeName = "subType")]
        public string SubType { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Samples", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class Samples
    {
        [XmlElement(ElementName = "Torque", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public List<Torque> Torque { get; set; }
    }

    [XmlRoot(ElementName = "ComponentStream", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class ComponentStream
    {
        [XmlElement(ElementName = "Samples", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public Samples Samples { get; set; }
        [XmlAttribute(AttributeName = "component")]
        public string Component { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "componentId")]
        public string ComponentId { get; set; }
    }

    [XmlRoot(ElementName = "DeviceStream", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class DeviceStream
    {
        [XmlElement(ElementName = "ComponentStream", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public List<ComponentStream> ComponentStream { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "uuid")]
        public string Uuid { get; set; }
    }

    [XmlRoot(ElementName = "Streams", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class Streams
    {
        [XmlElement(ElementName = "DeviceStream", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public DeviceStream DeviceStream { get; set; }
    }

    [XmlRoot(ElementName = "MTConnectStreams", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class MTConnectStreams
    {
        [XmlElement(ElementName = "Header", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public Header Header { get; set; }
        [XmlElement(ElementName = "Streams", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public Streams Streams { get; set; }
        [XmlAttribute(AttributeName = "m", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string M { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string SchemaLocation { get; set; }
    }
}
