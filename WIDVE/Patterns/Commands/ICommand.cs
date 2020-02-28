using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Patterns
{
	/// <summary>
	/// Interface used for commands.
	/// </summary>
	public interface ICommand
	{
		/// <summary>
		/// Name of the command.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Execute the command.
		/// </summary>
		void Execute();

		/// <summary>
		/// Undo the command.
		/// </summary>
		void Undo();
	}
}