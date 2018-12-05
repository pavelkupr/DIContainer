using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DIContainer
{
	public class DependencyProvider
	{
		private DependencyValidator validator;
		private Dictionary<Type, Type> pairs;
		private List<Type> singleton;

		public DependencyProvider(DependenciesConfiguration config)
		{
			pairs = new Dictionary<Type, Type>(config.Pairs);
			singleton = new List<Type>(config.Singleton);
			validator = new DependencyValidator();
			if (!validator.Validate(config))
				throw new ArgumentException("Wrong configuration");
		}

		public T Resolve<T>()
			where T : class
		{
			object result;
			if (!singleton.Contains(typeof(T)))
			{
				result = Create(typeof(T));
				if (result == null)
					throw new ArgumentException();
				else
					return (T)result;
			}
			return null;
		}

		private object Create(Type type)
		{
			
			
			object result = null;
			Type typeForCreate = GetCreateType(type);
			if (typeForCreate == null)
				return null;
			List<Type> bannedTypes = new List<Type>();
			bannedTypes.Add(typeForCreate);
			foreach (ConstructorInfo constructorInfo in typeForCreate.GetConstructors())
			{
				result = CallConstructor(constructorInfo, bannedTypes);
				if (result != null)
					break;
			}
			
			return result;
		}
		private Type GetCreateType(Type type, bool isFirst = true)
		{
			foreach (KeyValuePair<Type, Type> pair in pairs)
			{
				if (pair.Key == type)
					return pair.Value != type ? GetCreateType(pair.Value, false) : pair.Value;
			}

			return isFirst ? null : type;
		}

		private object CallConstructor(ConstructorInfo constructor, List<Type> bannedTypes)
		{
			ParameterInfo[] parametersInfo = constructor.GetParameters();
			object[] parameters = new object[parametersInfo.Length];
			int i = 0;
			foreach (var parameterInfo in parametersInfo)
			{
				object result = null;
				List<Type> curr;
				Type type = GetCreateType(parameterInfo.ParameterType);

				if (type == null || bannedTypes.Contains(type))
					return null;

				foreach (ConstructorInfo constructorInfo in type.GetConstructors())
				{
					curr = new List<Type>(bannedTypes);
					curr.Add(type);
					result = CallConstructor(constructorInfo, curr);
					if (result != null)
						break;
				}
				parameters[i] = result;
				i++;
			}

			return constructor.Invoke(parameters);
		}
	}
}
