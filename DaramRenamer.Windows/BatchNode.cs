using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace DaramRenamer
{
	public class BatchNode
	{
		public ICommand Command { get; set; }
		public ICondition Condition { get; set; }

		public ObservableCollection<BatchNode> Children { get; } = new ObservableCollection<BatchNode>();

		public override string ToString()
		{
			var kind = (Condition == null ? "BatchWindow_Command" : "BatchWindow_Condition");
			var target = ((object) Command ?? (object) Condition);
			var type = target?.GetType();
			var localizationKey = type?.GetCustomAttribute<LocalizationKeyAttribute>()?.LocalizationKey;

			var args = new StringBuilder("(");
			foreach (var propInfo in type.GetProperties())
			{
				var propLocalizationKey = propInfo.GetCustomAttribute<LocalizationKeyAttribute>();
				if (propLocalizationKey == null)
					continue;
				args.AppendFormat("{0} = {1}, ", Strings.Instance[propLocalizationKey.LocalizationKey], propInfo.GetValue(target));
			}

			if (args.Length > 1)
			{
				args.Remove(args.Length - 2, 2);
				args.Append(")");
			}
			else args.Clear();

			return $"{Strings.Instance[kind]}: {Strings.Instance[localizationKey]}{args.ToString()}";
		}

		public void Execute(FileInfo fileInfo)
		{
			Command?.DoCommand(fileInfo);
			if(Condition != null && !Condition.IsSatisfyThisCondition (fileInfo))
				return;

			foreach (var node in Children)
				node.Execute(fileInfo);
		}
	}

	public class RootBatchNode : BatchNode
	{
		public override string ToString() => Strings.Instance["BatchWindow_Title"];
	}
}
