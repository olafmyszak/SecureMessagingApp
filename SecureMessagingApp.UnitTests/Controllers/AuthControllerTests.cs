﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SecureMessagingApp.Controllers;
using SecureMessagingApp.Dtos;
using SecureMessagingApp.Models;
using SecureMessagingApp.Services;

namespace SecureMessagingApp.UnitTests.Controllers;

[TestFixture]
[TestOf(typeof(AuthController))]
public class AuthControllerTests
{
    private Mock<UserManager<User>> _userManagerMock;
    private Mock<ITokenService> _tokenServiceMock;
    private AuthController _authController;

    [SetUp]
    public void Setup()
    {
        _userManagerMock = MockUserManager<User>([]);
        _tokenServiceMock = new Mock<ITokenService>();
        _authController = new AuthController(_userManagerMock.Object, _tokenServiceMock.Object);
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
        ActionResult<User> actionResult = await _authController.Register(dto);

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

        const string expected = "Username already exists.";

        // Act
        ActionResult<User> actionResult = await _authController.Register(dto);

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
        ActionResult<User> actionResult = await _authController.Register(dto);

        // Assert
        var badRequestObjectResult = actionResult.Result as BadRequestObjectResult;
        Assert.That(badRequestObjectResult, Is.Not.Null);

        var actual = badRequestObjectResult.Value as IEnumerable<IdentityError>;
        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var dto = new UserLoginDto { UserName = "user", Password = "password" };
        var user = new User();
        const string expected = "jwtToken";

        _userManagerMock.Setup(x => x.FindByNameAsync(dto.UserName))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, dto.Password))
            .ReturnsAsync(true);
        _tokenServiceMock.Setup(x => x.GenerateJwtToken(user))
            .Returns(expected);

        // Act
        ActionResult<string> actionResult = await _authController.Login(dto);

        // Assert
        var okObjectResult = actionResult.Result as OkObjectResult;
        Assert.That(okObjectResult, Is.Not.Null);

        string? actual = okObjectResult.Value as string;
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task Login_UsernameNotExists_ReturnsUnauthorized()
    {
        // Arrange
        var dto = new UserLoginDto { UserName = "badUser", Password = "password" };
        var user = new User();
        const string expected = "Incorrect username or password";

        _userManagerMock.Setup(x => x.FindByNameAsync(dto.UserName))
            .ReturnsAsync(null as User);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, dto.Password))
            .ReturnsAsync(true);
        _tokenServiceMock.Setup(x => x.GenerateJwtToken(user))
            .Returns(expected);

        // Act
        ActionResult<string> actionResult = await _authController.Login(dto);

        // Assert
        var unauthorizedObjectResult = actionResult.Result as UnauthorizedObjectResult;
        Assert.That(unauthorizedObjectResult, Is.Not.Null);

        string? actual = unauthorizedObjectResult.Value as string;
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        var dto = new UserLoginDto { UserName = "badUser", Password = "password" };
        var user = new User();
        const string expected = "Incorrect username or password";

        _userManagerMock.Setup(x => x.FindByNameAsync(dto.UserName))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, dto.Password))
            .ReturnsAsync(false);
        _tokenServiceMock.Setup(x => x.GenerateJwtToken(user))
            .Returns(expected);

        // Act
        ActionResult<string> actionResult = await _authController.Login(dto);

        // Assert
        var unauthorizedObjectResult = actionResult.Result as UnauthorizedObjectResult;
        Assert.That(unauthorizedObjectResult, Is.Not.Null);

        string? actual = unauthorizedObjectResult.Value as string;
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.EqualTo(expected));
    }

    private static Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        mgr.Object.UserValidators.Add(new UserValidator<TUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

        mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
        mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success)
            .Callback<TUser, string>((x, y) => ls.Add(x));
        mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);

        return mgr;
    }
}