// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NERVV.Menu {
    public class MachineElement : MenuComponent {
        #region Static
        /// <summary>Returns string with capitalized first letter</summary>
        /// <param name="input">string to be capitalized</param>
        /// <returns>capitalized string</returns>
        public static string CapitalizeFirstLetter(string input) {
            // If invalid string, return invalid string
            if (string.IsNullOrEmpty(input))
                return input;

            // If only one char, return uppercase char
            if (input.Length == 1)
                return input.ToUpper();

            // Else return capitalized first char with rest of string
            return input.Substring(0, 1).ToUpper() + input.Substring(1).ToLower();
        }
        #endregion

        #region Element Properties
        [Header("Element Properties")]
        public bool _visible;
        public bool Visible {
            get => _visible;
            set {
                buttons = buttons ?? GetComponentsInChildren<Button>()
                    ?? throw new ArgumentNullException();
                colliders = GetComponentsInChildren<BoxCollider>()
                    ?? throw new ArgumentNullException();
                triggers = GetComponentsInChildren<EventTrigger>()
                    ?? throw new ArgumentNullException();

                _visible = value;
                foreach (Button b in buttons) b.enabled = _visible;
                foreach (BoxCollider c in colliders) c.enabled = _visible;
                foreach (EventTrigger t in triggers) t.enabled = _visible;
            }
        }
        #endregion

        #region Vars
        protected Button[] buttons;
        protected BoxCollider[] colliders;
        protected EventTrigger[] triggers;
        #endregion

        #region Unity Methods
        /// <summary>Set element as visible on load</summary>
        protected override void OnEnable() {
            base.OnEnable();
            Visible = true;
        }
        #endregion

        #region Static Methods
        public static object GetMemberValue(MemberInfo memberInfo, object forObject) {
            switch (memberInfo.MemberType) {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).GetValue(forObject);
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetValue(forObject);
                default:
                    throw new NotImplementedException();
            }
        }

        public static Type GetMemberType(MemberInfo member) {
            switch (member.MemberType) {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException("Input MemberInfo must be if" +
                        " type EventInfo, FieldInfo, MethodInfo, or PropertyInfo");
            }
        }

        public static void SetMemberValue(MemberInfo memberInfo, object forObject, object value) {
            switch (memberInfo.MemberType) {
                case MemberTypes.Field:
                    ((FieldInfo)memberInfo).SetValue(forObject, value);
                    return;
                case MemberTypes.Property:
                    ((PropertyInfo)memberInfo).SetValue(forObject, value);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }
        #endregion
    }
}
