using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using PersonService.DataAccess;
using PersonService.DataAccess.Entities;
using PersonService.Services;
using SchoolService.Client;

namespace PersonService.Controllers;

[ApiController]
[Route("[controller]")]
public class PersonController : ControllerBase
{
    private readonly ILogger<PersonController> _logger;
    private readonly PersonServiceDbContext _dbContext;
    private readonly IOutboxedSchoolClient _schoolClient;

    public PersonController(ILogger<PersonController> logger, PersonServiceDbContext dbContext, IOutboxedSchoolClient schoolClient)
    {
        _logger = logger;
        _dbContext = dbContext;
        _schoolClient = schoolClient;
    }

    [HttpPost]
    public async Task<ActionResult<Person>> Post()
    {
        var newPerson = new Person();
        _dbContext.Persons.Add(newPerson);
        await _dbContext.SaveChangesAsync();
        return newPerson;
    }

    [HttpPost("BecomeStudent/{personId:int}")]
    [ProducesResponseType(202)]
    public async Task<ActionResult> BecomeStudent([BindRequired] [FromRoute] int personId)
    {
        if (!await _dbContext.Persons.AnyAsync(x => x.Id == personId))
            return NotFound();

        await _schoolClient.StudentsAsync(new NewStudentRequest { PersonId = personId });

        return Accepted();
    }
}