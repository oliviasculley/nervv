using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Machine : MonoBehaviour, IMachine {

    [Header("Machine Properties")]
    public List<Axis> _axes;
    /// <summary>Machine axes of possible movement/rotation</summary>
    public virtual List<Axis> Axes {
        get { return _axes; }
        set { _axes = value; }
    }

    [Header("Machine Settings")]
    [Tooltip("Max speed of machine")]
    [SerializeField] protected float _maxSpeed;
    /// <summary>Max speed of machine</summary>
    public virtual float MaxSpeed {
        get { return _maxSpeed; }
        set { _maxSpeed = value; }
    }


    [Tooltip("Name of Machine")]
    [SerializeField] protected string _name;
    /// <summary>Name of Machine</summary>
    public virtual string Name {
        get { return _name; }
        set { _name = value; }
    }

    [Tooltip("Individual ID, used for individual machine identification and matching")]
    [SerializeField] protected string _uuid;
    /// <summary>Individual ID, used for individual machine identification and matching</summary>
    public virtual string UUID {
        get { return _uuid; }
        set { _uuid = value; }
    }

    [Tooltip("Name of machine manufacturer")]
    [SerializeField] protected string _manufacturer;
    /// <summary>Name of machine manufacturer</summary>
    public virtual string Manufacturer {
        get { return _manufacturer; }
        set { _manufacturer = value; }
    }

    [Tooltip("Model of machine")]
    [SerializeField] protected string _model;
    /// <summary>Model of machine</summary>
    public virtual string Model {
        get { return _model; }
        set { _model = value; }
    }

    #region IMachine Public Methods

    /// <summary>
    /// Performs inverse kinematics on the machine by a small delta
    /// </summary>
    /// <param name="targetPosition">Vector3 of target position in world space</param>
    public abstract void InverseKinematics(Vector3 targetPosition);

    #endregion

    #region Axis Class

    [System.Serializable]
    public class Axis {

        [Header("Properties")]

        /// <summary>Value of axis in external worldspace</summary>
        [SerializeField] protected float _externalValue;
        public virtual float ExternalValue {
            get {
                if (MaxDelta == 0)
                    return _externalValue;
                else
                    return _externalValue % MaxDelta;
            }
            set { _externalValue = value; }
        }

        /// <summary>Value of axis in Unity worldspace</summary>
        public virtual float Value {
            get {
                // Get value with offset and external value
                float value = (_externalValue + Offset) * ScaleFactor;

                // If rotary, keep between 0 and 360
                if (Type == AxisType.Rotary)
                    value = Mathf.Repeat(value, 360);

                // If maxDelta is set, utilize
                if (MaxDelta != 0)
                    value = Mathf.Clamp(value, -MaxDelta, MaxDelta);

                return value;
            }
            set { _externalValue += Value - value; }
        }

        [SerializeField] protected float _torque;
        /// <summary></summary>
        public virtual float Torque {
            get { return _torque; }
            set { _torque = value; }
        }

        [Header("Settings")]

        [Tooltip("ID of axis, used for matching")]
        [SerializeField] protected string _id;
        /// <summary>ID of axis, used for matching</summary>
        public virtual string ID {
            get { return _id; }
            set { _id = value; }
        }

        [Tooltip("Name of axis")]
        [SerializeField] protected string _name;
        /// <summary>Name of axis</summary>
        public virtual string Name {
            get { return _name; }
            set { _name = value; }
        }

        [Tooltip("Short description of axis")]
        [SerializeField] protected string _desc;
        /// <summary>Short description of axis</summary>
        public virtual string Description {
            get { return _desc; }
            set { _desc = value; }
        }

        [Tooltip("Maximum allowed deviation")]
        [SerializeField] protected float _maxDelta;
        /// <summary>Minimum allowed value</summary>
        public virtual float MaxDelta {
            get { return _maxDelta; }
            set { _maxDelta = value; }
        }

        [Tooltip("Offset used to correct from an external worldspace to Unity's worldspace")]
        [SerializeField] protected float _offset;
        /// <summary>Offset to add to external values</summary>
        public virtual float Offset {
            get { return _offset; }
            set { _offset = value; }
        }

        [Tooltip("Scale used to correct from an external worldspace to Unity's worldspace")]
        [SerializeField] protected float _scaleFactor = 1f;
        /// <summary>Scale used to correct between an external worldspace and Unity's worldspace</summary>
        public virtual float ScaleFactor {
            get { return _scaleFactor; }
            set { _scaleFactor = value; }
        }

        public enum AxisType { Rotary, Linear, None }
        [Tooltip("Type of axis")]
        [SerializeField] protected AxisType _type;
        /// <summary></summary>
        public virtual AxisType Type {
            get { return _type; }
            set { _type = value; }
        }

        [Tooltip("Vector3 Location/Rotation in Unity worldspace. Scales with Value")]
        [SerializeField] protected Vector3 _axisVector3;
        /// <summary>Vector3 Location/Rotation in Unity worldspace. Scales with Value</summary>
        public virtual Vector3 AxisVector3 {
            get { return _axisVector3 * Value; }
            set { _axisVector3 = value.normalized; }
        }
    }

    #endregion
}