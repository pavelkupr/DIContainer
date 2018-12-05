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
	class G : A { }
	class C { }
	class D : C { }
	class Program
	{
		static void Main(string[] args)
		{
			DependenciesConfiguration config = new DependenciesConfiguration();
			config.Registrate<A, B>();
			config.Registrate<A, G>();
			config.Registrate<C, D>();
			DependencyProvider p = new DependencyProvider(config);
			List<A> i = (List<A>)p.ResolveAll<A>();
			Console.Read();
		}
	}
}
