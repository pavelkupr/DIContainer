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
		private List<Dependency> dependencies;
		private Dictionary<KeyValuePair<Type, Type>, object> singletonResults;
		private readonly object locker;

		public DependencyProvider(DependenciesConfiguration config)
		{
			locker = new object();
			singletonResults = new Dictionary<KeyValuePair<Type, Type>, object>();
			dependencies = new List<Dependency>(config.Dependencies);
			validator = new DependencyValidator();
			if (!validator.Validate(config))
				throw new ArgumentException("Wrong configuration");
		}

		public T Resolve<T>()
			where T : class
		{
			if (typeof(T).IsGenericTypeDefinition)
				return null;

			foreach (Dependency dependency in dependencies)
			{
				if (dependency.pair.Key == typeof(T))
				{
					return (T)Generate(GetCreateType(dependency.pair.Value) ?? dependency.pair.Value, dependency);
				}
			}

			if (typeof(T).IsGenericType)
			{
				foreach (Dependency dependency in dependencies)
				{
					if (dependency.pair.Key == typeof(T).GetGenericTypeDefinition())
					{
						try
						{
							Type generic = (GetCreateType(dependency.pair.Value) ?? dependency.pair.Value).MakeGenericType(typeof(T).GenericTypeArguments);
							return (T)Generate(generic, 
								new Dependency(new KeyValuePair<Type, Type>(typeof(T), dependency.pair.Value.MakeGenericType(typeof(T).GenericTypeArguments)), 
								dependency.isSingleton));
						}
						catch
						{
							return null;
						}
					}
				}
			}

			return null;
			
		}

		public IEnumerable<T> ResolveAll<T>()
			where T : class
		{
			if (typeof(T).IsGenericTypeDefinition)
				return null;

			List<T> result = new List<T>();
			foreach (Dependency dependency in dependencies)
			{
				if (dependency.pair.Key == typeof(T))
				{
					result.Add((T)Generate(GetCreateType(dependency.pair.Value) ?? dependency.pair.Value, dependency));
				}
			}

			if (typeof(T).IsGenericType)
			{
				foreach (Dependency dependency in dependencies)
				{
					if (dependency.pair.Key == typeof(T).GetGenericTypeDefinition())
					{
						try
						{
							Type generic = (GetCreateType(dependency.pair.Value) ?? dependency.pair.Value).MakeGenericType(typeof(T).GenericTypeArguments);
							result.Add((T)Generate(generic,
								new Dependency(new KeyValuePair<Type, Type>(typeof(T), dependency.pair.Value.MakeGenericType(typeof(T).GenericTypeArguments)),
								dependency.isSingleton)));
						}
						catch
						{
							return null;
						}
					}
				}
			}

			return result;
		}

		private object Generate(Type typeForGeneration, Dependency dependency)
		{
			if (!dependency.isSingleton)
			{
				return Create(typeForGeneration);
			}
			else if (dependency.isSingleton)
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
						result = Create(typeForGeneration);
						singletonResults.Add(dependency.pair, result);
					}
				}

				return result;
			}
			return null;
		}

		private object Create(Type typeForCreation)
		{
			object result = null;
			List<Type> bannedTypes = new List<Type>();
			bannedTypes.Add(typeForCreation);
			foreach (ConstructorInfo constructorInfo in typeForCreation.GetConstructors())
			{
				result = CallConstructor(constructorInfo, bannedTypes);
				if (result != null)
					break;
			}
			
			return result;
		}

		private Type GetCreateType(Type type, bool isFirst = true)
		{
			foreach (Dependency dependency in dependencies)
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
				Type[] genericArgs = null;

				if (type == null && parameterInfo.ParameterType.IsGenericType)
				{
					genericArgs = parameterInfo.ParameterType.GenericTypeArguments;
					type = GetCreateType(parameterInfo.ParameterType.GetGenericTypeDefinition());
				}

				if (type == null || bannedTypes.Contains(type))
					return null;

				if (type.IsGenericTypeDefinition)
				{
					try
					{
						type = type.MakeGenericType(genericArgs);
					}
					catch
					{
						return null;
					}
				}

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
