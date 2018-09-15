using Autofac;
using WK.Info.Services;
using IContainer = Autofac.IContainer;

namespace WK.Info
{
	public static class ContainerAccessor
	{
		private static readonly IContainer _container;

		static ContainerAccessor()
		{
			_container = ConfigureServices();
		}

		public static IContainer Container => _container;

		private static IContainer ConfigureServices()
		{
			var builder = new ContainerBuilder();

			builder.RegisterType<DictionaryProvider>().As<IDictionaryProvider>().SingleInstance();

			return builder.Build();
		}
	}
}
