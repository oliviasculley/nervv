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
    [Header("MTConnect Settings")]
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

        // DEBUG: Send data to MachineManager, will use InputManager in the future
        foreach (MTConnectDevicesDevice d in xmlData.Devices) {
            Machine m;
            /*
            if ((m = MachineManager.Instance.GetMachine(d.id)) != null) {
                foreach (MTConnectDevicesDeviceComponentsAxes a in d.Components.Axes) {
                    foreach (var item in a.Components.Items) {
                        if (item.GetType() == typeof(MTConnectDevicesDeviceComponentsAxesComponentsLinear)) {
                            // Get axisID
                            ((MTConnectDevicesDeviceComponentsAxesComponentsLinear)item).DataItems.DataItem.statistic;
                        } else if (item.GetType() == typeof(MTConnectDevicesDeviceComponentsAxesComponentsRotary)) {
                            foreach(MTConnectDevicesDeviceComponentsAxesComponentsRotaryDataItem dataItem in
                                ((MTConnectDevicesDeviceComponentsAxesComponentsRotary)item).DataItems) {
                                dataItem.statistic;
                            }
                        } else {
                            Debug.LogError("[MTConnect] Could not find Component Type: " + item.GetType().ToString());
                        }
                    }
                }
            }
            */
        }
    }

}

#region XMLSerialization

/* XML Serialization Classes */
/* Converted from https://xmltocsharp.azurewebsites.net/ */
/*
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
        [XmlAttribute(AttributeName = "nextSequence")]
        public string NextSequence { get; set; }
        [XmlAttribute(AttributeName = "firstSequence")]
        public string FirstSequence { get; set; }
        [XmlAttribute(AttributeName = "lastSequence")]
        public string LastSequence { get; set; }
    }

    [XmlRoot(ElementName = "Torque", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class Torque {
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

    [XmlRoot(ElementName = "Samples", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class Samples {
        [XmlElement(ElementName = "Torque", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public List<Torque> Torque { get; set; }
    }

    [XmlRoot(ElementName = "ComponentStream", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class ComponentStream {
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
    public class DeviceStream {
        [XmlElement(ElementName = "ComponentStream", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public List<ComponentStream> ComponentStream { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "uuid")]
        public string Uuid { get; set; }
    }

    [XmlRoot(ElementName = "Streams", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class Streams {
        [XmlElement(ElementName = "DeviceStream", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public DeviceStream DeviceStream { get; set; }
    }

    [XmlRoot(ElementName = "MTConnectDevices", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public class MTConnectDevices {
        [XmlElement(ElementName = "Header", Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
        public Header Header { get; set; }
        [XmlElement(ElementName = "Streams", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public Streams Streams { get; set; }
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
*/
#endregion

#region VSGeneratedXML
namespace Xml2CSharp {
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:mtconnect.org:MTConnectDevices:1.4", IsNullable = false)]
    public partial class MTConnectDevices {

        private MTConnectDevicesHeader headerField;

        private MTConnectDevicesDevice[] devicesField;

