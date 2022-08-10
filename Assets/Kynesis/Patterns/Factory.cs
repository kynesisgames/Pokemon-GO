using UnityEngine;

namespace Kynesis.Patterns
{
    public abstract class Factory<TFactory, TElement> : Singleton<TFactory> 
        where TFactory : MonoBehaviour where TElement : Component
    {
        [SerializeField] protected TElement _prefab;
        public abstract TElement Create(Vector3 position = default, Quaternion rotation = default);
    }

    public class FactoryInstantiate<TFactory, TElement> : Factory<TFactory, TElement>
        where TFactory : MonoBehaviour where TElement : Component
    {
        public override TElement Create(Vector3 position = default, Quaternion rotation = default)
        {
            TElement element = Instantiate(_prefab, transform);
            element.transform.position = position;
            element.transform.rotation = rotation;
            
            return element;
        }
    }
}