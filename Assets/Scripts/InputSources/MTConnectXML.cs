// System
using System.Collections.Generic;

// XML Parsing
using System.Xml.Serialization;

/// <summary> Generated C# from XML via Visual Studio. </summary>
namespace MTConnectVR.XML.MTConnectStreams {

    [XmlRoot(ElementName = "Header", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class Header {
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

    [XmlRoot(ElementName = "MachineState", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class MachineState {
        [XmlAttribute(AttributeName = "dataItemId")]
        public string DataItemId { get; set; }
        [XmlAttribute(AttributeName = "timestamp")]
        public string Timestamp { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "sequence")]
        public string Sequence { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Events", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class Events {
        [XmlElement(ElementName = "MachineState", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public List<MachineState> MachineState { get; set; }
        [XmlElement(ElementName = "AssetChanged", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public AssetChanged AssetChanged { get; set; }
        [XmlElement(ElementName = "AssetRemoved", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public AssetRemoved AssetRemoved { get; set; }
        [XmlElement(ElementName = "Availability", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public List<Availability> Availability { get; set; }
        [XmlElement(ElementName = "RemoteState", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public RemoteState RemoteState { get; set; }
    }

    [XmlRoot(ElementName = "ComponentStream", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class ComponentStream {
        [XmlElement(ElementName = "Events", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public Events Events { get; set; }
        [XmlAttribute(AttributeName = "component")]
        public string Component { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "componentId")]
        public string ComponentId { get; set; }
        [XmlElement(ElementName = "Samples", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public Samples Samples { get; set; }
    }

    [XmlRoot(ElementName = "AssetChanged", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class AssetChanged {
        [XmlAttribute(AttributeName = "dataItemId")]
        public string DataItemId { get; set; }
        [XmlAttribute(AttributeName = "timestamp")]
        public string Timestamp { get; set; }
        [XmlAttribute(AttributeName = "sequence")]
        public string Sequence { get; set; }
        [XmlAttribute(AttributeName = "assetType")]
        public string AssetType { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "AssetRemoved", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class AssetRemoved {
        [XmlAttribute(AttributeName = "dataItemId")]
        public string DataItemId { get; set; }
        [XmlAttribute(AttributeName = "timestamp")]
        public string Timestamp { get; set; }
        [XmlAttribute(AttributeName = "sequence")]
        public string Sequence { get; set; }
        [XmlAttribute(AttributeName = "assetType")]
        public string AssetType { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Availability", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class Availability {
        [XmlAttribute(AttributeName = "dataItemId")]
        public string DataItemId { get; set; }
        [XmlAttribute(AttributeName = "timestamp")]
        public string Timestamp { get; set; }
        [XmlAttribute(AttributeName = "sequence")]
        public string Sequence { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Angle", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class Angle {
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

    [XmlRoot(ElementName = "Samples", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class Samples {
        [XmlElement(ElementName = "Angle", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public List<Angle> Angle { get; set; }
        [XmlElement(ElementName = "Torque", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public List<Torque> Torque { get; set; }
        [XmlElement(ElementName = "Position", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public List<Position> Position { get; set; }
        [XmlElement(ElementName = "AxisFeedrate", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public AxisFeedrate AxisFeedrate { get; set; }
        [XmlElement(ElementName = "SoundLevel", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public SoundLevel SoundLevel { get; set; }
    }

    [XmlRoot(ElementName = "Position", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class Position {
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

    [XmlRoot(ElementName = "DeviceStream", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class DeviceStream {
        [XmlElement(ElementName = "ComponentStream", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public List<ComponentStream> ComponentStream { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "uuid")]
        public string Uuid { get; set; }
    }

    [XmlRoot(ElementName = "AxisFeedrate", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class AxisFeedrate {
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

    [XmlRoot(ElementName = "RemoteState", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class RemoteState {
        [XmlAttribute(AttributeName = "dataItemId")]
        public string DataItemId { get; set; }
        [XmlAttribute(AttributeName = "timestamp")]
        public string Timestamp { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "sequence")]
        public string Sequence { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "SoundLevel", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class SoundLevel {
        [XmlAttribute(AttributeName = "dataItemId")]
        public string DataItemId { get; set; }
        [XmlAttribute(AttributeName = "timestamp")]
        public string Timestamp { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "sequence")]
        public string Sequence { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Streams", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class Streams {
        [XmlElement(ElementName = "DeviceStream", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
        public List<DeviceStream> DeviceStream { get; set; }
    }

    [XmlRoot(ElementName = "MTConnectStreams", Namespace = "urn:mtconnect.org:MTConnectStreams:1.4")]
    public class MTConnectStreams {
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