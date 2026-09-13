using app.Models;
using app.Repositories;
using app.Requests;
using app.Services;
using Moq;
using BC = BCrypt.Net.BCrypt;

namespace Vinylium.UnitTests.Services;

[TestFixture]
public class UserServiceTests{
	private const string Password = "vinylium123";
	private const string Email = "strale@vinylium.rs";

	private Mock<IUserRepository> _userRepository = null!;
	private Mock<IOrderService> _orderService = null!;
	private UserService _sut = null!;

	[SetUp]
	public void SetUp(){
		_userRepository = new Mock<IUserRepository>();
		_orderService = new Mock<IOrderService>();
		_sut = new UserService(_userRepository.Object, _orderService.Object);
	}

	private static RegisterReq RegisterReq(string email = Email, string username = "strale"){
		return new RegisterReq{ Username = username, Email = email, Password = Password };
	}

	private User ExistingUser(Guid? id = null, string password = Password){
		var user = TestData.NewUser(id, Email, hashedPassword: BC.HashPassword(password, BC.GenerateSalt()));
		_userRepository.Setup(r => r.FindUserByIdAsync(user.Id)).ReturnsAsync(user);
		return user;
	}

	[Test]
	public async Task Register_StoresHashedPasswordNeverThePlaintext(){
		User? stored = null;
		_userRepository.Setup(r => r.RegisterUserAsync(It.IsAny<User>()))
			.Callback((User u) => stored = u)
			.Returns(Task.CompletedTask);

		await _sut.RegisterUserAsync(RegisterReq());

		Assert.That(stored, Is.Not.Null);
		Assert.Multiple(() => {
			Assert.That(stored!.Password, Is.Not.EqualTo(Password));
			Assert.That(BC.Verify(Password, stored.Password), Is.True);
			Assert.That(stored.Admin, Is.False, "registration must never hand out admin rights");
		});
	}

	[Test]
	public async Task Register_BackfillsGuestOrdersForThatEmail(){
		User? stored = null;
		_userRepository.Setup(r => r.RegisterUserAsync(It.IsAny<User>()))
			.Callback((User u) => stored = u)
			.Returns(Task.CompletedTask);

		await _sut.RegisterUserAsync(RegisterReq());

		_orderService.Verify(o => o.BackfillGuestOrdersAsync(stored!.Id, Email), Times.Once);
	}

	[Test]
	public async Task Register_BackfillFailure_DoesNotFailRegistration(){
		_orderService.Setup(o => o.BackfillGuestOrdersAsync(It.IsAny<Guid>(), It.IsAny<string>()))
			.ThrowsAsync(new Exception("db down"));

		var user = await _sut.RegisterUserAsync(RegisterReq());

		Assert.That(user.Email, Is.EqualTo(Email));
		_userRepository.Verify(r => r.RegisterUserAsync(It.IsAny<User>()), Times.Once);
	}

	[Test]
	public void Login_UnknownUser_Throws(){
		_userRepository.Setup(r => r.FindUserByEmailOrUsernameAsync("nobody")).ReturnsAsync((User?)null);

		var ex = Assert.ThrowsAsync<Exception>(() =>
			_sut.LoginUserAsync(new LoginReq{ EmailOrUsername = "nobody", Password = Password }));

		Assert.That(ex!.Message, Is.EqualTo("User nobody does not exist"));
	}

	[Test]
	public void Login_WrongPassword_Throws(){
		var user = TestData.NewUser(hashedPassword: BC.HashPassword(Password, BC.GenerateSalt()));
		_userRepository.Setup(r => r.FindUserByEmailOrUsernameAsync("strale")).ReturnsAsync(user);

		var ex = Assert.ThrowsAsync<Exception>(() =>
			_sut.LoginUserAsync(new LoginReq{ EmailOrUsername = "strale", Password = "wrong-password" }));

		Assert.That(ex!.Message, Is.EqualTo("Incorrect password"));
	}

	[Test]
	public async Task Login_CorrectPassword_ReturnsUser(){
		var user = TestData.NewUser(hashedPassword: BC.HashPassword(Password, BC.GenerateSalt()));
		_userRepository.Setup(r => r.FindUserByEmailOrUsernameAsync("strale")).ReturnsAsync(user);

		var loggedIn = await _sut.LoginUserAsync(new LoginReq{ EmailOrUsername = "strale", Password = Password });

		Assert.That(loggedIn.Id, Is.EqualTo(user.Id));
	}

	[Test]
	public void UpdateEmail_WrongPassword_ThrowsWithoutWriting(){
		var user = ExistingUser();

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.UpdateEmailAsync(user.Id, "new@vinylium.rs", "wrong"));

		Assert.That(ex!.Message, Is.EqualTo("Incorrect password"));
		_userRepository.Verify(r => r.UpdateEmailAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
	}

	[Test]
	public void UpdateEmail_UnknownUser_Throws(){
		var id = Guid.CreateVersion7();
		_userRepository.Setup(r => r.FindUserByIdAsync(id)).ReturnsAsync((User?)null);

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.UpdateEmailAsync(id, "new@vinylium.rs", Password));

		Assert.That(ex!.Message, Is.EqualTo($"User with id {id} not found"));
	}

	[Test]
	public async Task UpdateEmail_CorrectPassword_UpdatesAndBackfillsUnderNewAddress(){
		var user = ExistingUser();
		var updated = TestData.NewUser(user.Id, "new@vinylium.rs");
		_userRepository.Setup(r => r.UpdateEmailAsync(user.Id, "new@vinylium.rs")).ReturnsAsync(updated);

		var result = await _sut.UpdateEmailAsync(user.Id, "new@vinylium.rs", Password);

		Assert.That(result.Email, Is.EqualTo("new@vinylium.rs"));
		_orderService.Verify(o => o.BackfillGuestOrdersAsync(user.Id, "new@vinylium.rs"), Times.Once);
	}

	[Test]
	public void UpdatePassword_WrongOldPassword_ThrowsWithoutWriting(){
		var user = ExistingUser();

		var ex = Assert.ThrowsAsync<Exception>(() => _sut.UpdatePasswordAsync(user.Id, "wrong", "brand-new-pass"));

		Assert.That(ex!.Message, Is.EqualTo("Incorrect password"));
		_userRepository.Verify(r => r.UpdatePasswordAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
	}

	[Test]
	public async Task UpdatePassword_CorrectOldPassword_StoresHashOfTheNewOne(){
		var user = ExistingUser();
		string? storedHash = null;
		_userRepository.Setup(r => r.UpdatePasswordAsync(user.Id, It.IsAny<string>()))
			.Callback((Guid _, string hash) => storedHash = hash)
			.ReturnsAsync(user);

		await _sut.UpdatePasswordAsync(user.Id, Password, "brand-new-pass");

		Assert.That(storedHash, Is.Not.Null);
		Assert.Multiple(() => {
			Assert.That(storedHash, Is.Not.EqualTo("brand-new-pass"));
			Assert.That(BC.Verify("brand-new-pass", storedHash!), Is.True);
			Assert.That(BC.Verify(Password, storedHash!), Is.False);
		});
	}
}
