namespace UnityServiceLocator
{
	public class ServiceLookup
	{
		public ServiceLookup Get<T>(out T service)
		{
			service = ServiceLocator.Get<T>();
			return this;
		}
		
		public ServiceLookup TryGet<T>(out T service)
		{
			service = ServiceLocator.TryGet<T>();
			return this;
		}

		public ServiceLookup Done()
		{
			return this;
		}
	}
}
