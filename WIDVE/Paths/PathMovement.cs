using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

namespace WIDVE.Paths
{
    public class PathMovement : MonoBehaviour
    {
        [SerializeField]
        ButtonVector2 _movementButton;
        public ButtonVector2 MovementButton
		{
			get { return _movementButton; }
			set { _movementButton = value; }
		}

        [SerializeField]
        PathPosition _position;
        public PathPosition Position => _position;

        [SerializeField]
        [Range(.1f, 5f)]
        float _speed = 1f;
        float Speed => _speed;

        void Move(float distance)
        {
            Position.SetDistance(Position.Distance + distance);
        }

        void Update()
        {
            if(MovementButton && Position)
            {
                Move(MovementButton.GetValue()[1] * Speed * Time.deltaTime);
            }
        }
    }
}