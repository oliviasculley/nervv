// System
using System;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// This class handles adding and removing machines from the scene.
    /// It also handles sending data to the machines. Static references
    /// to self are added in Awake(), so any calls to Instance must
    /// happen in Start() or later.
    /// </summary>
    public class NObjectManager : MonoBehaviour {
        #region Static
        protected static List<NObjectManager> _instances;
        public static List<NObjectManager> Instances {
            get {
                if (_instances == null)
                    _instances = new List<NObjectManager>() ??
                        throw new ArgumentNullException();
                return _instances;
            }
        }
        #endregion

        #region Properties
        [Tooltip("List of machines in scene"), Header("Properties")]
        protected List<INObject> _nobjects;
        /// <summary>List of machines in scene</summary>
        public List<INObject> NObjects {
            get {
                if (_nobjects == null)
                    _nobjects = new List<INObject>() ??
                        throw new ArgumentNullException();
                return _nobjects;
            }
        }

        public event EventHandler<NObjectEventArgs> OnNObjectAdded;
        public event EventHandler<NObjectEventArgs> OnNObjectRemoved;
        #endregion

        #region Settings
        [Header("Settings")]
        public string Name;
        public bool PrintDebugMessages = false;
        #endregion

        #region Unity Methods
        /// <summary>Add static ref to self and init vars</summary>
        protected virtual void Awake() {
            if (Instances.Contains(this))
                LogWarning("Reference already exists in Instances list!");
            Instances.Add(this);
        }

        /// <summary>Remove static ref to self</summary>
        protected virtual void OnDestroy() => Instances.Remove(this);
        #endregion

        #region Public Methods
        /// <summary>Adds machine to machines List, checks for duplicate machines</summary>
        /// <param name="nobject">NObject to add</param>
        /// <returns>Succesfully added?</returns>
        /// <seealso cref="INObject"/>
        public virtual bool AddNObject(INObject nobject) {
            if (NObjects.Contains(nobject))
                return false;
            NObjects.Add(nobject);
            return true;
        }

        /// <summary>Removes machine from machines list</summary>
        /// <param name="nobject">INobject to remove</param>
        /// <returns>Succesfully removed?</returns>
        public virtual bool RemoveNObject(INObject nobject) =>
            NObjects.Remove(nobject);

        /// <summary>Returns machines of same type</summary>
        /// <typeparam name="T">Type of machine to return</typeparam>
        /// <returns>List<Machine> of machines</returns>
        public virtual List<INObject> GetNObjects<T>() =>
            NObjects.FindAll(x => x.GetType() == typeof(T));

        /// <summary>Returns machines of same type</summary>
        /// <param name="type">Type of machine to return</param>
        /// <returns>List<Machine> of machines</returns>
        public virtual List<INObject> GetNObjects(string type) =>
            NObjects.FindAll(x => x.GetType().ToString() == type);
        #endregion

        #region Methods
        /// <summary>Convenience method to trigger OnNObjectAdded</summary>
        protected virtual void TriggerOnNObjectAdded(NObjectEventArgs eventArgs) =>
            OnNObjectAdded?.Invoke(this, eventArgs);

        /// <summary>Convenience method to trigger OnNObjectRemoved</summary>
        protected virtual void TriggerOnNObjectRemoved(NObjectEventArgs eventArgs) =>
            OnNObjectRemoved?.Invoke(this, eventArgs);

        protected void Log(string s) { if (PrintDebugMessages) Debug.Log($"<b>[{GetType()}]</b> " + s); }
        protected void LogWarning(string s) { if (PrintDebugMessages) Debug.LogWarning($"<b>[{GetType()}]</b> " + s); }
        protected void LogError(string s) { if (PrintDebugMessages) Debug.LogError($"<b>[{GetType()}]</b> " + s); }
        #endregion

        #region EventTrigger Class
        public class NObjectEventArgs : EventArgs {
            public INObject NObject;

            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="machine"/> is null
            /// </exception>
            public NObjectEventArgs(INObject nobject) =>
                NObject = nobject ?? throw new ArgumentNullException();
        }
        #endregion
    }
}