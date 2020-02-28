using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIDVE.Patterns
{
	/// <summary>
	/// Interface for classes that use a state machine.
	/// </summary>
	public interface IStateMachine
	{
		StateMachine SM { get; }
	}

	/// <summary>
	/// Interface for classes that use a FiniteStateMachine.
	/// </summary>
	public interface IFiniteStateMachine
	{
		FiniteStateMachine SM { get; }
	}

	/// <summary>
	/// Interface for classes that use a PushdownStateMachine.
	/// </summary>
	public interface IPushdownStateMachine
	{
		PushdownStateMachine SM { get; }
	}

	public abstract class StateMachine
	{
		/// <summary>
		/// The StateMachine is currently in this state.
		/// </summary>
		public abstract State CurrentState { get; protected set; }

		/// <summary>
		/// Called after a new state has been entered.
		/// </summary>
		public event State.StateChangedDelegate StateChanged;

		/// <summary>
		/// Set this to true to print debug data.
		/// </summary>
		public bool DebugPrint = false;

		/// <summary>
		/// Create a new StateMachine with empty initial state.
		/// </summary>
		public StateMachine()
		{
			SetState(State.Idle);
		}

		/// <summary>
		/// Create a new StateMachine that starts in the given state.
		/// </summary>
		/// <param name="initialState">State that will be set on creation.</param>
		public StateMachine(State initialState)
		{
			SetState(initialState);
		}

		/// <summary>
		/// Set CurrentState to a new value.
		/// </summary>
		/// <param name="newState">New state to enter.</param>
		/// <param name="forceChange">Enter and exit even if newState == CurrentState.</param>
		public abstract void SetState(State newState, bool forceChange = false);

		/// <summary>
		/// Calls the Update method on the current state.
		/// </summary>
		public void UpdateState()
		{
			CurrentState?.Update();
		}

		/// <summary>
		/// Notify that the current state has changed.
		/// </summary>
		/// <param name="state">The new current state.</param>
		protected void InvokeStateChanged(State state)
		{
			StateChanged?.Invoke(state);
		}
	}

	/// <summary>
	/// StateMachine that stores a single state at a time.
	/// </summary>
	public class FiniteStateMachine : StateMachine
	{
		State _currentState;
		public override State CurrentState
		{
			get => _currentState;
			protected set => _currentState = value;
		}

		public FiniteStateMachine() : base() { }

		public FiniteStateMachine(State initialState) : base(initialState) { }

		public override void SetState(State newState, bool forceChange=false)
		{	//enter and exit only if current state will change
			if (forceChange || CurrentState != newState)
			{
				string currentStateName = CurrentState == null ? "null state" : CurrentState.GetType().Name;

				if (DebugPrint) Debug.Log("Exiting " + currentStateName);
				CurrentState?.Exit();

				CurrentState = newState;

				if (DebugPrint) Debug.Log("Entering " + CurrentState.GetType().Name);
				CurrentState?.Enter();

				//invoke StateChanged *after* new state has finished entering
				InvokeStateChanged(CurrentState);
			}
		}
	}

	/// <summary>
	/// StateMachine that stores a stack of past states.
	/// </summary>
	public class PushdownStateMachine : StateMachine
	{
		Stack<State> _stateStack;
		/// <summary>
		/// Keeps a history of states that have been entered and exited.
		/// </summary>
		Stack<State> StateStack => _stateStack ?? (_stateStack = new Stack<State>());

		/// <summary>
		/// The state used to initialize this state machine.
		/// </summary>
		State BaseState;

		/// <summary>
		/// Returns true if the current state is the base state.
		/// </summary>
		bool AtBaseState => StateStack.Count <= 1;

		public override State CurrentState
		{
			get => (StateStack.Count > 0) ? StateStack.Peek() : null;
			protected set { /* CurrentState is 'set' whenever a new state is pushed onto the stack */ }
		}

		public PushdownStateMachine() : base()
		{
			BaseState = State.Idle;
		}

		public PushdownStateMachine(State initialState) : base(initialState)
		{
			BaseState = initialState;
		}

		public override void SetState(State newState, bool forceChange = false)
		{   //push the new state onto the stack
			if (forceChange || CurrentState != newState)
			{
				CurrentState?.Exit();

				StateStack.Push(newState);

				CurrentState?.Enter();

				InvokeStateChanged(CurrentState);
			}
		}

		/// <summary>
		/// Exit the current state and enter the previous state.
		/// <para>The base state will not be popped.</para>
		/// </summary>
		/// <param name="finalPop">Optional: states will be popped until reaching and popping this state. This effectively enters the state just before this one.</param>
		/// <returns>The final popped state. Trying to pop the base state will return null.</returns>
		public State PopState(State finalPop = null)
		{	//pop the current state from the stack and return to the previous state
			State poppedState = null;
			if(!AtBaseState)
			{  
				//pop the current state and exit
				poppedState = StateStack.Pop();
				poppedState?.Exit();

				if (finalPop != null)
				{   //pop all the intervening states until also popping the final state
					while (!AtBaseState && poppedState != finalPop)
					{	//don't need to enter and exit here
						poppedState = StateStack.Pop();
					}
				}

				//enter the new current state
				CurrentState?.Enter();

				InvokeStateChanged(CurrentState);
			}
			return poppedState;
		}

		/// <summary>
		/// Clear the state stack and return to the base state.
		/// </summary>
		public void Clear()
		{
			CurrentState?.Exit();
			StateStack.Clear();
			SetState(BaseState, true);
		}
	}
}