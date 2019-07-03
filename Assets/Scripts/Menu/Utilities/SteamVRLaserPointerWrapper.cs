using UnityEngine;
using UnityEngine.EventSystems;

using Valve.VR;
using Valve.VR.Extras;

// Code from https://answers.unity.com/questions/1600790/how-to-implement-steamvr-laser-pointer.html
[RequireComponent(typeof(SteamVR_LaserPointer))]
public class SteamVRLaserPointerWrapper : MonoBehaviour
{
    [Header("UI Scroll")]
    public SteamVR_Action_Vector2 scrollUI;

    [Header("References")]
    public Menu menu;

    // Private vars
    private SteamVR_LaserPointer steamVrLaserPointer;
    private bool clicking = false;
    private Transform lastTarget;

    private void Awake()
    {
        Debug.Assert(menu != null, "[LaserPointerWrapper] Could not get reference to main menu!");

        steamVrLaserPointer = gameObject.GetComponent<SteamVR_LaserPointer>();
        steamVrLaserPointer.PointerIn += OnPointerIn;
        steamVrLaserPointer.PointerOut += OnPointerOut;
        steamVrLaserPointer.PointerClick += OnPointerClick;

        clicking = false;
    }

    public void Update()
    {
        // Set laser pointers active if menu is visible
        transform.Find("New Game Object").gameObject.SetActive(steamVrLaserPointer.active = menu.visible);

        // Trigger on scroll
        if (lastTarget != null && scrollUI.active) {
            IScrollHandler sh = lastTarget.GetComponent<IScrollHandler>();
            if (sh != null) {
                PointerEventData data = new PointerEventData(EventSystem.current);
                data.delta = scrollUI.delta;
                sh.OnScroll(data);
            }
        }
    }

    private void OnPointerClick(object sender, PointerEventArgs e)
    {
        IPointerClickHandler clickHandler = e.target.GetComponent<IPointerClickHandler>();
        if (clickHandler != null)
            clickHandler.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    private void OnPointerOut(object sender, PointerEventArgs e)
    {
        IPointerExitHandler pointerExitHandler = e.target.GetComponent<IPointerExitHandler>();
        if (pointerExitHandler != null)
            pointerExitHandler.OnPointerExit(new PointerEventData(EventSystem.current));
        lastTarget = null;
    }

    private void OnPointerIn(object sender, PointerEventArgs e)
    {
        lastTarget = e.target;
        IPointerEnterHandler pointerEnterHandler = e.target.GetComponent<IPointerEnterHandler>();
        if (pointerEnterHandler != null)
            pointerEnterHandler.OnPointerEnter(new PointerEventData(EventSystem.current));
    }
}