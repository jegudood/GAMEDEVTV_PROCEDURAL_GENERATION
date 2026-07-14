using System;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class InspectorButtonAttribute : Attribute
{
    public string Label { get; }
    public bool PlayModeOnly { get; }
    public bool EditModeOnly { get; }

    public InspectorButtonAttribute(
        string label = null,
        bool playModeOnly = false,
        bool editModeOnly = false)
    {
        Label = label;
        PlayModeOnly = playModeOnly;
        EditModeOnly = editModeOnly;
    }
}
