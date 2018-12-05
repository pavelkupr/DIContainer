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
		private List<Dependency> pairs;
		private Dictionary<KeyValuePair<Type, Type>, object> singletonResults;
		private readonly object locker;

		public DependencyProvider(DependenciesConfiguration config)
		{
			locker = new object();
			singletonResults = new Dictionary<KeyValuePair<Type, Type>, object>();
			pairs = new List<Dependency>(config.Pairs);
			validator = new DependencyValidator();
			if (!validator.Validate(config))
				throw new ArgumentException("Wrong configuration");
		}

		public T Resolve<T>()
			where T : class
		{
			foreach (Dependency dependency in pairs)
			{
				if (dependency.pair.Key == typeof(T))
				{
					return (T)Generate(dependency);
				}
			}
			return null;
			
		}

		public IEnumerable<T> ResolveAll<T>()
			where T : class
		{
			List<T> result = new List<T>();
			foreach (Dependency dependency in pairs)
			{
				if (dependency.pair.Key == typeof(T))
				{
					result.Add((T)Generate(dependency));
				}
			}

			return result;
		}

		private object Generate(Dependency dependency)
		{
			if (pairs.Exists(x => x.pair.Key == dependency.pair.Key && x.pair.Value == dependency.pair.Value) && !dependency.isSingleton)
			{
				return Create(dependency.pair);

			}
			else if (pairs.Exists(x => x.pair.Key == dependency.pair.Key && x.pair.Value == dependency.pair.Value) && dependency.isSingleton)
			{
				object result;

				lock (locker)
				{
					if (singletonResults.Keys.ToList().Exists(x => x.Key == dependency.pair.Key && x.Value == dependency.pair.Value))
					{
						singletonResults.TryGetValue(dependency.pair, out result);
					}
					else
					{
						result = Create(dependency.pair);
						singletonResults.Add(dependency.pair, result);
					}
				}

				return result;
			}
			return null;
		}

		private object Create(KeyValuePair<Type, Type> currPair)
		{
			object result = null;
			Type typeForCreate = GetCreateType(currPair.Value) ?? currPair.Value;
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
			foreach (Dependency dependency in pairs)
			{
				if (dependency.pair.Key == type)
					return dependency.pair.Value != type ? GetCreateType(dependency.pair.Value, false) : dependency.pair.Value;
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