        /// <remarks/>
        public MTConnectDevicesHeader Header {
            get {
                return this.headerField;
            }
            set {
                this.headerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Device", IsNullable = false)]
        public MTConnectDevicesDevice[] Devices {
            get {
                return this.devicesField;
            }
            set {
                this.devicesField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesHeader {

        private System.DateTime creationTimeField;

        private string senderField;

        private uint instanceIdField;

        private string versionField;

        private ushort assetBufferSizeField;

        private byte assetCountField;

        private uint bufferSizeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime creationTime {
            get {
                return this.creationTimeField;
            }
            set {
                this.creationTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string sender {
            get {
                return this.senderField;
            }
            set {
                this.senderField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint instanceId {
            get {
                return this.instanceIdField;
            }
            set {
                this.instanceIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string version {
            get {
                return this.versionField;
            }
            set {
                this.versionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort assetBufferSize {
            get {
                return this.assetBufferSizeField;
            }
            set {
                this.assetBufferSizeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte assetCount {
            get {
                return this.assetCountField;
            }
            set {
                this.assetCountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint bufferSize {
            get {
                return this.bufferSizeField;
            }
            set {
                this.bufferSizeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDevice {

        private MTConnectDevicesDeviceDescription descriptionField;

        private MTConnectDevicesDeviceDataItem[] dataItemsField;

        private MTConnectDevicesDeviceComponents componentsField;

        private string idField;

        private string nameField;

        private string uuidField;

        /// <remarks/>
        public MTConnectDevicesDeviceDescription Description {
            get {
                return this.descriptionField;
            }
            set {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("DataItem", IsNullable = false)]
        public MTConnectDevicesDeviceDataItem[] DataItems {
            get {
                return this.dataItemsField;
            }
            set {
                this.dataItemsField = value;
            }
        }

        /// <remarks/>
        public MTConnectDevicesDeviceComponents Components {
            get {
                return this.componentsField;
            }
            set {
                this.componentsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string uuid {
            get {
                return this.uuidField;
            }
            set {
                this.uuidField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceDescription {

        private string manufacturerField;

        private string modelField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string manufacturer {
            get {
                return this.manufacturerField;
            }
            set {
                this.manufacturerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string model {
            get {
                return this.modelField;
            }
            set {
                this.modelField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceDataItem {

        private string categoryField;

        private string idField;

        private string typeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string category {
            get {
                return this.categoryField;
            }
            set {
                this.categoryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponents {

        private MTConnectDevicesDeviceComponentsSystems systemsField;

        private MTConnectDevicesDeviceComponentsController controllerField;

        private MTConnectDevicesDeviceComponentsAxes[] axesField;

        /// <remarks/>
        public MTConnectDevicesDeviceComponentsSystems Systems {
            get {
                return this.systemsField;
            }
            set {
                this.systemsField = value;
            }
        }

        /// <remarks/>
        public MTConnectDevicesDeviceComponentsController Controller {
            get {
                return this.controllerField;
            }
            set {
                this.controllerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Axes")]
        public MTConnectDevicesDeviceComponentsAxes[] Axes {
            get {
                return this.axesField;
            }
            set {
                this.axesField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponentsSystems {

        private MTConnectDevicesDeviceComponentsSystemsComponents componentsField;

        private string idField;

        /// <remarks/>
        public MTConnectDevicesDeviceComponentsSystemsComponents Components {
            get {
                return this.componentsField;
            }
            set {
                this.componentsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponentsSystemsComponents {

        private MTConnectDevicesDeviceComponentsSystemsComponentsSensor sensorField;

        /// <remarks/>
        public MTConnectDevicesDeviceComponentsSystemsComponentsSensor Sensor {
            get {
                return this.sensorField;
            }
            set {
                this.sensorField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponentsSystemsComponentsSensor {

        private MTConnectDevicesDeviceComponentsSystemsComponentsSensorDataItem[] dataItemsField;

        private string idField;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("DataItem", IsNullable = false)]
        public MTConnectDevicesDeviceComponentsSystemsComponentsSensorDataItem[] DataItems {
            get {
                return this.dataItemsField;
            }
            set {
                this.dataItemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponentsSystemsComponentsSensorDataItem {

        private string categoryField;

        private string idField;

        private string nameField;

        private string nativeUnitsField;

        private string typeField;

        private string unitsField;

        private string subTypeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string category {
            get {
                return this.categoryField;
            }
            set {
                this.categoryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string nativeUnits {
            get {
                return this.nativeUnitsField;
            }
            set {
                this.nativeUnitsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string units {
            get {
                return this.unitsField;
            }
            set {
                this.unitsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string subType {
            get {
                return this.subTypeField;
            }
            set {
                this.subTypeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponentsController {

        private MTConnectDevicesDeviceComponentsControllerDataItem[] dataItemsField;

        private string idField;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("DataItem", IsNullable = false)]
        public MTConnectDevicesDeviceComponentsControllerDataItem[] DataItems {
            get {
                return this.dataItemsField;
            }
            set {
                this.dataItemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponentsControllerDataItem {

        private string categoryField;

        private string idField;

        private string nameField;

        private string typeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string category {
            get {
                return this.categoryField;
            }
            set {
                this.categoryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponentsAxes {

        private MTConnectDevicesDeviceComponentsAxesDataItems dataItemsField;

        private MTConnectDevicesDeviceComponentsAxesComponents componentsField;

        private string idField;

        private string nameField;

        /// <remarks/>
        public MTConnectDevicesDeviceComponentsAxesDataItems DataItems {
            get {
                return this.dataItemsField;
            }
            set {
                this.dataItemsField = value;
            }
        }

        /// <remarks/>
        public MTConnectDevicesDeviceComponentsAxesComponents Components {
            get {
                return this.componentsField;
            }
            set {
                this.componentsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponentsAxesDataItems {

        private MTConnectDevicesDeviceComponentsAxesDataItemsDataItem dataItemField;

        /// <remarks/>
        public MTConnectDevicesDeviceComponentsAxesDataItemsDataItem DataItem {
            get {
                return this.dataItemField;
            }
            set {
                this.dataItemField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponentsAxesDataItemsDataItem {

        private string categoryField;

        private string idField;

        private string nameField;

        private string nativeUnitsField;

        private string subTypeField;

        private string typeField;

        private string unitsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string category {
            get {
                return this.categoryField;
            }
            set {
                this.categoryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string nativeUnits {
            get {
                return this.nativeUnitsField;
            }
            set {
                this.nativeUnitsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string subType {
            get {
                return this.subTypeField;
            }
            set {
                this.subTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string units {
            get {
                return this.unitsField;
            }
            set {
                this.unitsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponentsAxesComponents {

        private object[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Linear", typeof(MTConnectDevicesDeviceComponentsAxesComponentsLinear))]
        [System.Xml.Serialization.XmlElementAttribute("Rotary", typeof(MTConnectDevicesDeviceComponentsAxesComponentsRotary))]
        public object[] Items {
            get {
                return this.itemsField;
            }
            set {
                this.itemsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponentsAxesComponentsLinear {

        private MTConnectDevicesDeviceComponentsAxesComponentsLinearDataItems dataItemsField;

        private string idField;

        private string nameField;

        /// <remarks/>
        public MTConnectDevicesDeviceComponentsAxesComponentsLinearDataItems DataItems {
            get {
                return this.dataItemsField;
            }
            set {
                this.dataItemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponentsAxesComponentsLinearDataItems {

        private MTConnectDevicesDeviceComponentsAxesComponentsLinearDataItemsDataItem dataItemField;

        /// <remarks/>
        public MTConnectDevicesDeviceComponentsAxesComponentsLinearDataItemsDataItem DataItem {
            get {
                return this.dataItemField;
            }
            set {
                this.dataItemField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponentsAxesComponentsLinearDataItemsDataItem {

        private string categoryField;

        private string idField;

        private string nameField;

        private string nativeUnitsField;

        private string subTypeField;

        private string typeField;

        private string unitsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string category {
            get {
                return this.categoryField;
            }
            set {
                this.categoryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string nativeUnits {
            get {
                return this.nativeUnitsField;
            }
            set {
                this.nativeUnitsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string subType {
            get {
                return this.subTypeField;
            }
            set {
                this.subTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string units {
            get {
                return this.unitsField;
            }
            set {
                this.unitsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponentsAxesComponentsRotary {

        private MTConnectDevicesDeviceComponentsAxesComponentsRotaryDataItem[] dataItemsField;

        private string idField;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("DataItem", IsNullable = false)]
        public MTConnectDevicesDeviceComponentsAxesComponentsRotaryDataItem[] DataItems {
            get {
                return this.dataItemsField;
            }
            set {
                this.dataItemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:mtconnect.org:MTConnectDevices:1.4")]
    public partial class MTConnectDevicesDeviceComponentsAxesComponentsRotaryDataItem {

        private string categoryField;

        private string idField;

        private string nameField;

        private string nativeUnitsField;

        private string subTypeField;

        private string typeField;

        private string unitsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string category {
            get {
                return this.categoryField;
            }
            set {
                this.categoryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string nativeUnits {
            get {
                return this.nativeUnitsField;
            }
            set {
                this.nativeUnitsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string subType {
            get {
                return this.subTypeField;
            }
            set {
                this.subTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string units {
            get {
                return this.unitsField;
            }
            set {
                this.unitsField = value;
            }
        }
    }
}
#endregion
