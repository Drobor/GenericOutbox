using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public abstract class DependencyInjectionTestBase : IDisposable
{
    protected readonly IServiceProvider ServiceProvider;

    private readonly List<IDisposable> _disposables;

    private bool _disposed = false;

    protected DependencyInjectionTestBase()
    {
        _disposables = new List<IDisposable>();

        ServiceProvider = CreateScopedServiceProvider(TestConfiguration.TestServiceProvider);
    }

    private IServiceProvider CreateScopedServiceProvider(IServiceProvider rootServiceProvider)
    {
        var scope = rootServiceProvider.CreateScope();
        _disposables.Add(scope);
        return scope.ServiceProvider;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var disposable in _disposables)
                disposable.Dispose();

            _disposed = true;
        }
    }

    ~DependencyInjectionTestBase()
    {
        Dispose();
    }
}