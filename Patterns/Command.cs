using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Patterns.States;

namespace WIDVE.Patterns.Commands
{
	/// <summary>
	/// Interface that identifies a command.
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
	/// Command that targets a specific class.
	/// </summary>
	/// <typeparam name="T">Type of component.</typeparam>
	public abstract class Command<T> : Command
	{
		protected T Target;

		public Command(T target)
		{
			Target = target;
		}
	}

	/// <summary>
	/// Command that sets the target to a new state.
	/// <para>Prefer to use a PushdownStateMachine, rather than this command type...</para>
	/// </summary>
	public abstract class TrackStateCommand<T> : Command<T> where T : IStateMachine
	{
		protected State i_State { get; private set; } = null;
		bool StateCached = false;

		public TrackStateCommand(T target) : base(target) { }

		/// <summary>
		/// Caches StateMachine's current state. Call this at the beginning of Execute.
		/// </summary>
		protected bool CacheState()
		{
			if (!StateCached)
			{
				i_State = Target.SM.CurrentState;
				StateCached = true;
				return true;
			}
			else return false;
		}

		/// <summary>
		/// Sets StateMachine back to the cached state. Call this at the end of Undo.
		/// <para>Does nothing if the initial state was never cached.</para>
		/// </summary>
		protected bool RestoreState()
		{
			if (StateCached)
			{
				Target.SM.SetState(i_State);
				return true;
			}
			else return false;
		}
	}

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
			for (int i = 0; i < Commands.Length; i++)
			{
				Commands[i].Execute();
			}
		}

		/// <summary>
		/// Undo all sequenced commands in reverse order.
		/// </summary>
		public void Undo()
		{
			for (int i = Commands.Length - 1; i >= 0; i--)
			{
				Commands[i].Undo();
			}
		}
	}
}