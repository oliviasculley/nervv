using UnityEngine;
using UnityEngine.EventSystems;

using Valve.VR;
using Valve.VR.Extras;

// Code from https://answers.unity.com/questions/1600790/how-to-implement-steamvr-laser-pointer.html
[RequireComponent(typeof(SteamVR_LaserPointer))]
public class SteamVRLaserPointerWrapper : MonoBehaviour
{
    [Header("References")]
    public Menu menu;

    // Private vars
    private SteamVR_LaserPointer steamVrLaserPointer;

    private void Awake()
    {
        Debug.Assert(menu != null, "[LaserPointerWrapper] Could not get reference to main menu!");

        steamVrLaserPointer = gameObject.GetComponent<SteamVR_LaserPointer>();
        steamVrLaserPointer.PointerIn += OnPointerIn;
        steamVrLaserPointer.PointerOut += OnPointerOut;
        steamVrLaserPointer.PointerClick += OnPointerClick;
    }

    public void Update()
    {
        // Set laser pointers active if menu is visible
        transform.Find("New Game Object").gameObject.SetActive(steamVrLaserPointer.active = menu.visible);
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
    }

    private void OnPointerIn(object sender, PointerEventArgs e)
    {
        IPointerEnterHandler pointerEnterHandler = e.target.GetComponent<IPointerEnterHandler>();
        if (pointerEnterHandler != null)
            pointerEnterHandler.OnPointerEnter(new PointerEventData(EventSystem.current));
    }
}