using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIDVE.Utilities;

namespace WIDVE.Patterns.Commands
{
	[CreateAssetMenu(fileName = nameof(CommandHistory), menuName = nameof(Patterns) + "/" + nameof(CommandHistory), order = UtilitiesEditor.ORDER)]
	public class CommandHistory : ScriptableObject
	{
		[SerializeField]
		bool _recording = true;
		/// <summary>
		/// When true, executed commands will be recorded in the undo history.
		/// </summary>
		public bool Recording
		{
			get => _recording;
			set => _recording = value;
		}

		[SerializeField][Tooltip("Print when commands are executed/undone")]
		bool PrintToConsole = false;

		Stack<ICommand> _undoHistory;
		/// <summary>
		/// Stores commands that have been undone.
		/// </summary>
		Stack<ICommand> UndoHistory => _undoHistory ?? (_undoHistory = new Stack<ICommand>());

		Stack<ICommand> _redoHistory;
		/// <summary>
		/// Stores commands that have been redone.
		/// </summary>
		Stack<ICommand> RedoHistory => _redoHistory ?? (_redoHistory = new Stack<ICommand>());

		/// <summary>
		/// Add the given command to the undo history.
		/// </summary>
		/// <param name="command">Command to record.</param>
		/// <param name="clearRedoHistory">Clear current redo history.</param>
		public void Record(ICommand command, bool clearRedoHistory=true)
		{
			UndoHistory.Push(command);
			if(clearRedoHistory && RedoHistory.Count > 0)
			{	//all current redos are now invalid
				RedoHistory.Clear();
			}
		}

		/// <summary>
		/// Execute and record the given command.
		/// </summary>
		/// <param name="command">Command to execute.</param>
		public void Execute(ICommand command)
		{
			if (command != null)
			{
				if (PrintToConsole) Debug.Log($"Executing {command.Name} command.");
				command.Execute();
				if (Recording) Record(command);
			}
			else
			{
				if (PrintToConsole) Debug.Log("Command is null; cannot execute.");
			}
		}

		/// <summary>
		/// Execute the command.
		/// <para>If <paramref name="commandHistory"/> is not null, record the command in <paramref name="commandHistory"/>.</para>
		/// </summary>
		/// <param name="command">Command to execute. Null commands are ignored.</param>
		/// <param name="commandHistory">CommandHistory to record command. If null, the command executes without being recorded.</param>
		public static void Execute(ICommand command, CommandHistory commandHistory)
		{
			if (commandHistory != null) commandHistory.Execute(command);
			else command?.Execute();
		}

		/// <summary>
		/// Undo the most recent command.
		/// </summary>
		/// <returns>Command that was undone.</returns>
		public ICommand Undo()
		{
			ICommand command = null;
			if(UndoHistory.Count > 0)
			{
				command = UndoHistory.Pop();
				if (command != null)
				{
					if (PrintToConsole) Debug.Log($"Undoing {command.Name} command.");
					command.Undo();
					RedoHistory.Push(command);
				}
				else
				{
					if (PrintToConsole) Debug.Log("Command is null; cannot undo.");
				}
			}
			return command;
		}

		/// <summary>
		/// Redo the most recent command.
		/// </summary>
		/// <returns>Command that was redone.</returns>
		public ICommand Redo()
		{
			ICommand command = null;
			if(RedoHistory.Count > 0)
			{
				command = RedoHistory.Pop();
				if(command != null)
				{
					if (PrintToConsole) Debug.Log($"Redoing {command.Name} command...");
					command.Execute();
					UndoHistory.Push(command);
				}
				else
				{
					if (PrintToConsole) Debug.Log("Command is null; cannot redo.");
				}
			}
			return command;
		}

		/// <summary>
		/// Undo all recorded commands.
		/// </summary>
		/// <returns>Number of commands undone.</returns>
		public int UndoAll()
		{
			int commandsUndone = 0;
			while(UndoHistory.Count > 0)
			{
				ICommand command = Undo();
				if(command != null)
				{
					commandsUndone++;
				}
			}
			return commandsUndone;
		}
		
		/// <summary>
		/// Redo all undone commands.
		/// </summary>
		/// <returns>Number of commands redone.</returns>
		public int RedoAll()
		{
			int commandsRedone = 0;
			while(RedoHistory.Count > 0)
			{
				ICommand command = Redo();
				if(command != null)
				{
					commandsRedone++;
				}
			}
			return commandsRedone;
		}

		/// <summary>
		/// Clear all undo entries.
		/// </summary>
		/// <returns>Number of commands cleared.</returns>
		public int ClearUndoHistory()
		{
			int entriesCleared = UndoHistory.Count;
			UndoHistory.Clear();
			return entriesCleared;
		}

		/// <summary>
		/// Clear all redo entries.
		/// </summary>
		/// <returns>Number of commands cleared.</returns>
		public int ClearRedoHistory()
		{
			int entriesCleared = RedoHistory.Count;
			RedoHistory.Clear();
			return entriesCleared;
		}

		/// <summary>
		/// Clear all undo and redo entries. 
		/// </summary>
		/// <returns>Number of entries cleared.</returns>
		public int ClearHistory()
		{
			int entriesCleared = 0;
			entriesCleared += ClearUndoHistory();
			entriesCleared += ClearRedoHistory();
			return entriesCleared;
		}
	}
}