using AutoMapper;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OtusSocialNetwork.Database;
using OtusSocialNetwork.Database.Entities;
using OtusSocialNetwork.DataClasses.Dtos;
using OtusSocialNetwork.DataClasses.Requests;
using OtusSocialNetwork.DataClasses.Responses;
using OtusSocialNetwork.Services;

namespace OtusSocialNetwork.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UserController: ControllerBase
{
    private readonly IDatabaseContext _db;
    private readonly IMapper _mapper;
    private readonly IPasswordService _pass;
    public UserController(IDatabaseContext db, IMapper mapper, IPasswordService pass)
    {
        _db = db;
        _mapper = mapper;
        _pass = pass;
    }
    [AllowAnonymous]
    [HttpPost("user/register")]
    public async Task<IActionResult> Register([FromBody] RegisterReq data)
    {
        var user = _mapper.Map<UserEntity>(data);

        var password = _pass.HashPassword(data.Password);

        var dbRes = await _db.RegisterAsync(user, password);
        if (dbRes.isSuccess)
        {
            return Ok(new RegisterRes(dbRes.userId));
        }
        return BadRequest();
    }

    [Authorize]
    [HttpGet("user/get/{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var dbRes = await _db.GetUserAsync(id.ToString());
        if (!dbRes.isSuccess) return BadRequest(new ErrorRes(dbRes.msg));

        var res = _mapper.Map<UserDto>(dbRes.user);
        return Ok(res);
    }

    [Authorize]
    [HttpGet("user/search")]
    public async Task<IActionResult> Search(SearchReq query)
    {
        var dbRes = await _db.SearchUserAsync(query.FirstName, query.LastName);
        if (!dbRes.isSuccess) return BadRequest(new ErrorRes(dbRes.msg));

        var res = _mapper.Map<List<UserDto>>(dbRes.users);
        return Ok(res);
    }

}
