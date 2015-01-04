using System;
using System.Collections.Generic;
using System.Linq;

namespace obsidianUpdater
{
	public abstract class ProgramAction : IEnumerable<ProgramAction>
	{
		private readonly IDictionary<string, ProgramAction> _actions = new Dictionary<string, ProgramAction>();

		public string Name { get; private set; }
		public ProgramAction Parent { get; private set; }
		public string FullName { get; private set; }
		public string FullNameWithoutRoot { get; private set; }
		public string[] Help { get; protected set; }

		public bool IsRoot { get { return (Parent != null); } }
		public bool HasSubActions { get { return (_actions.Count > 0); } }

		public ProgramAction this[string name] {
			get {
				ProgramAction action;
				return (_actions.TryGetValue(name.ToLowerInvariant(), out action) ? action : null);
			}
		}


		public ProgramAction(string name, ProgramAction parent)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			Name = name;
			Parent = parent;
			FullName = ((parent != null) ? (parent.FullName + " " + name) : name);
			FullNameWithoutRoot = (((parent != null) && !parent.IsRoot) ? (parent.FullNameWithoutRoot + " " + name) : name);
		}


		protected void AddSubAction(ProgramAction action)
		{
			_actions[action.Name] = action;
		}

		public virtual void Handle(Stack<string> args)
		{
			if (!HasSubActions)
				throw new NotImplementedException(String.Format("Action '{0}' is not implemented yet.", FullNameWithoutRoot));
			if (args.Count <= 0)
				throw new InvalidUsageException(this, "Missing an action.");
			string name = args.Pop();
			ProgramAction action;
			if ((action = this[name]) == null)
				throw new InvalidUsageException(this, String.Format("Unknown action '{0}'.", name.ToLowerInvariant()));
			action.Handle(args);
		}

		public virtual string GetShortHelp()
		{
			return (HasSubActions ? Name + " " + String.Join("|", this.Select(a => a.Name)) : Name);
		}

		public virtual string GetLongHelp()
		{
			return (GetShortHelp() + ((!HasSubActions && (Help != null) && (Help.Length == 1)) ? (" => " + Help[0]) : ""));
		}

		public virtual string GetUsingHelp()
		{
			return (Parent.IsRoot ? (Parent.FullName + " ") : "") + GetShortHelp();
		}


		#region IEnumerable implementation

		public IEnumerator<obsidianUpdater.ProgramAction> GetEnumerator()
		{
			return _actions.Values.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion


		public class InvalidUsageException : Exception
		{
			public ProgramAction Action { get; private set; }

			public InvalidUsageException(ProgramAction action, string message) : base(message)
			{
				Action = action;
			}
		}
	}
}

