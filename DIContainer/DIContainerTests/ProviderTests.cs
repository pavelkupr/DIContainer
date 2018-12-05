using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DIContainer;

namespace DIContainerTests
{
	public interface I {}
	public class A : I {}
	public class A2 : I { }
	public class B : A
	{
		public B (C c)
		{

		}
	}
	public class C {}
	public class D : C {}
	public interface IGen1<T> { }
	public interface IGen2<T> { }
	public class Gen2<T> : IGen2<T> { }
	public class Gen1<T> : IGen1<T>
	{
		public Gen1(IGen2<T> gen2)
		{

		}
	}

	[TestClass]
	public class ProviderTests
	{
		private static DependenciesConfiguration config;
		private static DependencyProvider provider;

		[TestInitialize]
		public void TestInitilize()
		{
			config = new DependenciesConfiguration();
		}

		[TestMethod]
		public void CorrectGenericCreation()
		{
			config.Registrate(typeof(IGen1<>), typeof(Gen1<>));
			config.Registrate(typeof(IGen2<>), typeof(Gen2<>));
			provider = new DependencyProvider(config);
			Assert.IsNotNull(provider.Resolve<IGen1<int>>(), "Generic creation error");
		}
	}
}
