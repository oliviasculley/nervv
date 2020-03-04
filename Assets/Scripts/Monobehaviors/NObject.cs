// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// Base implementation of a NObject. These are automatically
    /// added to NObjectManager when they are initialized.
    /// </summary>
    public abstract class NObject : MonoBehaviour, INObject {
        #region NObject Properties
        /// <summary>Invoked when any field value is updated.</summary>
        public EventHandler OnNObjectUpdated { get; set; }

        [SerializeField, Header("Properties")]
        protected Transform _transform;
        /// <summary>Transform for object</summary>
        public Transform Transform {
            get => _transform;
            set => _transform = value;
        }

        [SerializeField]
        protected Collider _collider;
        /// <summary>Collider for object</summary>
        public Collider Collider {
            get => _collider;
            set => _collider = value;
        }

        [SerializeField]
        protected Mesh _mesh;
        /// <summary>Mesh for object</summary>
        public Mesh Mesh {
            get => _mesh;
            set => _mesh = value;
        }
        #endregion

        #region NObject Settings
        [SerializeField,
        Tooltip("Human-readable name of NObject"),
        Header("NObject Settings")]
        protected string _name;
        /// <summary>Human readable name of NObject</summary>
        /// <see cref="INObject"/>
        public virtual string Name {
            get => _name;
            set {
                _name = value;
                TriggerOnNObjectUpdated(this);
            }
        }

        [SerializeField,
        Tooltip("Individual ID, used for individual NObject identification and matching")]
        protected string _uuid;
        /// <summary>Individual ID, used for individual NObject identification and matching</summary>
        /// <see cref="INObject"/>
        public virtual string UUID {
            get => _uuid;
            set {
                _uuid = value;
                TriggerOnNObjectUpdated(this);
            }
        }

        public bool PrintDebugMessages = false;
        #endregion

        #region NObject References
        [SerializeField,
        Tooltip("If null, will attempt to use global reference"),
        Header("NObject References")]
        protected NObjectManager _nobjectManager;
        public NObjectManager NObjectManager {
            get {
                if (_nobjectManager == null) {
                    if (NObjectManager.Instances.Count > 0)
                        _nobjectManager = NObjectManager.Instances[0];
                    else
                        throw new ArgumentNullException("Could not get a ref to a NObjectManager!");
                }
                return _nobjectManager;
            }
            set => _nobjectManager = value;
        }
        #endregion

        #region Unity Methods
        /// <summary>Runs initial safety checks and adds self to NObjectManager</summary>
        /// <exception cref="Exception">Thrown if could not add self to NObjectManager</exception>
        protected virtual void OnEnable() {
            if (NObjectManager == null)
                throw new ArgumentNullException("NObjectManager is null!");
            if (!NObjectManager.AddNObject(this))
                throw new Exception("Could not add self to NObjectManager!");
        }
        #endregion

        #region Public Methods
        public void TriggerOnNObjectUpdated(object sender, EventArgs args = null) =>
            OnNObjectUpdated?.Invoke(sender, args);
        #endregion

        #region Methods
        protected void Log(string s) { if (PrintDebugMessages) Debug.Log($"<b>[{GetType()}]</b> " + s); }
        protected void LogWarning(string s) { if (PrintDebugMessages) Debug.LogWarning($"<b>[{GetType()}]</b> " + s); }
        protected void LogError(string s) { if (PrintDebugMessages) Debug.LogError($"<b>[{GetType()}]</b> " + s); }
        #endregion
    }
}