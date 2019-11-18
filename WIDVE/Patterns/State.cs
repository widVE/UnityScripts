namespace WIDVE.Patterns.States
{
	public abstract class State
	{
		static IdleState _idle;
		/// <summary>
		/// Empty state to use as a placeholder.
		/// </summary>
		public static IdleState Idle => _idle ?? (_idle = new IdleState());

		/// <summary>
		/// Delegate type used when states are changed.
		/// </summary>
		/// <param name="newState">The new state that has just been entered.</param>
		public delegate void StateChangedDelegate(State newState);

		/// <summary>
		/// Called when this state is set as the current state.
		/// </summary>
		public virtual void Enter() { }

		/// <summary>
		/// Should be called during Update by the StateMachine's parent behaviour.
		/// </summary>
		public virtual void Update() { }

		/// <summary>
		/// Called before another state is set as the current state.
		/// </summary>
		public virtual void Exit() { }
	}

	/// <summary>
	/// Empty state that holds no information and does nothing.
	/// </summary>
	public sealed class IdleState : State { }

	/// <summary>
	/// State linked to a specific object.
	/// </summary>
	/// <typeparam name="T">Type of the object this state is linked to.</typeparam>
	public abstract class State<T> : State
	{
		readonly protected T _target;
		/// <summary>
		/// The object that this state is linked to.
		/// </summary>
		public T Target => _target;

		public State(T target)
		{
			_target = target;
		}
	}
}