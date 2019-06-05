﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Machine : MonoBehaviour
{
    public enum AxisType { Rotary, Linear, None }

    public class Axis {
        [SerializeField] private string id, name;
        [SerializeField] private float value, minValue, maxValue;
        [SerializeField] private AxisType type;

        public Axis(string id, string name) {
            this.id = id;
            this.name = name;
        }

        public Axis(string id,
                    string name,
                    float value,
                    AxisType type) : this(id, name) {
            this.value = value;
            this.type = type;
        }

        public Axis(string id,
                    string name,
                    float value,
                    AxisType type,
                    float minValue,
                    float maxValue) : this(id, name, value, type) {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public string GetID() {
            return id;
        }

        public string GetName() {
            return name;
        }

        public void SetValue(float newValue) {
            if (minValue != maxValue)
                this.value = Mathf.Clamp(newValue, minValue, maxValue);
            else
                this.value = newValue;
        }

        public float GetValue() {
            return value;
        }

    }

    [Header("Machine Properties")]
    public List<Axis> axes;

    [Header("Machine Settings")]
    public int axisCount;           // Number of axes on machine
    public float maxSpeed;          // Max speed of machine
    public new string name;         // Individual ID
    public string   id,
                    uuid,
                    manufacturer,
                    model;


    /* Public Methods */

    /// <summary>
    /// Returns the Vector3 for the associated axis
    /// </summary>
    /// <param name="axis">Axis to get Vector3</param>
    /// <returns>Vector3 of axis value in local space</returns>
    public abstract Vector3 GetAxisVector3(Axis axis);
}