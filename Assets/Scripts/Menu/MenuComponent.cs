// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;

namespace NERVV.Menu {
    public abstract class MenuComponent : MonoBehaviour {
        #region Properties
        public event EventHandler OnComponentLoad;
        public event EventHandler OnComponentClose;
        #endregion

        #region References
        [SerializeField, Header("References")]
        protected Menu _menu = null;
        public Menu Menu {
            get {
                if (_menu == null) {
                    _menu = GetComponentInParent<Menu>();
                    Debug.Assert(Menu != null);
                }
                return _menu;
            }
        }
        #endregion

        #region Unity Methods
        /// <summary>Invoke OnComponentEnable on component enabled</summary>
        protected virtual void OnEnable() {
            InvokeOnComponentLoad(null);
        }

        /// <summary>Invoke OnComponentDisable on component disabled</summary>
        protected virtual void OnDisable() {
            InvokeOnComponentDisable(null);
        }
        #endregion

        #region Methods
        protected virtual void InvokeOnComponentLoad(EventArgs e) {
            EventHandler handler = OnComponentLoad;
            handler?.Invoke(this, e);
        }

        protected virtual void InvokeOnComponentDisable(EventArgs e) {
            EventHandler handler = OnComponentClose;
            handler?.Invoke(this, e);
        }
        #endregion

        #region Exception Class
        public class DependencyException : Exception {
            public DependencyException() : this("Menu dependency was not satisified!") { }
            public DependencyException(string message) : base(message) { }
        }
        #endregion
    }

}
