using System;

namespace obsidianUpdater
{
	public class ActionParameter
	{
		public static readonly string PREFIX = "-";

		public string Name { get; private set; }

		public ParameterType Type { get; private set; }
		public string DefaultValue { get; private set; }
		public string Alias { get; private set; }
		public bool IsRequired { get; private set; }
		public bool IsHidden { get; private set; }
		public string Help { get; private set; }
		public Func<string, string> Validator { get; private set; }

		public string Value { get; protected set; }

		public bool IsSet { get { return (Value != null); } }
		public bool HasValue { get { return !String.IsNullOrEmpty(Value); } }

		public ActionParameter(string name,
		                       ParameterType type = ParameterType.Value, string defaultValue = null,
		                       string alias = null, bool isHidden = false, bool isRequired = false,
		                       string help = null, Func<string, string> validator = null)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			Name = name;
			Type = type;
			DefaultValue = defaultValue;
			Alias = alias;
			IsRequired = isRequired;
			IsHidden = isHidden;
			Help = help;
			Validator = validator;
		}

		public virtual void Reset()
		{
			Value = DefaultValue;
		}

		public virtual void Handle(string value)
		{
			switch (Type) {
				case ParameterType.Value:
					if (value == null)
						throw new ArgumentException(String.Format("Parameter '{0}' requires a value.", Name));
					Value = value;
					break;
				case ParameterType.Enabled:
					if (value != null)
						throw new ArgumentException(String.Format("Parameter '{0}' requires no value.", Name));
					Value = String.Empty;
					break;
				case ParameterType.Both:
					Value = (value ?? String.Empty);
					break;
			}
			if (Validator != null) {
				var message = Validator(Value);
				if (message != null)
					throw new ArgumentException(String.Format(message, Name));
			}
		}
	}

	public enum ParameterType
	{
		Value,
		Enabled,
		Both
	}
}

