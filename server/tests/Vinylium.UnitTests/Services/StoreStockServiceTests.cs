using app.Models;
using app.Repositories;
using app.Services;
using Moq;
using Newtonsoft.Json.Linq;

namespace Vinylium.UnitTests.Services;

[TestFixture]
public class StoreStockServiceTests{
	private const string Barcode = "5099969944123";

	private Mock<IStoreStockRepository> _repository = null!;
	private StoreStockService _sut = null!;
	private Store _store = null!;

	[SetUp]
	public void SetUp(){
		_repository = new Mock<IStoreStockRepository>();
		_sut = new StoreStockService(_repository.Object);
		_store = TestData.NewStore();
	}

	private static JArray Json(params StoreStock[] stock){
		return JArray.FromObject(stock);
	}

	[Test]
	public void CreateStoreStockFromJson_NegativeQuantity_Throws(){
		var json = Json(TestData.NewStockWithStore(Barcode, _store, -1));

		var ex = Assert.Throws<Exception>(() => _sut.CreateStoreStockFromJson(json, TestData.NewProduct()));

		Assert.That(ex!.Message, Is.EqualTo("Quantity cant be negative"));
	}

	[Test]
	public void CreateStoreStockFromJson_EverythingZero_MarksProductOutOfStock(){
		var product = TestData.NewProduct(inStock: true);
		var json = Json(TestData.NewStockWithStore(Barcode, _store, 0),
			TestData.NewStockWithStore(Barcode, TestData.NewStore(), 0));

		var stock = _sut.CreateStoreStockFromJson(json, product);

		Assert.That(product.InStock, Is.False);
		Assert.That(stock, Has.Count.EqualTo(2), "zero-quantity rows are still stored, only the flag flips");
	}

	[Test]
	public void CreateStoreStockFromJson_AnyQuantityLeft_MarksProductInStock(){
		var product = TestData.NewProduct(inStock: false);
		var json = Json(TestData.NewStockWithStore(Barcode, _store, 0),
			TestData.NewStockWithStore(Barcode, TestData.NewStore(), 2));

		_sut.CreateStoreStockFromJson(json, product);

		Assert.That(product.InStock, Is.True);
	}

	[Test]
	public void CreateStoreStockFromJson_UsesProductBarcodeAndNestedStoreId(){
		var json = Json(TestData.NewStockWithStore("stale-barcode", _store, 7));

		var stock = _sut.CreateStoreStockFromJson(json, TestData.NewProduct(Barcode));

		Assert.That(stock, Has.Count.EqualTo(1));
		Assert.Multiple(() => {
			Assert.That(stock[0].ProductBarcode, Is.EqualTo(Barcode));
			Assert.That(stock[0].StoreId, Is.EqualTo(_store.Id));
			Assert.That(stock[0].Quantity, Is.EqualTo(7));
		});
	}

	[Test]
	public void CreateStoreStockFromJson_MalformedJson_Throws(){
		Assert.Throws<Newtonsoft.Json.JsonReaderException>(() =>
			_sut.CreateStoreStockFromJson("not json", TestData.NewProduct()));
	}

	[Test]
	public async Task MoveToWarehouse_DelegatesToRepository(){
		var from = Guid.CreateVersion7();
		var warehouse = Guid.CreateVersion7();

		await _sut.MoveToWarehouse(from, warehouse);

		_repository.Verify(r => r.MoveToWarehouse(from, warehouse), Times.Once);
	}

	[Test]
	public async Task DecrementStock_DelegatesToRepository(){
		var storeId = Guid.CreateVersion7();

		await _sut.DecrementStockAsync(storeId, Barcode, 3);

		_repository.Verify(r => r.DecrementStockAsync(storeId, Barcode, 3), Times.Once);
	}
}
