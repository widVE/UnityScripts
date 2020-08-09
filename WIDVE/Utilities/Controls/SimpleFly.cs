using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Utilities
{
	public class SimpleFly : MonoBehaviour
	{
		[SerializeField]
		Transform _flyer;
		Transform Flyer => _flyer;

		[SerializeField]
		Transform _orientation;
		Transform Orientation => _orientation;

		[SerializeField]
		[Range(0, 100)]
		float _speed = 10f;
		float Speed => _speed;

		[SerializeField]
		ButtonVector2 _button;
		ButtonVector2 Button => _button;

		void Move()
		{
			if (!Flyer) return;
			if (!Orientation) return;
			if (Mathf.Approximately(Speed, 0f)) return;
			if (!Button) return;

			Vector2 distance = Button.GetValue() * Speed * Time.deltaTime;
			Vector3 forwardDistance = Orientation.forward * distance.y;
			Vector3 rightDistance = Orientation.right * distance.x;

			Flyer.position += forwardDistance + rightDistance;
		}

		void Update()
		{
			Move();
		}
	}
}