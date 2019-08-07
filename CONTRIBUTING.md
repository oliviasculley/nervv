Thanks for your interest in contributing to NERVV!

If you are submitting code, please try to copy the current coding style:
* The main regions are Static, Properties, Settings, References, Vars, Unity Methods, Public Methods, and Methods.
These sections should also have a `[Header("Properties")]` attribute in front of the first variable in that region.
Additional regions can be used for situations such as convenience classes and child class Properties, Settings and References (where the region and header should say `[Header("Classname Properties")]` in order to distinguish itself from the parent properties.
    * Static is used for any static fields or declarations like enums or readonly reference variables.
    * Properties are considered as any public variables that should be accessible to other scripts but otherwise change according to the object's state. If you don't want the property to be externally modifiable, please use automatic getter/setter methods that reference a private variable with the `[SerializeField]` attribute.
    * Settings are parameters that are used when the object is initialized. These are normally set in the Unity Editor.
    * References are direct Unity object references that are set in the Unity Editor. Make sure to check these references as early as possible with Debug.Assert() in Awake(), Start(), or OnEnable()!
    * Vars are protected and private variables that are not publically available.
    * Unity Methods are any methods that are called via reflection in Unity.
    * Public Methods are any public methods that can be called from external contexts.
    * Methods are any protected and private methods.
* `[Tooltip("Explanation on hover in the Unity Editor")]` attribute is good practice for all Unity Editor-visible fields.
* Please try to use summaries blocks for methods as much as possible in order to allow for collapsing the method body.
* Using statements should be separated into logical groupings.
* Keep in mind that summaries need to be before attributes in order to work properly, and should be one line when they can!

As always, all parts of this project accepts [merge requests](https://gitlab.com/csculley/nervv/merge_requests), so please don't hesitate to send your suggestions!
Thanks!