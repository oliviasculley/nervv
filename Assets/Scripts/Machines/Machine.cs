using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Machine : MonoBehaviour, IMachine {

    [Header("Machine Properties")]
        public List<Axis> _axes;
        public List<Axis> Axes {
            get { return _axes; }
            set { _axes = value; }
        }

    [Header("Machine Settings")]
        [Tooltip("Max speed of machine")]
        [SerializeField] private float _maxSpeed;
        public float MaxSpeed {
            get { return _maxSpeed; }
            set { _maxSpeed = value; }
        }

        [Tooltip("Individual ID")]
        [SerializeField] private string _name;
        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        [SerializeField] private string _uuid;
        public string UUID {
            get { return _uuid; }
            set { _uuid = value; }
        }

        [SerializeField] private string _manufacturer;
        public string Manufacturer {
            get { return _manufacturer; }
            set { _manufacturer = value; }
        }

        [SerializeField] private string _model;
        public string Model {
            get { return _model; }
            set { _model = value; }
        }

    #region IMachine Public Methods

    /// <summary>
    /// Returns the Vector3 for the associated axis
    /// </summary>
    /// <param name="axis">Axis to get Vector3</param>
    /// <returns>Vector3 of axis value in local space</returns>
    public abstract Vector3 GetAxisVector3(Machine.Axis axis);

    /// <summary>
    /// Sets the value of a certain axis by axis' ID. Use this function to apply offsets or mirrors to incoming values!
    /// </summary>
    /// <param name="axisID">Axis ID (MTConnect string identifier) to set</param>
    /// <param name="value">Value of axis to set</param>
    public abstract void SetAxisValue(string axisID, float value);

    /// <summary>
    /// Activate a small delta of inverse kinematics for the target position.
    /// </summary>
    /// <param name="targetPosition">Vector3 of target position in world space</param>
    public abstract void InverseKinematics(Vector3 targetPosition);

    #endregion

    #region Axis Class

    [System.Serializable]
    public class Axis {

        [Header("Properties")]
            [SerializeField] private float _value;
            public float Value {
                get { return _value; }
                set {
                    if (MinValue != MaxValue)
                        _value = Mathf.Clamp(
                            Mathf.Repeat(value, 360),
                            MinValue,
                            MaxValue
                        );
                    else
                        _value = Mathf.Repeat(value, 360);
                }
            }

            [SerializeField] private float _torque;
            public float Torque {
                get { return _torque; }
                set { _torque = value; }
            }

        [Header("Settings")]
            [SerializeField] private string _id;
            public string ID {
                get { return _id; }
                set { _id = value; }
            }

            [SerializeField] private string _name;
            public string Name {
                get { return _name; }
                set { _name = value; }
            }

            [SerializeField] private string _desc;
            public string Description {
                get { return _desc; }
                set { _desc = value; }
            }

            [SerializeField] private float _minValue;
            public float MinValue {
                get { return _minValue; }
                set { _minValue = value; }
            }

            [SerializeField] private float _maxValue;
            public float MaxValue {
                get { return _maxValue; }
                set { _maxValue = value; }
            }

            public enum AxisType { Rotary, Linear, None }
            [SerializeField] private AxisType _type;
            public AxisType Type {
                get { return _type; }
                set { _type = value; }
            }
    }

    #endregion
}