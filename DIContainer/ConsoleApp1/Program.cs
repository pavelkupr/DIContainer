using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DIContainer;
namespace ConsoleApp1
{
	interface A { }
	class B : A { public B(C c) { } }
	class C { }
	class D : C { }
	class Program
	{
		static void Main(string[] args)
		{
			DependenciesConfiguration config = new DependenciesConfiguration();
			config.Registrate<A, B>();
			config.Registrate<C, D>();
			DependencyProvider p = new DependencyProvider(config);
			A i = p.Resolve<A>();
			Console.Read();
		}
	}
}
