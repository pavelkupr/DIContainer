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
		internal List<Dependency> Pairs { get; }

		public DependenciesConfiguration()
		{
			Pairs = new List<Dependency>();
		}

		public void Registrate<T1, T2>()
			where T1 : class
			where T2 : class
		{
			Dependency dependency = new Dependency(new KeyValuePair<Type, Type>(typeof(T1), typeof(T2)), false);
			if (!Pairs.Exists(x=> x.pair.Key == dependency.pair.Key && x.pair.Value == dependency.pair.Value))
				Pairs.Add(dependency);
		}

		public void RegistrateAsSingleton<T1, T2>()
			where T1 : class
			where T2 : class
		{
			Dependency dependency = new Dependency(new KeyValuePair<Type, Type>(typeof(T1), typeof(T2)), true);
			if (!Pairs.Exists(x => x.pair.Key == dependency.pair.Key && x.pair.Value == dependency.pair.Value))
				Pairs.Add(dependency);
		}
	}
}
