using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Patterns
{
	/// <summary>
	/// Struct for storing multiple commands that must be executed/undone in sequence.
	/// </summary>
	public readonly struct CommandSequence : ICommand
	{
		public readonly ICommand[] Commands;

		public int Length => Commands != null ? Commands.Length : 0;

		public string Name => $"CommandSequence[{Length}]";

		public CommandSequence(params ICommand[] commands)
		{
			Commands = commands;
		}

		/// <summary>
		/// Execute all sequenced commands in order.
		/// </summary>
		public void Execute()
		{
			for(int i = 0; i < Commands.Length; i++)
			{
				Commands[i].Execute();
			}
		}

		/// <summary>
		/// Undo all sequenced commands in reverse order.
		/// </summary>
		public void Undo()
		{
			for(int i = Commands.Length - 1; i >= 0; i--)
			{
				Commands[i].Undo();
			}
		}
	}
}