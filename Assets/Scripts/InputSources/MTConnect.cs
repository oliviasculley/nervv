using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;                  
using UnityEngine.Networking;

// XML Parsing
using System.Xml.Serialization;
using System.Net;
using System.IO;
using System.Globalization;
using MTConnectStreamsXML;

public class MTConnect : InputSource
{
    [Header("MTConnect Settings")]
    public readonly string source = "";
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
        XmlSerializer serializer = new XmlSerializer(typeof(MTConnectStreams));
        TextReader reader = new StringReader(input);
        MTConnectStreams xmlData = (MTConnectStreams) serializer.Deserialize(reader);

        // DEBUG: Send data to MachineManager, will use InputManager in the future
		
		// For each device
        foreach (DeviceStream ds in xmlData.Streams.DeviceStream) {
            Machine m = MachineManager.Instance.machines.Find(x => x.uuid == ds.Uuid);
			if (m == null)
				continue;
			
			// Go through each component
			foreach (ComponentStream cs in ds.ComponentStream) {
				Machine.Axis a = m.axes.Find(x => x.GetID() == cs.ComponentId);
				if (a == null)
					continue;

				switch (a.GetAxisType()) {
					case Machine.AxisType.Linear:
						// Linear axis, get latest position
						if (cs.Samples.Position.Count == 0)
							break;
						cs.Samples.Position.Sort(new PositionTimeStampCompare());
						Position p = cs.Samples.Position[cs.Samples.Position.Count - 1];

						if (p.Text == "UNAVAILABLE")
							break;

						// Set axis
						Debug.Log("[MTConnect] Set Position " + a.GetID() + "'s value: " + p.Text);
						a.SetValue(float.Parse(p.Text, CultureInfo.InvariantCulture));
						break;

					case Machine.AxisType.Rotary:
						// Rotary axis, get latest angle
						if (cs.Samples.Angle.Count == 0)
							break;
						cs.Samples.Angle.Sort(new AngleTimeStampCompare());
						Angle angle = cs.Samples.Angle[cs.Samples.Angle.Count - 1];

						if (angle.Text == "UNAVAILABLE")
							break;
						
						// Set axis
						Debug.Log("[MTConnect] Set Angle " + a.GetID() + "'s value: " + angle.Text);
						a.SetValue(float.Parse(angle.Text, CultureInfo.InvariantCulture));
						break;

					default:
						Debug.LogError("[MTConnect] Invalid AxisType!");
						break;
				}
			}
			
        }
    }

	/* Private Methods */
	private class PositionTimeStampCompare : IComparer<Position> {
		public int Compare(Position x, Position y) {
			if (x == null || y == null)
				return 0;
			Debug.Log("xT: " + x.Timestamp + ", " +
				"yT: " + y.Timestamp + ", " +
				x.Timestamp.CompareTo(y.Timestamp));
			return x.Timestamp.CompareTo(y.Timestamp);
		}
	}

	private class AngleTimeStampCompare : IComparer<Angle> {
		public int Compare(Angle x, Angle y) {
			if (x == null || y == null)
				return 0;
			Debug.Log("xT: " + x.Timestamp + ", " +
				"yT: " + y.Timestamp + ", " +
				x.Timestamp.CompareTo(y.Timestamp));
			return x.Timestamp.CompareTo(y.Timestamp);
		}
	}

}

#region XMLSerialization

namespace MTConnectStreamsXML {
    
