using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DIContainer;

namespace DIContainerTests
{
	public interface I {}
	public class A : I {}
	public class B1 : A
	{
		public B1 (C c)
		{

		}
	}
	public class B2 : A { }
	public class C {}
	public class D : C {}
	public class G : B2 {}
	public interface IGen1<T> { }
	public interface IGen2<T> { }
	public class Gen1<T> : IGen1<T>
	{
		public Gen1(IGen2<T> gen2)
		{

		}
	}
	public class Gen2<T> : IGen2<T> { }
	public class Gen3<T> : IGen2<int> { }

	public class Rec1 { public Rec1(Rec2 rec2) {} }
	public class Rec2 { public Rec2(Rec1 rec1) {} }


	[TestClass]
	public class ProviderTests
	{
		private DependenciesConfiguration config;
		private DependencyProvider provider;

		[TestInitialize]
		public void TestInitilize()
		{
			config = new DependenciesConfiguration();
		}

		[TestMethod]
		public void CorrectGenericCreationByValidator()
		{
			config.Registrate(typeof(IGen1<>), typeof(Gen1<>));
			config.Registrate(typeof(IGen2<>), typeof(Gen2<>));
			provider = new DependencyProvider(config);
			Assert.IsNotNull(provider.Resolve<IGen1<int>>(), "Generic creation error");
		}

		[TestMethod]
		public void CorrectCountOfCreatedObjectsByValidator()
		{
			int expected = 2;
			int actual;
			config.Registrate<A,B1>();
			config.Registrate<A, B2>();
			config.Registrate<C, D>();
			provider = new DependencyProvider(config);
			actual = ((List<A>)provider.ResolveAll<A>()).Count;
			Assert.AreEqual(expected, actual, "Wrong types count");
		}

		[TestMethod]
		public void CorrectTypeOfCreatedByProviderInstance()
		{
			config.Registrate<I, A>();
			config.Registrate<A, B2>();
			config.Registrate<B2, G>();
			provider = new DependencyProvider(config);
			Assert.IsTrue(provider.Resolve<I>() is G, "Wrong type");
		}

		[TestMethod]
		public void CorrectTypeOfCreatedByProviderInstanceWithAsSelfMode()
		{
			config.Registrate<A, A>();
			provider = new DependencyProvider(config);
			Assert.IsNotNull(provider.Resolve<A>(), "Wrong type");
		}

		[TestMethod]
		public void ProviderCantCreateWrongGeneric()
		{
			config.Registrate(typeof(IGen2<>), typeof(Gen3<>));
			provider = new DependencyProvider(config);
			Assert.IsNull(provider.Resolve<IGen2<string>>(), "Wrong generic created");
		}

		[TestMethod]
		public void ProviderCreatesGenericWithSameAttr()
		{
			config.Registrate(typeof(IGen2<>), typeof(Gen3<>));
			provider = new DependencyProvider(config);
			Assert.IsNotNull(provider.Resolve<IGen2<int>>(), "Generic creation error");
		}

		[TestMethod]
		public void ValidatorSpotsRecursion()
		{
			config.Registrate<Rec1, Rec1>();
			config.Registrate<Rec2, Rec2>();
			try
			{
				new DependencyProvider(config);
				Assert.Fail("Validator confirm wrong config");
			}
			catch (ArgumentException e)
			{
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		[TestMethod]
		public void ProviderCreatesSinltoneObjects()
		{
			I I1, I2;
			config.RegistrateAsSingleton<I, A>();
			config.Registrate<A, B2>();
			config.Registrate<B2, G>();
			provider = new DependencyProvider(config);
			I1 = provider.Resolve<I>();
			I2 = provider.Resolve<I>();
			Assert.AreEqual(I1, I2, "Provider creates different objects");
		}

		[TestMethod]
		public void CorrectGenericSingletonObjectCreation()
		{
			IGen1<int> I1, I2;
			config.RegistrateAsSingleton(typeof(IGen1<>), typeof(Gen1<>));
			config.RegistrateAsSingleton(typeof(IGen2<>), typeof(Gen2<>));
			provider = new DependencyProvider(config);
			I1 = provider.Resolve<IGen1<int>>();
			I2 = provider.Resolve<IGen1<int>>();
			Assert.AreEqual(I1, I2, "Provider creates different objects");
		}
	}
}
