namespace GenericOutbox;

public class OutboxServiceLocator : IOutboxServiceLocator
{
    private static readonly AsyncLocal<ServiceProviderHolder> s_serviceProviderCurrent = new AsyncLocal<ServiceProviderHolder>();

    public IServiceProvider? ServiceProvider
    {
        get
        {
            return s_serviceProviderCurrent.Value?.ServiceProvider;
        }
        set
        {
            var holder = s_serviceProviderCurrent.Value;

            if (holder != null)
                holder.ServiceProvider = null;

            if (value != null)
                s_serviceProviderCurrent.Value = new ServiceProviderHolder { ServiceProvider = value };
        }
    }

    private sealed class ServiceProviderHolder
    {
        public IServiceProvider? ServiceProvider;
    }
}
