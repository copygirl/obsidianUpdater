using System;
using System.Collections.Generic;
using System.Linq;

namespace obsidianUpdater
{
	public abstract class ProgramAction : IEnumerable<ProgramAction>
	{
		private readonly ICollection<ProgramAction> _actions = new List<ProgramAction>();
		private readonly ICollection<ActionParameter> _parameters = new List<ActionParameter>();

		private readonly IDictionary<string, ProgramAction> _actionLookup = new Dictionary<string, ProgramAction>();
		private readonly IDictionary<string, ActionParameter> _parameterLookup = new Dictionary<string, ActionParameter>();

		public string Name { get; private set; }
		public ProgramAction Parent { get; private set; }
		public string FullName { get; private set; }
		public string FullNameWithoutRoot { get; private set; }

		public bool IsHidden { get; protected set; }
		public string[] Help { get; protected set; }
		public string[] Aliases { get; protected set; }

		public bool IsRoot { get { return (Parent != null); } }
		public bool HasSubActions { get { return (_actionLookup.Count > 0); } }
		public bool HasParameters { get { return (_parameterLookup.Count > 0); } }
		public bool HasRequiredParameters { get; private set; }

		public ProgramAction this[string name] {
			get {
				ProgramAction action;
				return (_actionLookup.TryGetValue(name.ToLowerInvariant(), out action) ? action : null);
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
			_actions.Add(action);
			_actionLookup[action.Name] = action;
			if (action.Aliases != null)
				foreach (var alias in action.Aliases)
					_actionLookup[alias] = action;
		}

		protected void AddParameter(ActionParameter parameter)
		{
			_parameters.Add(parameter);
			_parameterLookup[parameter.Name] = parameter;
			if (parameter.Alias != null)
				_parameterLookup[parameter.Alias] = parameter;
		}
		protected void AddParameters(params ActionParameter[] parameters)
		{
			foreach (var param in parameters)
				AddParameter(param);
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

		protected void HandleParameters(Stack<string> args)
		{
			var parameters = new List<ActionParameter>(_parameters);
			foreach (var param in parameters)
				param.Reset();
			
			while (args.Count > 0) {
				var parameterName = args.Pop();
				if (!parameterName.StartsWith(ActionParameter.PREFIX))
					throw new InvalidUsageException(this, String.Format("Expected a parameter, got '{0}'.", parameterName));
				parameterName = parameterName.Substring(1);

				ActionParameter parameter;
				if (!_parameterLookup.TryGetValue(parameterName, out parameter))
					throw new InvalidUsageException(this, String.Format("Unknown parameter '{0}'.", parameterName));

				if (!parameters.Remove(parameter))
					throw new InvalidUsageException(this, String.Format("Duplicate parameter '{0}'.", parameterName));

				var isNextValue = (args.Count > 0) && !args.Peek().StartsWith(ActionParameter.PREFIX);
				var nextValue = isNextValue ? args.Pop() : null;
				parameter.Handle(nextValue);
			}

			var missing = parameters.Where(param => param.IsRequired).Select(m => ActionParameter.PREFIX + m.Name);
			if (missing.Count() > 0)
				throw new InvalidUsageException(this, String.Format("Missing required parameter(s) {0}.", String.Join(", ", missing)));
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
			return _actions.GetEnumerator();
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

