using GenericOutbox;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Services;

public class KeyStorageServiceHelper : IKeyStorageServiceHelper
{
    private readonly Dictionary<string, string>? _dictionary;
    private readonly IKeyStorageService _keyStorageService;

    public KeyStorageServiceHelper(IKeyStorageService keyStorageService, IOutboxServiceLocator outboxServiceLocator)
    {
        _dictionary = outboxServiceLocator.ServiceProvider?.GetRequiredService<Dictionary<string,string>>();
        _keyStorageService = keyStorageService;
    }

    public void AddUsingScope(string key)
    {
        _dictionary[key] = null;
        _keyStorageService.CopyFromScopedDictionary();
    }
}
