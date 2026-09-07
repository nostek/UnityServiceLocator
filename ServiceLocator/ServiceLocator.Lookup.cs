namespace UnityServiceLocator
{
	public static partial class ServiceLocator
	{
		public class Lookup
		{
			public Lookup Get<T>(out T service)
			{
				service = ServiceLocator.Get<T>();
				return this;
			}

			public Lookup GetOrDefault<T>(out T service)
			{
				service = ServiceLocator.GetOrDefault<T>();
				return this;
			}

			public Lookup Done()
			{
				return this;
			}
		}
	}
}
