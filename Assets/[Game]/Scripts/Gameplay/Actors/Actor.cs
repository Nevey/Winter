using UnityEngine;

namespace Game.Gameplay.Actors
{
    public abstract class Actor : MonoBehaviour
    {
        // TODO: Create View/ActorView component
        [SerializeField] private GameObject view;

        protected virtual void Awake()
        {
            Instantiate(view, transform);
        }

        protected virtual void OnDestroy()
        {

        }
    }
}
