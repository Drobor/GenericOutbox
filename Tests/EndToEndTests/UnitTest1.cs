using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonService.Client;
using SchoolService.DataAccess;

namespace EndToEndTests;

public class UnitTest1 : DependencyInjectionTestBase
{
    private readonly IPersonClient _personClient;
    private readonly SchoolServiceDbContext _schoolServiceDbContext;

    public UnitTest1()
    {
        _personClient = ServiceProvider.GetRequiredService<IPersonClient>();
        _schoolServiceDbContext = SchoolServiceProvider.GetRequiredService<SchoolServiceDbContext>();
    }

    [Fact]
    public async Task GuaranteedDelivery()
    {
        for (int i = 0; i < 100; i++)
        {
            var newPerson = await _personClient.PersonAsync();
            await _personClient.BecomeStudentAsync(newPerson.Id);

            var studentId = await Wait.ForNotDefault(
                3000,
                10,
                async () => await _schoolServiceDbContext.Students.Where(x => x.PersonId == newPerson.Id).Select(x => x.Id).FirstOrDefaultAsync());

            Assert.NotEqual(0, studentId);
        }
    }
}