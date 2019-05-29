using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;                  // Unity
using UnityEngine.Networking;
using System.Xml.Serialization;     // XML Parsing

public class MTConnect : InputSource
{
    // Constructor
    public MTConnect() : base("MTConnect") { }

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

    [XmlRoot(ElementName = "Header", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
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
        [XmlAttribute(AttributeName = "assetBufferSize")]
        public string AssetBufferSize { get; set; }
        [XmlAttribute(AttributeName = "assetCount")]
        public string AssetCount { get; set; }
        [XmlAttribute(AttributeName = "bufferSize")]
        public string BufferSize { get; set; }
    }

    [XmlRoot(ElementName = "Description", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Description
    {
        [XmlAttribute(AttributeName = "manufacturer")]
        public string Manufacturer { get; set; }
        [XmlAttribute(AttributeName = "model")]
        public string Model { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "DataItem", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class DataItem
    {
        [XmlAttribute(AttributeName = "category")]
        public string Category { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "nativeUnits")]
        public string NativeUnits { get; set; }
        [XmlAttribute(AttributeName = "subType")]
        public string SubType { get; set; }
        [XmlAttribute(AttributeName = "units")]
        public string Units { get; set; }
    }

    [XmlRoot(ElementName = "DataItems", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class DataItems
    {
        [XmlElement(ElementName = "DataItem", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public List<DataItem> DataItem { get; set; }
    }

    [XmlRoot(ElementName = "Controller", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Controller
    {
        [XmlElement(ElementName = "DataItems", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public DataItems DataItems { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "Linear", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Linear
    {
        [XmlElement(ElementName = "DataItems", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public DataItems DataItems { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "Components", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Components
    {
        [XmlElement(ElementName = "Linear", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public List<Linear> Linear { get; set; }
        [XmlElement(ElementName = "Controller", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public Controller Controller { get; set; }
        [XmlElement(ElementName = "Axes", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public List<Axes> Axes { get; set; }
        [XmlElement(ElementName = "Systems", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public Systems Systems { get; set; }
    }

    [XmlRoot(ElementName = "Axes", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Axes
    {
        [XmlElement(ElementName = "DataItems", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public DataItems DataItems { get; set; }
        [XmlElement(ElementName = "Components", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public Components Components { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "Device", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Device
    {
        [XmlElement(ElementName = "Description", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public Description Description { get; set; }
        [XmlElement(ElementName = "DataItems", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public DataItems DataItems { get; set; }
        [XmlElement(ElementName = "Components", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public Components Components { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "uuid")]
        public string Uuid { get; set; }
    }

    [XmlRoot(ElementName = "Rotary", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Rotary
    {
        [XmlElement(ElementName = "DataItems", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public DataItems DataItems { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "Sensor", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Sensor
    {
        [XmlElement(ElementName = "DataItems", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public DataItems DataItems { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "Systems", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Systems
    {
        [XmlElement(ElementName = "Components", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public Components Components { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "Devices", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Devices
    {
        [XmlElement(ElementName = "Device", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public List<Device> Device { get; set; }
    }

    [XmlRoot(ElementName = "MTConnectDevices", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class MTConnectDevices
    {
        [XmlElement(ElementName = "Header", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public Header Header { get; set; }
        [XmlElement(ElementName = "Devices", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public Devices Devices { get; set; }
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
