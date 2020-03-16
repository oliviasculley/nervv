// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

namespace NERVV.Menu {
    public abstract class MenuPanel : MenuComponent {
        #region Unity Methods
        /// <summary>Add self to parent Menu on instantiation</summary>
        /// <remarks>This will be called by Menu even if the object is disabled!</remarks>
        public virtual void Awake() {
            // Try to find other instances
            if (Menu.MenuPanels.ContainsKey(GetType())) {
                if (Menu.MenuPanels[GetType()] != this)
                    throw new InvalidOperationException();
            } else {
                // Else add self
                Menu.MenuPanels.Add(GetType(), this);
            }
        }

        /// <summary>Remove self from parent menu on destroy</summary>
        protected void OnDestroy() {
            if (!Menu.MenuPanels.ContainsKey(GetType()))
                throw new InvalidOperationException();
            Debug.Assert(Menu.MenuPanels.Remove(GetType()));
        }
        #endregion
    }
}
