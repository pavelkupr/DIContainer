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
		private List<KeyValuePair<Type, Type>> pairs;
		private List<KeyValuePair<Type, Type>> singletonPairs;
		private Dictionary<KeyValuePair<Type, Type>, object> singletonResults;
		private readonly object locker;

		public DependencyProvider(DependenciesConfiguration config)
		{
			locker = new object();
			singletonResults = new Dictionary<KeyValuePair<Type, Type>, object>();
			pairs = new List<KeyValuePair<Type, Type>>(config.Pairs);
			singletonPairs = new List<KeyValuePair<Type, Type>>(config.SingletonPairs);
			validator = new DependencyValidator();
			if (!validator.Validate(config))
				throw new ArgumentException("Wrong configuration");
		}

		public T Resolve<T>()
			where T : class
		{
			foreach (KeyValuePair<Type, Type> pair in pairs.Concat(singletonPairs))
			{
				if (pair.Key == typeof(T))
				{
					return (T)Generate(pair);
				}
			}
			return null;
			
		}

		public IEnumerable<T> ResolveAll<T>()
			where T : class
		{
			List<T> result = new List<T>();
			foreach (KeyValuePair<Type, Type> pair in pairs.Concat(singletonPairs))
			{
				if (pair.Key == typeof(T))
				{
					result.Add((T)Generate(pair));
				}
			}

			return result;
		}

		private object Generate(KeyValuePair<Type, Type> currPair)
		{
			if (pairs.Exists(x => x.Key == currPair.Key && x.Value == currPair.Value))
			{
				return Create(currPair);

			}
			else if (singletonPairs.Exists(x => x.Key == currPair.Key && x.Value == currPair.Value))
			{
				object result;

				lock (locker)
				{
					if (singletonResults.Keys.ToList().Exists(x => x.Key == currPair.Key && x.Value == currPair.Value))
					{
						singletonResults.TryGetValue(currPair, out result);
					}
					else
					{
						result = Create(currPair);
						singletonResults.Add(currPair, result);
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
			foreach (KeyValuePair<Type, Type> pair in pairs.Concat(singletonPairs))
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
