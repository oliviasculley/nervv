//System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NERVV.Menu {
    public class OutputsList : MonoBehaviour {
        #region References
        /// <summary>Parent to spawn machine buttons underneath</summary>
        [Tooltip("Parent to spawn machine buttons underneath"),
        Header("References")]
        public Transform scrollViewParent;
        public GameObject inputButtonPrefab;
        #endregion

        #region Unity Methods
        /// <summary>Check references and generate buttons OnEnable</summary>
        void OnEnable() {
            Debug.Assert(inputButtonPrefab != null);
            Debug.Assert(OutputManager.Instance != null);

            GenerateOutputButtons();
        }
        #endregion

        #region Public Methods
        /// <summary>Toggle output enabled</summary>
        /// <seealso cref="IOutputSource"/>
        public void ToggleOutputs(IOutputSource o) {
            o.OutputEnabled = !o.OutputEnabled;
            GenerateOutputButtons();
        }
        #endregion

        #region Methods
        void GenerateOutputButtons() {
            // Delete old objects
            foreach (Transform t in scrollViewParent.transform)
                Destroy(t.gameObject);

            // Spawn buttons for each machine
            GameObject g;
            foreach (IOutputSource o in OutputManager.Instance.Outputs) {
                g = Instantiate(inputButtonPrefab, scrollViewParent);

                g.transform.Find("InputName").GetComponent<TextMeshProUGUI>().text = o.Name;
                g.transform.Find("ToggleEnabled").GetComponent<Toggle>().isOn = o.OutputEnabled;

                // Push machine to stack so button works correctly
                IOutputSource buttonOutput = o;
                g.GetComponent<Button>().onClick.AddListener(delegate { ToggleOutputs(buttonOutput); });
            }
        }
        #endregion
    }
}