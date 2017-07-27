using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public static  class EntityExtensions
    {
        public static NestManager NestManager(this GameObject gameObject)
        {
            return Component<NestManager>(gameObject);
        }

        public static AntManager AntManager(this GameObject gameObject)
        {
            return Component<AntManager>(gameObject);
        }

        public static AntMovement AntMovement(this GameObject gameObject)
        {
            return Component<AntMovement>(gameObject);
        }

        public static SimData AntData(this GameObject gameObject)
        {
            return Component<SimData>(gameObject);
        }

        public static T Component<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject == null)
                return null;              

            return (T)gameObject.GetComponent(typeof(T));
        }

        public static T ComponentFromChild<T>(this Transform transform, string childName) where T : Component
        {
            return transform.Find(childName).GetComponent<T>();
        }

        public static void ChangeColour(this Component component, Color colour)
        {
            component.GetComponentInChildren<Renderer>().material.color = colour;
        }
    }
}
