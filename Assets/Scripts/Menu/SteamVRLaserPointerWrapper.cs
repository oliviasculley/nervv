using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR.Extras;

// Code from https://answers.unity.com/questions/1600790/how-to-implement-steamvr-laser-pointer.html
[RequireComponent(typeof(SteamVR_LaserPointer))]
public class SteamVRLaserPointerWrapper : MonoBehaviour
{
    [Header("References")]
    public Menu menu;

    // Private vars
    private SteamVR_LaserPointer steamVrLaserPointer;
    private bool clicking = false;

    private void Awake()
    {
        Debug.Assert(menu != null, "[LaserPointerWrapper] Could not get reference to main menu!");

        steamVrLaserPointer = gameObject.GetComponent<SteamVR_LaserPointer>();
        steamVrLaserPointer.PointerIn += OnPointerIn;
        steamVrLaserPointer.PointerOut += OnPointerOut;
        steamVrLaserPointer.PointerClick += OnPointerClick;
        steamVrLaserPointer.PointerDown += OnPointerDown;
        steamVrLaserPointer.PointerUp += OnPointerUp;

        clicking = false;
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

    private void OnPointerDown(object sender, PointerEventArgs e)
    {
        /*
        IPointerDownHandler clickHandler = e.target.GetComponent<IPointerDownHandler>();
        if (clickHandler != null)
            clickHandler.OnPointerDown(new PointerEventData(EventSystem.current));
        */
    }

    private void OnPointerUp(object sender, PointerEventArgs e)
    {
        /*
        IPointerUpHandler clickHandler = e.target.GetComponent<IPointerUpHandler>();
        if (clickHandler != null)
            clickHandler.OnPointerUp(new PointerEventData(EventSystem.current));
        */
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