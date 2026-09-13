using app.Models;
using app.Repositories;
using app.Requests;
using app.Services;
using Moq;

namespace app.UnitTest.Services;

[TestFixture]
public class StoreServiceTests{
	private Mock<IStoreRepository> _storeRepository = null!;
	private Mock<IStoreStockService> _storeStockService = null!;
	private Mock<IUnitOfWork> _unitOfWork = null!;
	private StoreService _sut = null!;

	[SetUp]
	public void SetUp(){
		_storeRepository = new Mock<IStoreRepository>();
		_storeStockService = new Mock<IStoreStockService>();
		_unitOfWork = new Mock<IUnitOfWork>();

		_unitOfWork.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
			.Returns<Func<Task>>(action => action());

		_storeRepository.Setup(r => r.CreateStoreAsync(It.IsAny<Store>()))
			.ReturnsAsync((Store s) => s);
		_storeRepository.Setup(r => r.UpdateStoreAsync(It.IsAny<Guid>(), It.IsAny<Store>()))
			.ReturnsAsync((Guid _, Store s) => s);

		_sut = new StoreService(_storeRepository.Object, _storeStockService.Object, _unitOfWork.Object);
	}

	private static AddStoreReq AddReq(string name = "Vinylium Niš", bool isWarehouse = false,
		string openingHours = "09:00", string closingHours = "21:00"){
		return new AddStoreReq{
			Name = name,
			Address = "Obrenovićeva 10",
			City = "Niš",
			ContactNumber = "0601234567",
			OpeningHours = openingHours,
			ClosingHours = closingHours,
			IsWarehouse = isWarehouse
		};
	}

	private static UpdateStoreReq UpdateReq(Guid id, string name = "Vinylium Niš", bool isWarehouse = false,
		string openingHours = "09:00", string closingHours = "21:00"){
		return new UpdateStoreReq{
			Id = id,
			Name = name,
			Address = "Obrenovićeva 10",
			City = "Niš",
			ContactNumber = "0601234567",
			OpeningHours = openingHours,
			ClosingHours = closingHours,
			IsWarehouse = isWarehouse
		};
	}

	[Test]
	public async Task CreateStore_Warehouse_AppendsSuffixToName(){
		var store = await _sut.CreateStoreAsync(AddReq("Central", isWarehouse: true), []);

		Assert.That(store.Name, Is.EqualTo("Central Warehouse"));
		Assert.That(store.IsWarehouse, Is.True);
	}

	[Test]
	public async Task CreateStore_RegularStore_KeepsNameAsGiven(){
		var store = await _sut.CreateStoreAsync(AddReq("Central"), []);

		Assert.That(store.Name, Is.EqualTo("Central"));
		Assert.That(store.IsWarehouse, Is.False);
	}

	[Test]
	public async Task CreateStore_ParsesWorkingHours(){
		var store = await _sut.CreateStoreAsync(AddReq(openingHours: "09:30", closingHours: "21:15"), []);

		Assert.Multiple(() => {
			Assert.That(store.OpeningTime, Is.EqualTo(new TimeOnly(9, 30)));
			Assert.That(store.ClosingTime, Is.EqualTo(new TimeOnly(21, 15)));
		});
	}

