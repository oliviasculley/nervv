using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;                  
using UnityEngine.Networking;

// XML Parsing
using System.Xml.Serialization;
using System.IO;
using Xml2CSharp;

public class MTConnect : InputSource
{
    [Header("Settings")]
    public string source;
    public float pollInterval;  // Interval in seconds to poll
    public char delim;
    public char[] trimChars;

    //[Header("References")]

    // Private vars
    float timeToTrigger = 0.0f;

    private void Awake() {
        // Safety checks
        Debug.Assert(!string.IsNullOrEmpty(source), "MTConnectURL is null or empty!");
        if (pollInterval == 0)
            Debug.LogWarning("Poll interval set to 0, will send GET request every frame!");
    }

    private void Start() {
        // Add self to InputManager
        Debug.Assert(InputManager.Instance != null, "[MTConnect] Could not get ref to InputManager!");
        if (!InputManager.Instance.AddInput(this))
            Debug.LogError("[MTConnect] Could not add self to InputManager!");
    }

    private void Update()
    {
        // Check if time to trigger
        if (Time.time > timeToTrigger) {

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
        using (UnityWebRequest www = UnityWebRequest.Get(source)) {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) {
                Debug.LogError("GET request returned error: " + www.error);
            } else {
                //Debug.Log("[INFO] GET request returned: " + www.downloadHandler.text);

                // Parse XML
                // DEBUG: Time how long it takes
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                ParseXML(www.downloadHandler.text);
                sw.Stop();
                Debug.Log("Parsed XML in " + sw.ElapsedMilliseconds + " ms");
            }
        }
    }

    private void ParseXML(string input) {
        XmlSerializer serializer = new XmlSerializer(typeof(MTConnectDevices));
        TextReader reader = new StringReader(input);
        MTConnectDevices xmlData = (MTConnectDevices) serializer.Deserialize(reader);

        // Debug list out devices
        string temp = "DEBUG: Parsing XML Devices...\n";
        foreach (Device d in xmlData.Devices.Device)
            temp += "ID: " + d.Id.ToString() +
                    ", Name: " + d.Name.ToString() +
                    ", UUID: " + d.Uuid.ToString() + "\n";
        Debug.Log(temp);
    }

}

#region XMLSerialization

/* XML Serialization Classes */
/* Converted from https://xmltocsharp.azurewebsites.net/ */
namespace Xml2CSharp {

    [XmlRoot(ElementName = "Header", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Header {
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
    public class Description {
        [XmlAttribute(AttributeName = "manufacturer")]
        public string Manufacturer { get; set; }
        [XmlAttribute(AttributeName = "model")]
        public string Model { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "DataItem", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class DataItem {
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
    public class DataItems {
        [XmlElement(ElementName = "DataItem", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public List<DataItem> DataItem { get; set; }
    }

    [XmlRoot(ElementName = "Controller", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Controller {
        [XmlElement(ElementName = "DataItems", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public DataItems DataItems { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "Linear", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Linear {
        [XmlElement(ElementName = "DataItems", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public DataItems DataItems { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "Components", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Components {
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
    public class Axes {
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
    public class Device {
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
    public class Rotary {
        [XmlElement(ElementName = "DataItems", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public DataItems DataItems { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "Sensor", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Sensor {
        [XmlElement(ElementName = "DataItems", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public DataItems DataItems { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "Systems", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Systems {
        [XmlElement(ElementName = "Components", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public Components Components { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "Devices", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class Devices {
        [XmlElement(ElementName = "Device", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public List<Device> Device { get; set; }
    }

    [XmlRoot(ElementName = "MTConnectDevices", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class MTConnectDevices {
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
#endregion