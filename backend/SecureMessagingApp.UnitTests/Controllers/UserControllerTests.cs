using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using MockQueryable;
using Moq;
using NUnit.Framework.Legacy;
using SecureMessagingApp.Controllers;
using SecureMessagingApp.Dtos;
using SecureMessagingApp.Models;
using SecureMessagingApp.Services;

namespace SecureMessagingApp.UnitTests.Controllers;

[TestFixture]
[TestOf(typeof(UserController))]
public class UserControllerTests
{
    private Mock<UserManager<User>> _userManagerMock;
    private UserController _userController;

    [SetUp]
    public void Setup()
    {
        _userManagerMock = MockUserManager<User>([]);
        new Mock<ITokenService>();
        _userController = new UserController(_userManagerMock.Object);
    }

    [TestCase("user", "password")]
    [TestCase("user2", "Asd@21q12dąą..___@@!@#$%^&*()[]]\\/?..<`~")]
    public async Task Register_ValidUser_ReturnsEmptyCreated(string username, string password)
    {
        // Arrange
        var dto = new UserRegistrationDto { UserName = username, Password = password };
        _userManagerMock.Setup(manager => manager.FindByNameAsync(dto.UserName))
            .ReturnsAsync(null as User);
        _userManagerMock.Setup(manager => manager.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);


        // Act
        ActionResult<User> actionResult = await _userController.Register(dto);

        // Assert
        var createdResult = actionResult.Result as CreatedResult;
        Assert.That(createdResult, Is.Not.Null);

        string? actual = createdResult.Value as string;
        Assert.That(actual, Is.Null);
    }

    [Test]
    public async Task Register_UsernameExists_ReturnsConflict()
    {
        // Arrange
        var dto = new UserRegistrationDto { UserName = "user", Password = "password" };
        _userManagerMock.Setup(manager => manager.FindByNameAsync(dto.UserName))
            .ReturnsAsync(new User());
        _userManagerMock.Setup(manager => manager.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());

        const string expected = "UserName already exists.";

        // Act
        ActionResult<User> actionResult = await _userController.Register(dto);

        // Assert
        var conflictObjectResult = actionResult.Result as ConflictObjectResult;
        Assert.That(conflictObjectResult, Is.Not.Null);

        string? actual = conflictObjectResult.Value as string;
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task Register_UserManagerError_ReturnsBadRequest()
    {
        // Arrange
        var dto = new UserRegistrationDto { UserName = "user", Password = "password" };
        _userManagerMock.Setup(manager => manager.FindByNameAsync(dto.UserName))
            .ReturnsAsync(null as User);
        _userManagerMock.Setup(manager => manager.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());

        // Act
        ActionResult<User> actionResult = await _userController.Register(dto);

        // Assert
        var badRequestObjectResult = actionResult.Result as BadRequestObjectResult;
        Assert.That(badRequestObjectResult, Is.Not.Null);

        var actual = badRequestObjectResult.Value as IEnumerable<IdentityError>;
        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    public async Task GetAllUsers_Always_ReturnsListOfUserIdWithUsername()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, UserName = "Alice" },
            new() { Id = 2, UserName = "Bob" }
        };

        var mock = users.BuildMock();
        _userManagerMock.Setup(manager => manager.Users).Returns(mock);

        //Act
        ActionResult<List<UserIdWithUsernameDto>> actionResult = await _userController.GetAllUsers();

        // Assert
        var okObjectResult = actionResult.Result as OkObjectResult;
        Assert.That(okObjectResult, Is.Not.Null);


        var actual = okObjectResult.Value as List<UserIdWithUsernameDto>;
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.All.Not.Null);
        Assert.That(actual, Has.Count.EqualTo(users.Count));

        List<string> actualUsernames = actual.Select(u => u.UserName).ToList();
        List<string?> expectedUsernames = users.Select(u => u.UserName).ToList();
        Assert.That(actualUsernames, Is.EquivalentTo(expectedUsernames));
    }

    private static Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        mgr.Object.UserValidators.Add(new UserValidator<TUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

        mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
        mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success)
            .Callback<TUser, string>((x, _) => ls.Add(x));
        mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);

        return mgr;
    }
}