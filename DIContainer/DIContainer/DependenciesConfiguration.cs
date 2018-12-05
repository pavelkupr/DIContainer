using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIContainer
{
    public class DependenciesConfiguration
    {
		internal Dictionary<Type, Type> Pairs { get; }
		internal Dictionary<Type, Type> SingletonPairs { get; }

		public DependenciesConfiguration()
		{
			Pairs = new Dictionary<Type, Type>();
			SingletonPairs = new Dictionary<Type, Type>();
		}

		public void Registrate<T1, T2>()
			where T1 : class
			where T2 : class
		{
			Pairs.Add(typeof(T1),typeof(T2));
		}

		public void RegistrateAsSingleton<T1, T2>()
			where T1 : class
			where T2 : class
		{
			SingletonPairs.Add(typeof(T1), typeof(T2));
		}
	}
}