	[TestCase("9:30")]
	[TestCase("25:00")]
	[TestCase("09:30:00")]
	[TestCase("")]
	public void CreateStore_MalformedWorkingHours_ThrowsBeforeWriting(string openingHours){
		Assert.ThrowsAsync<FormatException>(() => _sut.CreateStoreAsync(AddReq(openingHours: openingHours), []));

		_unitOfWork.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()), Times.Never);
		_storeRepository.Verify(r => r.CreateStoreAsync(It.IsAny<Store>()), Times.Never);
	}

	[Test]
	public async Task CreateStore_SeedsStockForEveryProduct(){
		List<string> barcodes = ["111", "222"];

		var store = await _sut.CreateStoreAsync(AddReq(), barcodes);

		_storeStockService.Verify(s => s.CreateStockForNewStore(store.Id, barcodes), Times.Once);
		_unitOfWork.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()), Times.Once);
	}

	[Test]
	public async Task DeleteStore_RegularStore_MovesStockToWarehouseBeforeDeleting(){
		var storeId = Guid.CreateVersion7();
		var warehouseId = Guid.CreateVersion7();
		_storeRepository.Setup(r => r.GetStoreByIdAsync(storeId)).ReturnsAsync(TestData.NewStore(storeId));
		_storeRepository.Setup(r => r.GetWarehouseId()).ReturnsAsync(warehouseId);

		var calls = new List<string>();
		_storeStockService.Setup(s => s.MoveToWarehouse(storeId, warehouseId))
			.Callback(() => calls.Add("move")).Returns(Task.CompletedTask);
		_storeRepository.Setup(r => r.DeleteStoreAsync(storeId))
			.Callback(() => calls.Add("delete")).Returns(Task.CompletedTask);

		await _sut.DeleteStoreAsync(storeId);

		Assert.That(calls, Is.EqualTo(new[]{ "move", "delete" }));
	}

	[Test]
	public async Task DeleteStore_Warehouse_DoesNotMoveStockToItself(){
		var warehouseId = Guid.CreateVersion7();
		_storeRepository.Setup(r => r.GetStoreByIdAsync(warehouseId))
			.ReturnsAsync(TestData.NewStore(warehouseId, "Central Warehouse", isWarehouse: true));
		_storeRepository.Setup(r => r.GetWarehouseId()).ReturnsAsync(warehouseId);

		await _sut.DeleteStoreAsync(warehouseId);

		_storeStockService.Verify(s => s.MoveToWarehouse(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
		_storeRepository.Verify(r => r.DeleteStoreAsync(warehouseId), Times.Once);
	}

	[Test]
	public async Task DeleteStore_NoWarehouseExists_DeletesWithoutMovingStock(){
		var storeId = Guid.CreateVersion7();
		_storeRepository.Setup(r => r.GetStoreByIdAsync(storeId)).ReturnsAsync(TestData.NewStore(storeId));
		_storeRepository.Setup(r => r.GetWarehouseId()).ReturnsAsync((Guid?)null);

		await _sut.DeleteStoreAsync(storeId);

		_storeStockService.Verify(s => s.MoveToWarehouse(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
		_storeRepository.Verify(r => r.DeleteStoreAsync(storeId), Times.Once);
	}

	[Test]
	public async Task UpdateStore_MapsRequestOntoStore(){
		var id = Guid.CreateVersion7();
		Store? passed = null;
		_storeRepository.Setup(r => r.UpdateStoreAsync(id, It.IsAny<Store>()))
			.Callback((Guid _, Store s) => passed = s)
			.ReturnsAsync((Guid _, Store s) => s);

		await _sut.UpdateStoreAsync(UpdateReq(id, "Vinylium Beograd", openingHours: "10:00", closingHours: "22:00"));

		Assert.That(passed, Is.Not.Null);
		Assert.Multiple(() => {
			Assert.That(passed!.Name, Is.EqualTo("Vinylium Beograd"));
			Assert.That(passed.City, Is.EqualTo("Niš"));
			Assert.That(passed.ContactNumber, Is.EqualTo("0601234567"));
			Assert.That(passed.OpeningTime, Is.EqualTo(new TimeOnly(10, 0)));
			Assert.That(passed.ClosingTime, Is.EqualTo(new TimeOnly(22, 0)));
		});
	}

	[TestCase(true)]
	[TestCase(false)]
	public async Task UpdateStore_PassesWarehouseFlagThroughUnchanged(bool isWarehouse){
		var id = Guid.CreateVersion7();
		Store? passed = null;
		_storeRepository.Setup(r => r.UpdateStoreAsync(id, It.IsAny<Store>()))
			.Callback((Guid _, Store s) => passed = s)
			.ReturnsAsync((Guid _, Store s) => s);

		await _sut.UpdateStoreAsync(UpdateReq(id, isWarehouse: isWarehouse));

		Assert.That(passed!.IsWarehouse, Is.EqualTo(isWarehouse));
	}

	[Test]
	public async Task UpdateStore_DoesNotAppendWarehouseSuffix(){
		var id = Guid.CreateVersion7();

		var updated = await _sut.UpdateStoreAsync(UpdateReq(id, "Central Warehouse", isWarehouse: true));

		Assert.That(updated.Name, Is.EqualTo("Central Warehouse"));
	}

	[TestCase("22:60")]
	[TestCase("noon")]
	public void UpdateStore_MalformedWorkingHours_ThrowsBeforeWriting(string closingHours){
		var id = Guid.CreateVersion7();

		Assert.ThrowsAsync<FormatException>(() => _sut.UpdateStoreAsync(UpdateReq(id, closingHours: closingHours)));

		_storeRepository.Verify(r => r.UpdateStoreAsync(It.IsAny<Guid>(), It.IsAny<Store>()), Times.Never);
	}

	[Test]
	public async Task GetFiltered_MapsArgumentsIntoFilter(){
		StoreFilterReq? passed = null;
		_storeRepository.Setup(r => r.GetFilteredAsync(It.IsAny<StoreFilterReq>()))
			.Callback((StoreFilterReq f) => passed = f)
			.ReturnsAsync((new List<Store>(), 0));

		await _sut.GetFilteredAsync(2, 10, "beatles", true);

		Assert.Multiple(() => {
			Assert.That(passed!.Page, Is.EqualTo(2));
			Assert.That(passed.PerPage, Is.EqualTo(10));
			Assert.That(passed.Search, Is.EqualTo("beatles"));
			Assert.That(passed.IsWarehouse, Is.True);
		});
	}

	[Test]
	public async Task HasWarehouse_DelegatesToRepository(){
		_storeRepository.Setup(r => r.HasWarehouse()).ReturnsAsync(true);

		Assert.That(await _sut.HasWarehouse(), Is.True);
	}
}
