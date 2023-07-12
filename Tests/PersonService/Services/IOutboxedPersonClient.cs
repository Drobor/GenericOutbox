using GenericOutbox;
using SchoolService.Client;

namespace PersonService.Services;

[OutboxInterface]
public interface IOutboxedSchoolClient : ISchoolClient
{
    
}