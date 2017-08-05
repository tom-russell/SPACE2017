using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Extensions
{
    public static class InterfaceExtensions
    {
        public static Text TextByName(this MonoBehaviour uiScript, string name)
        {
            return uiScript.transform.ComponentFromChild<Text>(name);
        }

        public static Text TextByName(this GameObject gameObject, string name)
        {
            return gameObject.transform.ComponentFromChild<Text>(name);
        }

        public static InputField InputByName(this GameObject gameObject, string name)
        {
            return gameObject.transform.ComponentFromChild<InputField>(name);
        }

        public static void SetColour(this InputField input, Color colour)
        {
            input.transform.ComponentFromChild<Text>("Text").color = colour;
        }

        public static Button ButtonByName(this GameObject gameObject, string name)
        {
            return gameObject.transform.ComponentFromChild<Button>(name);
        }

        public static Button ButtonByName(this MonoBehaviour uiScript, string name)
        {
            return uiScript.transform.ComponentFromChild<Button>(name);
        }

        public static void SetText(this Button button, string text)
        {
            button.GetComponentInChildren<Text>().text = text;
        }
    }
}
