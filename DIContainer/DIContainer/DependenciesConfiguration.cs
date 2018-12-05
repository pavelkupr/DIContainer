using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIContainer
{
    public class DependenciesConfiguration
    {
		internal List<KeyValuePair<Type, Type>> Pairs { get; }
		internal List<KeyValuePair<Type, Type>> SingletonPairs { get; }

		public DependenciesConfiguration()
		{
			Pairs = new List<KeyValuePair<Type, Type>>();
			SingletonPairs = new List<KeyValuePair<Type, Type>>();
		}

		public void Registrate<T1, T2>()
			where T1 : class
			where T2 : class
		{
			KeyValuePair<Type, Type> pair = new KeyValuePair<Type, Type>(typeof(T1), typeof(T2));
			if (!Pairs.Exists(x=> x.Key == pair.Key && x.Value == pair.Value) && !SingletonPairs.Exists(x => x.Key == pair.Key && x.Value == pair.Value))
				Pairs.Add(pair);
		}

		public void RegistrateAsSingleton<T1, T2>()
			where T1 : class
			where T2 : class
		{
			KeyValuePair<Type, Type> pair = new KeyValuePair<Type, Type>(typeof(T1), typeof(T2));
			if (!Pairs.Exists(x => x.Key == pair.Key && x.Value == pair.Value) && !SingletonPairs.Exists(x => x.Key == pair.Key && x.Value == pair.Value))
				SingletonPairs.Add(pair);
		}
	}
}