    	[XmlRoot(ElementName="Header", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class Header {
		[XmlAttribute(AttributeName="creationTime")]
		public string CreationTime { get; set; }
		[XmlAttribute(AttributeName="sender")]
		public string Sender { get; set; }
		[XmlAttribute(AttributeName="instanceId")]
		public string InstanceId { get; set; }
		[XmlAttribute(AttributeName="version")]
		public string Version { get; set; }
		[XmlAttribute(AttributeName="bufferSize")]
		public string BufferSize { get; set; }
		[XmlAttribute(AttributeName="nextSequence")]
		public string NextSequence { get; set; }
		[XmlAttribute(AttributeName="firstSequence")]
		public string FirstSequence { get; set; }
		[XmlAttribute(AttributeName="lastSequence")]
		public string LastSequence { get; set; }
	}

	[XmlRoot(ElementName="MachineState", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class MachineState {
		[XmlAttribute(AttributeName="dataItemId")]
		public string DataItemId { get; set; }
		[XmlAttribute(AttributeName="timestamp")]
		public string Timestamp { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="sequence")]
		public string Sequence { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="Events", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class Events {
		[XmlElement(ElementName="MachineState", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public List<MachineState> MachineState { get; set; }
		[XmlElement(ElementName="AssetChanged", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public AssetChanged AssetChanged { get; set; }
		[XmlElement(ElementName="AssetRemoved", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public AssetRemoved AssetRemoved { get; set; }
		[XmlElement(ElementName="Availability", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public List<Availability> Availability { get; set; }
		[XmlElement(ElementName="RemoteState", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public RemoteState RemoteState { get; set; }
	}

	[XmlRoot(ElementName="ComponentStream", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class ComponentStream {
		[XmlElement(ElementName="Events", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public Events Events { get; set; }
		[XmlAttribute(AttributeName="component")]
		public string Component { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="componentId")]
		public string ComponentId { get; set; }
		[XmlElement(ElementName="Samples", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public Samples Samples { get; set; }
	}

	[XmlRoot(ElementName="AssetChanged", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class AssetChanged {
		[XmlAttribute(AttributeName="dataItemId")]
		public string DataItemId { get; set; }
		[XmlAttribute(AttributeName="timestamp")]
		public string Timestamp { get; set; }
		[XmlAttribute(AttributeName="sequence")]
		public string Sequence { get; set; }
		[XmlAttribute(AttributeName="assetType")]
		public string AssetType { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="AssetRemoved", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class AssetRemoved {
		[XmlAttribute(AttributeName="dataItemId")]
		public string DataItemId { get; set; }
		[XmlAttribute(AttributeName="timestamp")]
		public string Timestamp { get; set; }
		[XmlAttribute(AttributeName="sequence")]
		public string Sequence { get; set; }
		[XmlAttribute(AttributeName="assetType")]
		public string AssetType { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="Availability", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class Availability {
		[XmlAttribute(AttributeName="dataItemId")]
		public string DataItemId { get; set; }
		[XmlAttribute(AttributeName="timestamp")]
		public string Timestamp { get; set; }
		[XmlAttribute(AttributeName="sequence")]
		public string Sequence { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="Angle", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class Angle {
		[XmlAttribute(AttributeName="dataItemId")]
		public string DataItemId { get; set; }
		[XmlAttribute(AttributeName="timestamp")]
		public string Timestamp { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="sequence")]
		public string Sequence { get; set; }
		[XmlAttribute(AttributeName="subType")]
		public string SubType { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="Torque", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class Torque {
		[XmlAttribute(AttributeName="dataItemId")]
		public string DataItemId { get; set; }
		[XmlAttribute(AttributeName="timestamp")]
		public string Timestamp { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="sequence")]
		public string Sequence { get; set; }
		[XmlAttribute(AttributeName="subType")]
		public string SubType { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="Samples", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class Samples {
		[XmlElement(ElementName="Angle", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public List<Angle> Angle { get; set; }
		[XmlElement(ElementName="Torque", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public List<Torque> Torque { get; set; }
		[XmlElement(ElementName="Position", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public List<Position> Position { get; set; }
		[XmlElement(ElementName="AxisFeedrate", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public AxisFeedrate AxisFeedrate { get; set; }
		[XmlElement(ElementName="SoundLevel", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public SoundLevel SoundLevel { get; set; }
	}

	[XmlRoot(ElementName="Position", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class Position {
		[XmlAttribute(AttributeName="dataItemId")]
		public string DataItemId { get; set; }
		[XmlAttribute(AttributeName="timestamp")]
		public string Timestamp { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="sequence")]
		public string Sequence { get; set; }
		[XmlAttribute(AttributeName="subType")]
		public string SubType { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="DeviceStream", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class DeviceStream {
		[XmlElement(ElementName="ComponentStream", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public List<ComponentStream> ComponentStream { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="uuid")]
		public string Uuid { get; set; }
	}

	[XmlRoot(ElementName="AxisFeedrate", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class AxisFeedrate {
		[XmlAttribute(AttributeName="dataItemId")]
		public string DataItemId { get; set; }
		[XmlAttribute(AttributeName="timestamp")]
		public string Timestamp { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="sequence")]
		public string Sequence { get; set; }
		[XmlAttribute(AttributeName="subType")]
		public string SubType { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="RemoteState", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class RemoteState {
		[XmlAttribute(AttributeName="dataItemId")]
		public string DataItemId { get; set; }
		[XmlAttribute(AttributeName="timestamp")]
		public string Timestamp { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="sequence")]
		public string Sequence { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="SoundLevel", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class SoundLevel {
		[XmlAttribute(AttributeName="dataItemId")]
		public string DataItemId { get; set; }
		[XmlAttribute(AttributeName="timestamp")]
		public string Timestamp { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="sequence")]
		public string Sequence { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="Streams", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class Streams {
		[XmlElement(ElementName="DeviceStream", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public List<DeviceStream> DeviceStream { get; set; }
	}

	[XmlRoot(ElementName="MTConnectStreams", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
	public class MTConnectStreams {
		[XmlElement(ElementName="Header", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public Header Header { get; set; }
		[XmlElement(ElementName="Streams", Namespace="urn:mtconnect.org:MTConnectStreams:1.4")]
		public Streams Streams { get; set; }
		[XmlAttribute(AttributeName="m", Namespace="http://www.w3.org/2000/xmlns/")]
		public string M { get; set; }
		[XmlAttribute(AttributeName="xmlns")]
		public string Xmlns { get; set; }
		[XmlAttribute(AttributeName="xsi", Namespace="http://www.w3.org/2000/xmlns/")]
		public string Xsi { get; set; }
		[XmlAttribute(AttributeName="schemaLocation", Namespace="http://www.w3.org/2001/XMLSchema-instance")]
		public string SchemaLocation { get; set; }
	}

}

#endregion
