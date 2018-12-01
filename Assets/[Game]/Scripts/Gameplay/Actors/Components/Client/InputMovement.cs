using UnityEngine;

namespace Game.Gameplay.Actors.Components.Client
{
    // TODO: Split up input and movement
    public class InputMovement : ActorComponent
    {
        private float speed = .1f;

        protected override void Update()
        {
            base.Update();

            Vector3 position = transform.position;

            if (Input.GetKey(KeyCode.A))
            {
                position.x -= speed;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                position.z -= speed;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                position.x += speed;
            }
            else if (Input.GetKey(KeyCode.W))
            {
                position.z += speed;
            }

            transform.position = position;
        }
    }
}
