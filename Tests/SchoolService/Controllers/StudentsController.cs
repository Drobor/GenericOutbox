using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolService.DataAccess;
using SchoolService.DataAccess.Entities;

namespace SchoolService.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentsController : ControllerBase
{
    private readonly ILogger<StudentsController> _logger;
    private readonly SchoolServiceDbContext _dbContext;

    public StudentsController(ILogger<StudentsController> logger, SchoolServiceDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<Student>> Post([FromBody][BindRequired] NewStudentRequest request)
    {
        var newStudent = new Student
        {
            PersonId = request.PersonId,
        };

        _dbContext.Students.Add(newStudent);
        await _dbContext.SaveChangesAsync();
        return newStudent;
    }
}

public class NewStudentRequest
{
    [BindRequired]
    public int PersonId { get; set; }
}
