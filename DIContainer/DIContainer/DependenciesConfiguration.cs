using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIContainer
{
	internal struct Dependency
	{
		internal KeyValuePair<Type, Type> pair;
		internal bool isSingleton;

		internal Dependency(KeyValuePair<Type, Type> pair, bool isSingleton)
		{
			this.pair = pair;
			this.isSingleton = isSingleton;
		}
	}

    public class DependenciesConfiguration
    {
		internal List<Dependency> Dependencies { get; }

		public DependenciesConfiguration()
		{
			Dependencies = new List<Dependency>();
		}

		public void Registrate<T1, T2>()
			where T1 : class
			where T2 : class
		{
			Dependency dependency = new Dependency(new KeyValuePair<Type, Type>(typeof(T1), typeof(T2)), false);
			if (!Dependencies.Exists(x=> x.pair.Key == dependency.pair.Key && x.pair.Value == dependency.pair.Value))
				Dependencies.Add(dependency);
		}

		public void RegistrateAsSingleton<T1, T2>()
			where T1 : class
			where T2 : class
		{
			Dependency dependency = new Dependency(new KeyValuePair<Type, Type>(typeof(T1), typeof(T2)), true);
			if (!Dependencies.Exists(x => x.pair.Key == dependency.pair.Key && x.pair.Value == dependency.pair.Value))
				Dependencies.Add(dependency);
		}

		public void Registrate(Type type1, Type type2)
		{
			if ((type1.IsClass || type1.IsInterface) && (type2.IsClass || type2.IsInterface))
			{
				Dependency dependency = new Dependency(new KeyValuePair<Type, Type>(type1, type2), false);
				if (!Dependencies.Exists(x => x.pair.Key == dependency.pair.Key && x.pair.Value == dependency.pair.Value))
					Dependencies.Add(dependency);
			}
		}

		public void RegistrateAsSingleton(Type type1, Type type2)
		{
			if ((type1.IsClass || type1.IsInterface) && (type2.IsClass || type2.IsInterface))
			{
				Dependency dependency = new Dependency(new KeyValuePair<Type, Type>(type1, type2), true);
				if (!Dependencies.Exists(x => x.pair.Key == dependency.pair.Key && x.pair.Value == dependency.pair.Value))
					Dependencies.Add(dependency);
			}
		}
	}
}
