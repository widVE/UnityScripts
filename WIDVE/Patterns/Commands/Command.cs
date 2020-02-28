using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Patterns
{
	/// <summary>
	/// Base class for Commands.
	/// </summary>
	public abstract class Command : ICommand
	{
		public string Name => GetType().Name;

		public abstract void Execute();

		public abstract void Undo();
	}

	/// <summary>
	/// Command that targets a specific object.
	/// </summary>
	/// <typeparam name="T">Type of object.</typeparam>
	public abstract class Command<T> : Command
	{
		readonly protected T _target;
		/// <summary>
		/// The object this command is attached to.
		/// </summary>
		public T Target => _target;

		public Command(T target)
		{
			_target = target;
		}
	}
}