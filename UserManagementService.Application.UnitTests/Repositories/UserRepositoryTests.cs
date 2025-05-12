using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using UserManagementService.Application.Dtos;
using UserManagementService.Application.Repositories.UserRepository;
using UserManagementService.Database;

namespace UserManagementService.Application.UnitTests.Repositories;

public class UserRepositoryTests
{
    private readonly UserManagementServiceDbContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<UserManagementServiceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new UserManagementServiceDbContext(options);
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(DateTime.Parse("2/16/2000 12:15:00 PM"));

        _repository = new UserRepository(_dbContext, _timeProvider);
    }

    [Test]
    public async Task GetUserByIdAsync_ShouldReturnCorrectUser_WhenUserRequestedById()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Name = "Tom", 
            Email = "tom@example.com"
        };
        var id = await _repository.AddUserAsync(dto);

        // Act
        var user = await _repository.GetUserByIdAsync(id);

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be("tom@example.com");
    }

    [Test]
    public async Task GetAllUsersAsync_ShouldReturnAllUsers_WhenAllUsersRequested()
    {
        // Arrange
        await _repository.AddUserAsync(new CreateUserDto { Name = "A", Email = "a@example.com" });
        await _repository.AddUserAsync(new CreateUserDto { Name = "B", Email = "b@example.com" });

        // Act
        var users = await _repository.GetAllUsersAsync();

        // Assert
        users.Should().HaveCount(2);
    }

    [Test]
    public async Task AddUserAsync_ShouldAddUser_WhenValidRequest()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Name = "John", 
            Email = "john@example.com"
        };

        // Act
        var id = await _repository.AddUserAsync(dto);

        // Assert
        var user = await _dbContext.Users.FindAsync(id);
        user.Should().NotBeNull();
        user.Email.Should().Be("john@example.com");
        user.CreatedAt.Should().Be(_timeProvider.GetUtcNow().DateTime);
    }

    [Test]
    public async Task AddUserAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Name = "Jane", 
            Email = "jane@example.com"
        };
        await _repository.AddUserAsync(dto);

        // Act
        Func<Task> act = async () => await _repository.AddUserAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User with this email already exists.");
    }

    [Test]
    public async Task AddUserAsyncWithUserId_ShouldAddNewUser_WhenValidRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new CreateUserDto
        {
            Name = "Test User",
            Email = "test@example.com"
        };

        // Act
        var id = await _repository.AddUserAsync(userId, dto);

        // Assert
        id.Should().Be(userId);

        var user = await _dbContext.Users.FindAsync(userId);
        user.Should().NotBeNull();
        user!.Name.Should().Be("Test User");
        user.Email.Should().Be("test@example.com");
        user.CreatedAt.Should().Be(_timeProvider.GetUtcNow().DateTime);
    }

    [Test]
    public async Task AddUserAsyncWithUserId_ShouldThrow_WhenEmailAlreadyExists()
    {
        // Arrange
        var existingUser = new CreateUserDto
        {
            Name = "Existing", 
            Email = "duplicate@example.com"
        };
        await _repository.AddUserAsync(Guid.NewGuid(), existingUser);

        var newUserId = Guid.NewGuid();
        var userDto = new CreateUserDto
        {
            Name = "New", 
            Email = "duplicate@example.com"
        };

        // Act
        Func<Task> act = async () => await _repository.AddUserAsync(newUserId, userDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User with this email already exists.");
    }

    [Test]
    public async Task AddUserAsyncWithUserId_ShouldThrow_WhenIdAlreadyExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto1 = new CreateUserDto
        {
            Name = "First", 
            Email = "first@example.com"
        };
        await _repository.AddUserAsync(userId, userDto1);

        var userDto2 = new CreateUserDto
        {
            Name = "Second", 
            Email = "second@example.com"
        };

        // Act
        Func<Task> act = async () => await _repository.AddUserAsync(userId, userDto2);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You already have a user created. You can update or delete your user.");
    }

    [Test]
    public async Task UpdateUserAsyncWithId_ShouldUpdateFields_WhenValidRequest()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Name = "Old", 
            Email = "old@example.com"
        };
        var id = await _repository.AddUserAsync(dto);

        var updateDto = new UpdateUserDto
        {
            Name = "New", 
            Email = "new@example.com"
        };

        // Act
        await _repository.UpdateUserAsync(id, updateDto);

        // Assert
        var user = await _dbContext.Users.FindAsync(id);
        user!.Name.Should().Be("New");
        user.Email.Should().Be("new@example.com");
        user.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().DateTime);
    }

    [Test]
    public async Task UpdateUserAsyncWithId_ShouldThrow_WhenWrongUserId()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var updateDto = new UpdateUserDto();

        // Act
        Func<Task> act = async () => await _repository.UpdateUserAsync(nonExistentUserId, updateDto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found, please create a user first.");
    }

    [Test]
    public async Task UpdateUserAsyncWithId_ShouldThrow_WhenEmailExists()
    {
        // Arrange
        var existingUser = new CreateUserDto
        {
            Name = "User1", 
            Email = "used@example.com"
        };
        var other = new CreateUserDto
        {
            Name = "User2", 
            Email = "unique@example.com"
        };
        
        await _repository.AddUserAsync(existingUser);
        var otherId = await _repository.AddUserAsync(other);

        var updateDto = new UpdateUserDto
        {
            Name = "Dup",
            Email = "used@example.com"
        };

        // Act
        Func<Task> act = async () => await _repository.UpdateUserAsync(otherId, updateDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User with this email already exists.");
    }

    [Test]
    public async Task UpdateUserAsync_ShouldUpdateFields_WhenValidRequest()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Name = "Old",
            Email = "old@example.com"
        };
        var id = await _repository.AddUserAsync(dto);

        var adminUpdateDto = new AdminUpdateUserDto
        {
            Id = id,
            Name = "New",
            Email = "new@example.com"
        };

        // Act
        await _repository.UpdateUserAsync(adminUpdateDto);

        // Assert
        var user = await _dbContext.Users.FindAsync(id);
        user!.Name.Should().Be("New");
        user.Email.Should().Be("new@example.com");
        user.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().DateTime);
    }

    [Test]
    public async Task UpdateUserAsync_ShouldThrow_WhenWrongUserId()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var adminUpdateDto = new AdminUpdateUserDto
        {
            Id = nonExistentUserId
        };

        // Act
        Func<Task> act = async () => await _repository.UpdateUserAsync(adminUpdateDto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    [Test]
    public async Task UpdateUserAsync_ShouldThrow_WhenEmailExists()
    {
        // Arrange
        var existingUser = new CreateUserDto
        {
            Name = "User1",
            Email = "used@example.com"
        };
        var other = new CreateUserDto
        {
            Name = "User2",
            Email = "unique@example.com"
        };

        await _repository.AddUserAsync(existingUser);
        var otherId = await _repository.AddUserAsync(other);

        var adminUpdateDto = new AdminUpdateUserDto
        {
            Id = otherId,
            Name = "Dup",
            Email = "used@example.com"
        };

        // Act
        Func<Task> act = async () => await _repository.UpdateUserAsync(adminUpdateDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User with this email already exists.");
    }

    [Test]
    public async Task DeleteUserAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.DeleteUserAsync(id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    [SetUp]
    protected void SetUp()
    {
        _dbContext.Database.EnsureDeleted();
    }

    [OneTimeTearDown]
    protected void TearDown()
    {
        _dbContext.Dispose();
    }
}
