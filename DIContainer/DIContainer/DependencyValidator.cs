using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace DIContainer
{
	class DependencyValidator
	{

		private IEnumerable<Dependency> pairs;

		internal DependencyValidator()
		{
		}

		internal bool Validate(DependenciesConfiguration config)
		{
			pairs = config.Pairs.Concat(config.Pairs);
			foreach(Dependency dependency in pairs)
			{
				if (dependency.pair.Key != dependency.pair.Value && !dependency.pair.Key.IsAssignableFrom(dependency.pair.Value))
					return false;

				bool fl = false;
				Type typeForCreate = GetCreateType(dependency.pair.Value) ?? dependency.pair.Value;
				List<Type> bannedTypes = new List<Type>();
				bannedTypes.Add(typeForCreate); 
				foreach (ConstructorInfo constructorInfo in typeForCreate.GetConstructors())
				{
					fl = CheckConstructor(constructorInfo, bannedTypes);
					if (fl)
						break;
				}

				if (!fl)
					return false;
			}
			
			return true;
		}

		private Type GetCreateType(Type type, bool isFirst = true)
		{
			foreach (Dependency dependency in pairs)
			{
				if (dependency.pair.Key == type)
					return dependency.pair.Value != type ? GetCreateType(dependency.pair.Value, false) : dependency.pair.Value;
			}

			return isFirst ? null : type;
		}

		private bool CheckConstructor(ConstructorInfo constructor,List<Type> bannedTypes)
		{
			foreach (var parameterInfo in constructor.GetParameters())
			{
				bool fl = false;
				List<Type> curr;
				Type type = GetCreateType(parameterInfo.ParameterType);

				if (type == null || bannedTypes.Contains(type))
					return false;
		
				foreach (ConstructorInfo constructorInfo in type.GetConstructors())
				{
					curr = new List<Type>(bannedTypes);
					curr.Add(type);
					fl = CheckConstructor(constructorInfo, curr);
					if (fl)
						break;
				}
			}

			return true;
		}
	}
}
